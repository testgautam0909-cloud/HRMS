namespace HRMS.Application.DTOs.Chat;

public class ConversationCreateDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int Type { get; set; }
    public List<Guid> MemberIds { get; set; } = new();
}

public class MessageSendDto
{
    public Guid ConversationId { get; set; }
    public string? Content { get; set; }
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public long? FileSizeKB { get; set; }
    public int Type { get; set; } = 1;
}

public class MessageResponseDto
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderEmployeeId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public long? FileSizeKB { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsRetracted { get; set; }
}

public class ConversationResponseDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<ChatMemberDto> Members { get; set; } = new();
    public MessageResponseDto? LastMessage { get; set; }
    public int UnreadCount { get; set; }
}

public class ChatMemberDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public DateTime JoinedAt { get; set; }
}
