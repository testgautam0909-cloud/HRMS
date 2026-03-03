namespace HRMS.Domain.Entities;

public class ChatMember : BaseEntity
{
    public Guid ConversationId { get; set; }
    public ChatConversation Conversation { get; set; } = null!;
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public bool IsAdmin { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LastReadAt { get; set; }
    public int UnreadCount { get; set; }
}
