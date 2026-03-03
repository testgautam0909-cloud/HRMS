using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Persistence.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly AppDbContext _context;

    public ChatRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ChatConversation?> GetConversationAsync(Guid conversationId)
    {
        return await _context.ChatConversations
            .Include(c => c.Members)
                .ThenInclude(m => m.Employee)
            .FirstOrDefaultAsync(c => c.Id == conversationId);
    }

    public async Task<ChatConversation?> GetDirectConversationAsync(Guid employee1Id, Guid employee2Id)
    {
        return await _context.ChatConversations
            .Include(c => c.Members)
            .Where(c => c.Type == Domain.Enums.ChatType.Direct)
            .Where(c => c.Members.Any(m => m.EmployeeId == employee1Id) &&
                        c.Members.Any(m => m.EmployeeId == employee2Id))
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ChatConversation>> GetUserConversationsAsync(Guid employeeId)
    {
        return await _context.ChatConversations
            .Include(c => c.Members)
                .ThenInclude(m => m.Employee)
            .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
            .Where(c => c.Members.Any(m => m.EmployeeId == employeeId))
            .OrderByDescending(c => c.Messages.Max(m => (DateTime?)m.SentAt) ?? c.CreatedAt)
            .ToListAsync();
    }

    public async Task<ChatConversation> CreateConversationAsync(ChatConversation conversation)
    {
        await _context.ChatConversations.AddAsync(conversation);
        return conversation;
    }

    public async Task<ChatMessage> AddMessageAsync(ChatMessage message)
    {
        await _context.ChatMessages.AddAsync(message);
        return message;
    }

    public async Task<(IEnumerable<ChatMessage> Items, int TotalCount)> GetMessagesPagedAsync(
        Guid conversationId, int page, int pageSize)
    {
        var query = _context.ChatMessages
            .Include(m => m.Sender)
            .Where(m => m.ConversationId == conversationId && !m.IsRetracted);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<ChatMember?> GetMemberAsync(Guid conversationId, Guid employeeId)
    {
        return await _context.ChatMembers
            .FirstOrDefaultAsync(m => m.ConversationId == conversationId && m.EmployeeId == employeeId);
    }

    public async Task AddMemberAsync(ChatMember member)
    {
        await _context.ChatMembers.AddAsync(member);
    }

    public async Task UpdateMemberAsync(ChatMember member)
    {
        _context.ChatMembers.Update(member);
        await Task.CompletedTask;
    }

    public async Task<int> GetUnreadCountAsync(Guid conversationId, Guid employeeId)
    {
        var member = await GetMemberAsync(conversationId, employeeId);
        return member?.UnreadCount ?? 0;
    }

    public async Task MarkAsReadAsync(Guid conversationId, Guid employeeId)
    {
        var member = await GetMemberAsync(conversationId, employeeId);
        if (member != null)
        {
            member.LastReadAt = DateTime.UtcNow;
            member.UnreadCount = 0;
            _context.ChatMembers.Update(member);
        }
    }

    public async Task<ChatMessage?> GetMessageByIdAsync(Guid messageId)
    {
        return await _context.ChatMessages
            .Include(m => m.Sender)
            .FirstOrDefaultAsync(m => m.Id == messageId);
    }

    public void UpdateMessage(ChatMessage message)
    {
        _context.ChatMessages.Update(message);
    }

    public void UpdateConversation(ChatConversation conversation)
    {
        _context.ChatConversations.Update(conversation);
    }
}
