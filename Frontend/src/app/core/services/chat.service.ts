import { Injectable, inject, signal, effect } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { ApiService } from './api.service';

export interface ChatMessage {
    id: string;
    senderEmployeeId: string;
    senderName: string;
    content: string;
    timestamp: Date;
    conversationId: string;
    isRetracted?: boolean;
}

@Injectable({
    providedIn: 'root'
})
export class ChatService {
    private auth = inject(AuthService);
    private hubConnection?: signalR.HubConnection;

    messages = signal<any[]>([]);
    onlineUsers = signal<string[]>([]);
    isConnected = signal(false);

    private api = inject(ApiService);

    constructor() {
        effect(() => {
            const user = this.auth.currentUser();
            if (user) {
                this.startConnection();
            } else {
                this.stopConnection();
            }
        });
    }

    private startConnection() {
        const token = localStorage.getItem('hrms_token');
        if (!token) return;

        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(`${environment.hubUrl}`, {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .build();

        this.hubConnection.start()
            .then(() => {
                this.isConnected.set(true);
                this.registerHandlers();
            })
            .catch(err => console.error('Error while starting connection: ' + err));
    }

    private stopConnection() {
        this.hubConnection?.stop().then(() => this.isConnected.set(false));
    }

    private registerHandlers() {
        this.hubConnection?.on('ReceiveMessage', (message: any) => {
            this.messages.update(prev => [...prev, {
                ...message,
                timestamp: new Date(message.sentAt)
            }]);
        });

        this.hubConnection?.on('UserOnline', (userId: string) => {
            this.onlineUsers.update(prev => [...prev, userId]);
        });

        this.hubConnection?.on('UserOffline', (userId: string) => {
            this.onlineUsers.update(prev => prev.filter(id => id !== userId));
        });

        this.hubConnection?.on('MessagesRead', (data: { employeeId: string, conversationId: string }) => {
            // Can be used to update UI for read receipts if needed
            console.log('Messages read by', data.employeeId, 'in', data.conversationId);
        });
    }

    getMessages(conversationId: string) {
        this.api.get<any>(`chat/conversations/${conversationId}/messages`, { pageSize: 50 })
            .subscribe(res => {
                const msgs = (res.data?.data || res.data || []).map((m: any) => ({
                    ...m,
                    timestamp: new Date(m.sentAt)
                }));
                // Sort by date ascending for the chat view
                this.messages.set(msgs.sort((a: any, b: any) => a.timestamp.getTime() - b.timestamp.getTime()));
            });
    }

    async sendMessage(conversationId: string, content: string, fileUrl?: string, fileName?: string) {
        if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
            const dto = {
                conversationId,
                content,
                fileUrl,
                fileName,
                type: fileUrl ? 2 : 1
            };
            await this.hubConnection.invoke('SendMessage', dto);
        }
    }

    async joinConversation(conversationId: string) {
        if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
            await this.hubConnection.invoke('JoinConversation', conversationId);
        }
    }

    async markAsRead(conversationId: string) {
        if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
            await this.hubConnection.invoke('MarkAsRead', conversationId);
        }
    }

    retractMessage(messageId: string) {
        return this.api.put<any>(`chat/messages/${messageId}/retract`, {});
    }

    addMember(conversationId: string, employeeId: string) {
        return this.api.post<any>(`chat/conversations/${conversationId}/members/${employeeId}`, {});
    }
}
