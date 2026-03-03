using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

public class ChatConversation : BaseEntity
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ChatType Type { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<ChatMember> Members { get; set; } = new List<ChatMember>();
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
