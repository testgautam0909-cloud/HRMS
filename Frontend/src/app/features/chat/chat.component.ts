import { Component, inject, signal, OnInit, ElementRef, ViewChild, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../core/services/chat.service';
import { AuthService } from '../../core/services/auth.service';
import { ApiService } from '../../core/services/api.service';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatInputModule } from '@angular/material/input';
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

  currentUserId = this.auth.currentUser()?.id;
  conversations = signal<any[]>([]);
  selectedConv = signal<any>(null);
  newMessage = '';

  ngOnInit() {
    this.loadConversations();
  }

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  loadConversations() {
    this.api.get<any[]>('chat/conversations').subscribe({
      next: (res: any) => this.conversations.set(res.data)
    });
  }

  selectConversation(conv: any) {
    this.selectedConv.set(conv);
    this.chatService.joinConversation(conv.id);
  }

  async sendMessage() {
    if (!this.newMessage.trim() || !this.selectedConv()) return;

    await this.chatService.sendMessage(this.selectedConv().id, this.newMessage);
    this.newMessage = '';
  }

  private scrollToBottom() {
    if (this.scrollContainer) {
      this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight;
    }
  }
}
