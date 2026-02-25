using HRMS.Application.DTOs.Chat;
using HRMS.Application.DTOs.Common;
using HRMS.Shared.Wrappers;

namespace HRMS.Application.Interfaces;

public interface IChatService
{
    Task<ConversationResponseDto> CreateConversationAsync(Guid creatorEmployeeId, ConversationCreateDto dto);
    Task<ConversationResponseDto> GetOrCreateDirectConversationAsync(Guid employee1Id, Guid employee2Id);
    Task<IEnumerable<ConversationResponseDto>> GetUserConversationsAsync(Guid employeeId);
    Task<ConversationResponseDto> GetConversationAsync(Guid conversationId, Guid employeeId);
    Task<MessageResponseDto> SendMessageAsync(Guid senderEmployeeId, MessageSendDto dto);
    Task<PagedResponse<IEnumerable<MessageResponseDto>>> GetMessagesPagedAsync(Guid conversationId, Guid employeeId, PaginationParams pagination);
    Task MarkAsReadAsync(Guid conversationId, Guid employeeId);
    Task<MessageResponseDto> RetractMessageAsync(Guid messageId, Guid senderEmployeeId);
    Task AddMemberAsync(Guid conversationId, Guid employeeId, Guid addedByEmployeeId);
}
