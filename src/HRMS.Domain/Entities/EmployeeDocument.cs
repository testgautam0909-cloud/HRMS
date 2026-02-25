using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

public class EmployeeDocument : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSizeKB { get; set; }
    public string CloudinaryPublicId { get; set; } = string.Empty;
    public string SecureUrl { get; set; } = string.Empty;
    public DocumentCategory Category { get; set; }
    public bool IsActive { get; set; } = true;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
