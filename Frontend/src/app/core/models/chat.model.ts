export interface Conversation {
    id: string;
    participantId: string;
    participantName: string;
    lastMessage?: string;
    lastMessageTime?: string;
    unreadCount: number;
}

export interface ChatMessage {
    id: string;
    conversationId: string;
    senderId: string;
    content: string;
    timestamp: string;
    isMine: boolean;
}
