import { Injectable, inject, signal, effect } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';

export interface ChatMessage {
    id: string;
    senderId: string;
    senderName: string;
    content: string;
    timestamp: Date;
    conversationId: string;
}

@Injectable({
    providedIn: 'root'
})
export class ChatService {
    private auth = inject(AuthService);
    private hubConnection?: signalR.HubConnection;

    messages = signal<ChatMessage[]>([]);
    onlineUsers = signal<string[]>([]);
    isConnected = signal(false);

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
            .withUrl(environment.hubUrl, {
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
        this.hubConnection?.on('ReceiveMessage', (message: ChatMessage) => {
            this.messages.update(prev => [...prev, message]);
        });

        this.hubConnection?.on('UserOnline', (userId: string) => {
            this.onlineUsers.update(prev => [...prev, userId]);
        });

        this.hubConnection?.on('UserOffline', (userId: string) => {
            this.onlineUsers.update(prev => prev.filter(id => id !== userId));
        });
    }

    async sendMessage(conversationId: string, content: string) {
        if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
            await this.hubConnection.invoke('SendMessage', conversationId, content);
        }
    }

    async joinConversation(conversationId: string) {
        if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
            await this.hubConnection.invoke('JoinConversation', conversationId);
        }
    }
}
