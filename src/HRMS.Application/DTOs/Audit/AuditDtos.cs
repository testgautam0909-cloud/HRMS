namespace HRMS.Application.DTOs.Audit;

public class AuditLogResponseDto
{
    public Guid Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime PerformedAt { get; set; }
    public string? IpAddress { get; set; }
    public string? Remarks { get; set; }
}

public class AuditLogFilterDto
{
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public string? PerformedBy { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
