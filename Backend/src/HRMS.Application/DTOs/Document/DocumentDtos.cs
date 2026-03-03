using Microsoft.AspNetCore.Http;

namespace HRMS.Application.DTOs.Document;

public class DocumentUploadDto
{
    public Guid EmployeeId { get; set; }
    public IFormFile File { get; set; } = null!;
    public int Category { get; set; }
}

public class DocumentResponseDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSizeKB { get; set; }
    public string SecureUrl { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
