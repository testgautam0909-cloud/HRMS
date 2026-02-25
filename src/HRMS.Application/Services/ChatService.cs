using AutoMapper;
using HRMS.Application.DTOs.Chat;
using HRMS.Application.DTOs.Common;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Domain.Interfaces;
using HRMS.Shared.Exceptions;
using HRMS.Shared.Wrappers;

namespace HRMS.Application.Services;

public class ChatService : IChatService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ChatService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ConversationResponseDto> CreateConversationAsync(Guid creatorEmployeeId, ConversationCreateDto dto)
    {
        var type = dto.MemberIds.Count == 1 ? ChatType.Direct : ChatType.Group;

        var allMemberIds = new List<Guid>(dto.MemberIds) { creatorEmployeeId };
        allMemberIds = allMemberIds.Distinct().ToList();

        if (type == ChatType.Direct && allMemberIds.Count == 2)
        {
            var existing = await _unitOfWork.Chats.GetDirectConversationAsync(allMemberIds[0], allMemberIds[1]);
            if (existing != null) return _mapper.Map<ConversationResponseDto>(existing);
        }

        var conversation = new ChatConversation
        {
            Type = type,
            Name = type == ChatType.Group ? dto.Name : null,
            Description = type == ChatType.Group ? dto.Description : null,
            Members = allMemberIds.Select(id => new ChatMember
            {
                EmployeeId = id,
                JoinedAt = DateTime.UtcNow,
                IsAdmin = id == creatorEmployeeId
            }).ToList()
        };

        var created = await _unitOfWork.Chats.CreateConversationAsync(conversation);
        await _unitOfWork.SaveChangesAsync();

        var result = await _unitOfWork.Chats.GetConversationAsync(created.Id);
        return _mapper.Map<ConversationResponseDto>(result);
    }

    public async Task<ConversationResponseDto> GetOrCreateDirectConversationAsync(Guid employee1Id, Guid employee2Id)
    {
        var existing = await _unitOfWork.Chats.GetDirectConversationAsync(employee1Id, employee2Id);
        if (existing != null) return _mapper.Map<ConversationResponseDto>(existing);

        return await CreateConversationAsync(employee1Id, new ConversationCreateDto
        {
            MemberIds = new List<Guid> { employee2Id }
        });
    }

    public async Task<IEnumerable<ConversationResponseDto>> GetUserConversationsAsync(Guid employeeId)
    {
        var conversations = await _unitOfWork.Chats.GetUserConversationsAsync(employeeId);
        return _mapper.Map<IEnumerable<ConversationResponseDto>>(conversations);
    }

    public async Task<ConversationResponseDto> GetConversationAsync(Guid conversationId, Guid employeeId)
    {
        var conv = await _unitOfWork.Chats.GetConversationAsync(conversationId);
        if (conv == null) throw new NotFoundException("ChatConversation", conversationId);

        var isMember = conv.Members.Any(m => m.EmployeeId == employeeId);
        if (!isMember) throw new ForbiddenException();

        return _mapper.Map<ConversationResponseDto>(conv);
    }

    public async Task<MessageResponseDto> SendMessageAsync(Guid senderEmployeeId, MessageSendDto dto)
    {
        var conv = await _unitOfWork.Chats.GetConversationAsync(dto.ConversationId);
        if (conv == null) throw new NotFoundException("ChatConversation", dto.ConversationId);

        var isMember = conv.Members.Any(m => m.EmployeeId == senderEmployeeId);
        if (!isMember) throw new ForbiddenException();

        var type = !string.IsNullOrEmpty(dto.FileUrl) ? MessageType.File : MessageType.Text;

        var message = new ChatMessage
        {
            ConversationId = dto.ConversationId,
            SenderEmployeeId = senderEmployeeId,
            Content = dto.Content,
            FileUrl = dto.FileUrl,
            FileName = dto.FileName,
            Type = type,
            SentAt = DateTime.UtcNow
        };

        await _unitOfWork.Chats.AddMessageAsync(message);
        await _unitOfWork.SaveChangesAsync();

        var sender = await _unitOfWork.Employees.GetByIdAsync(senderEmployeeId);
        return new MessageResponseDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderEmployeeId = senderEmployeeId,
            SenderName = sender != null ? $"{sender.FirstName} {sender.LastName}" : "Unknown",
            Content = message.Content,
            FileUrl = message.FileUrl,
            FileName = message.FileName,
            Type = type.ToString(),
            SentAt = message.SentAt,
            IsRetracted = false
        };
    }

    public async Task<PagedResponse<IEnumerable<MessageResponseDto>>> GetMessagesPagedAsync(
        Guid conversationId, Guid employeeId, PaginationParams pagination)
    {
        var conv = await _unitOfWork.Chats.GetConversationAsync(conversationId);
        if (conv == null) throw new NotFoundException("ChatConversation", conversationId);
        if (!conv.Members.Any(m => m.EmployeeId == employeeId)) throw new ForbiddenException();

        var (messages, total) = await _unitOfWork.Chats.GetMessagesPagedAsync(
            conversationId, pagination.Page, pagination.PageSize);
        var dtos = _mapper.Map<IEnumerable<MessageResponseDto>>(messages);
        return PagedResponse<IEnumerable<MessageResponseDto>>.CreateResponse(
            dtos, pagination.Page, pagination.PageSize, total, "Messages fetched.");
    }

    public async Task MarkAsReadAsync(Guid conversationId, Guid employeeId)
    {
        await _unitOfWork.Chats.MarkAsReadAsync(conversationId, employeeId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<MessageResponseDto> RetractMessageAsync(Guid messageId, Guid senderEmployeeId)
    {
        var message = await _unitOfWork.Chats.GetMessageByIdAsync(messageId);
        if (message == null) throw new NotFoundException("ChatMessage", messageId);
        if (message.SenderEmployeeId != senderEmployeeId) throw new ForbiddenException();

        message.IsRetracted = true;
        message.Content = null;
        message.FileUrl = null;
        _unitOfWork.Chats.UpdateMessage(message);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<MessageResponseDto>(message);
    }

    public async Task AddMemberAsync(Guid conversationId, Guid employeeId, Guid addedByEmployeeId)
    {
        var conv = await _unitOfWork.Chats.GetConversationAsync(conversationId);
        if (conv == null) throw new NotFoundException("ChatConversation", conversationId);
        if (conv.Type != ChatType.Group) throw new ConflictException("Cannot add members to a direct conversation.");
        if (conv.Members.Any(m => m.EmployeeId == employeeId)) throw new ConflictException("Already a member.");

        var adder = conv.Members.FirstOrDefault(m => m.EmployeeId == addedByEmployeeId);
        if (adder == null || !adder.IsAdmin) throw new ForbiddenException();

        await _unitOfWork.Chats.AddMemberAsync(new ChatMember
        {
            ConversationId = conversationId,
            EmployeeId = employeeId,
            JoinedAt = DateTime.UtcNow,
            IsAdmin = false
        });
        await _unitOfWork.SaveChangesAsync();
    }
}
