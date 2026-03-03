using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

public class ChatMessage : BaseEntity
{
    public Guid ConversationId { get; set; }
    public ChatConversation Conversation { get; set; } = null!;
    public Guid SenderEmployeeId { get; set; }
    public Employee Sender { get; set; } = null!;
    public MessageType Type { get; set; }
    public string? Content { get; set; }
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public long? FileSizeKB { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsRetracted { get; set; }
    public DateTime? RetractedAt { get; set; }
}
