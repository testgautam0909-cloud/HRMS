import { Component, inject, signal, OnInit, ElementRef, ViewChild, AfterViewChecked, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../core/services/chat.service';
import { AuthService } from '../../core/services/auth.service';
import { ApiService } from '../../core/services/api.service';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatInputModule } from '@angular/material/input';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';

@Component({
    selector: 'app-chat',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        MatButtonModule,
        MatIconModule,
        MatDividerModule,
        MatInputModule,
        MatTooltipModule,
        MatCheckboxModule,
        EmptyStateComponent
    ],
    templateUrl: './chat.component.html',
    styleUrl: './chat.component.css'
})
export class ChatComponent implements OnInit, AfterViewChecked {
    @ViewChild('scrollContainer') private scrollContainer?: ElementRef;

    chatService = inject(ChatService);
    private auth = inject(AuthService);
    private api = inject(ApiService);

    currentUserId = this.auth.currentUser()?.employeeId;
    conversations = signal<any[]>([]);
    employees = signal<any[]>([]);
    activeTab = signal<'chats' | 'directory'>('chats');
    selectedConv = signal<any>(null);
    newMessage = '';
    searchQuery = signal('');
    conversationSearchQuery = signal('');
    showSearch = signal(false);
    showMembers = signal(false); // Toggle for group members list
    showAddMember = signal(false); // Toggle for adding member UI

    // Group creation state
    showGroupCreate = signal(false);
    groupName = '';
    groupDescription = '';
    selectedMembers = signal<Set<string>>(new Set());

    // --- Resolve conversation display name ---
    getConversationName(conv: any): string {
        if (!conv) return '';
        if (conv.type === 'Group' && conv.name) return conv.name;
        if (conv.members && conv.members.length > 0) {
            const other = conv.members.find((m: any) => m.employeeId !== this.currentUserId);
            if (other?.employeeName) return other.employeeName;
        }
        return conv.name || 'Unknown';
    }

    getConversationInitial(conv: any): string {
        const name = this.getConversationName(conv);
        return name ? name.charAt(0).toUpperCase() : '?';
    }

    // Computed signals for filtered lists
    filteredMessages = computed(() => {
        const query = this.conversationSearchQuery().toLowerCase().trim();
        const msgs = this.chatService.messages();
        if (!query) return msgs;
        return msgs.filter(m => m.content?.toLowerCase().includes(query) || m.fileName?.toLowerCase().includes(query));
    });

    filteredConversations = computed(() => {
        const query = this.searchQuery().toLowerCase().trim();
        const convs = this.conversations();
        if (!query) return convs;
        return convs.filter(c => {
            const name = this.getConversationName(c);
            return name?.toLowerCase().includes(query) ||
                c.lastMessage?.content?.toLowerCase().includes(query);
        });
    });

    filteredEmployees = computed(() => {
        const query = this.searchQuery().toLowerCase().trim();
        if (!query) return this.employees();
        return this.employees().filter(e =>
            e.fullName?.toLowerCase().includes(query) ||
            e.designation?.toLowerCase().includes(query) ||
            e.department?.toLowerCase().includes(query)
        );
    });

    ngOnInit() {
        this.loadConversations();
        this.loadEmployees();
    }

    ngAfterViewChecked() {
        this.scrollToBottom();
    }

    loadConversations() {
        this.api.get<any[]>('chat/conversations').subscribe({
            next: (res: any) => this.conversations.set(res.data || [])
        });
    }

    loadEmployees() {
        this.api.get<any>('employee').subscribe({
            next: (res: any) => this.employees.set(res.data || [])
        });
    }

    onTabChange(tab: 'chats' | 'directory') {
        this.activeTab.set(tab);
        this.searchQuery.set('');
    }

    isUserOnline(userId: string): boolean {
        return this.chatService.onlineUsers().includes(userId);
    }

    startDm(empId: string) {
        this.api.get<any>(`chat/conversations/dm/${empId}`).subscribe({
            next: (res: any) => {
                this.onTabChange('chats');
                this.loadConversations();
                this.selectConversation(res.data);
            }
        });
    }

    selectConversation(conv: any) {
        this.selectedConv.set(conv);
        this.chatService.joinConversation(conv.id);
        this.chatService.getMessages(conv.id);
        this.chatService.markAsRead(conv.id); // Mark as read when selected
        this.conversationSearchQuery.set('');
        this.showSearch.set(false);
        this.showAddMember.set(false);
    }

