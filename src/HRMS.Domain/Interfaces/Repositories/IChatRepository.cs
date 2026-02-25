using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Interfaces.Repositories;

public interface IChatRepository
{
    Task<ChatConversation?> GetConversationAsync(Guid conversationId);
    Task<ChatConversation?> GetDirectConversationAsync(Guid employee1Id, Guid employee2Id);
    Task<IEnumerable<ChatConversation>> GetUserConversationsAsync(Guid employeeId);
    Task<ChatConversation> CreateConversationAsync(ChatConversation conversation);
    Task<ChatMessage> AddMessageAsync(ChatMessage message);
    Task<(IEnumerable<ChatMessage> Items, int TotalCount)> GetMessagesPagedAsync(
        Guid conversationId, int page, int pageSize);
    Task<ChatMember?> GetMemberAsync(Guid conversationId, Guid employeeId);
    Task AddMemberAsync(ChatMember member);
    Task UpdateMemberAsync(ChatMember member);
    Task<int> GetUnreadCountAsync(Guid conversationId, Guid employeeId);
    Task MarkAsReadAsync(Guid conversationId, Guid employeeId);
    Task<ChatMessage?> GetMessageByIdAsync(Guid messageId);
    void UpdateMessage(ChatMessage message);
    void UpdateConversation(ChatConversation conversation);
}