    async sendMessage(attachment?: { url: string, fileName: string }) {
        if (!attachment && !this.newMessage.trim()) return;
        if (!this.selectedConv()) return;

        if (attachment) {
            const dto: any = {
                conversationId: this.selectedConv().id,
                content: `Sent an attachment: ${attachment.fileName}`,
                fileUrl: attachment.url,
                fileName: attachment.fileName,
                type: 2 // File
            };
            this.api.post('chat/messages', dto).subscribe(() => this.loadConversations());
        } else {
            await this.chatService.sendMessage(this.selectedConv().id, this.newMessage);
            this.newMessage = '';
        }
    }

    onFileSelected(event: any) {
        const file = event.target.files[0];
        if (!file) return;

        if (file.size > 5 * 1024 * 1024) {
            alert('File is too large (max 5MB)');
            return;
        }

        const formData = new FormData();
        formData.append('file', file);

        this.api.post<any>('chat/upload', formData).subscribe({
            next: (res: any) => this.sendMessage(res.data),
            error: (err) => console.error('Upload failed', err)
        });
    }

    toggleMembers() {
        this.showMembers.update(v => !v);
        if (this.showMembers()) this.showAddMember.set(false);
    }

    toggleSearch() {
        this.showSearch.update(v => !v);
        if (!this.showSearch()) this.conversationSearchQuery.set('');
    }

    retractMessage(messageId: string) {
        this.chatService.retractMessage(messageId).subscribe({
            next: (res: any) => {
                // Update local signal to reflect retraction immediately
                this.chatService.messages.update(msgs => msgs.map(m =>
                    m.id === messageId ? { ...m, isRetracted: true, content: 'This message was deleted', fileUrl: null, fileName: null } : m
                ));
            },
            error: (err) => console.error('Failed to retract message', err)
        });
    }

    toggleAddMember() {
        this.showAddMember.update(v => !v);
        if (this.showAddMember()) this.showMembers.set(true);
    }

    addMemberToGroup(employeeId: string) {
        const conv = this.selectedConv();
        if (!conv || conv.type !== 'Group') return;

        this.chatService.addMember(conv.id, employeeId).subscribe({
            next: () => {
                this.showAddMember.set(false);
                // Refresh conversation to get new members
                this.api.get<any>(`chat/conversations/${conv.id}`).subscribe({
                    next: (res: any) => {
                        this.selectedConv.set(res.data);
                        this.loadConversations(); // Update left sidebar too
                    }
                });
            },
            error: (err) => alert(err.error?.message || 'Failed to add member')
        });
    }

    get availableEmployeesToAdd() {
        const conv = this.selectedConv();
        if (!conv || !conv.members) return [];
        const memberIds = new Set(conv.members.map((m: any) => m.employeeId));
        return this.filteredEmployees().filter(e => !memberIds.has(e.id));
    }

    // --- Group creation ---
    openGroupCreate() {
        this.showGroupCreate.set(true);
        this.groupName = '';
        this.groupDescription = '';
        this.selectedMembers.set(new Set());
        this.onTabChange('directory');
    }

    closeGroupCreate() {
        this.showGroupCreate.set(false);
        this.groupName = '';
        this.groupDescription = '';
        this.selectedMembers.set(new Set());
    }

    toggleMember(empId: string) {
        const current = new Set(this.selectedMembers());
        if (current.has(empId)) {
            current.delete(empId);
        } else {
            current.add(empId);
        }
        this.selectedMembers.set(current);
    }

    isMemberSelected(empId: string): boolean {
        return this.selectedMembers().has(empId);
    }

    createGroup() {
        if (!this.groupName.trim() || this.selectedMembers().size === 0) return;

        const dto = {
            name: this.groupName.trim(),
            description: this.groupDescription.trim(),
            type: 2, // Group
            memberIds: Array.from(this.selectedMembers())
        };

        this.api.post<any>('chat/conversations', dto).subscribe({
            next: (res: any) => {
                this.closeGroupCreate();
                this.onTabChange('chats');
                this.loadConversations();
                this.selectConversation(res.data);
            }
        });
    }

    private scrollToBottom() {
        if (this.scrollContainer) {
            this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight;
        }
    }
}
