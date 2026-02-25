using HRMS.Application.DTOs.Document;
using HRMS.Domain.Enums;

namespace HRMS.Application.Interfaces;

public interface IDocumentService
{
    Task<DocumentResponseDto> UploadAsync(DocumentUploadDto dto, string performedBy);
    Task<IEnumerable<DocumentResponseDto>> GetByEmployeeAsync(Guid employeeId);
    Task<IEnumerable<DocumentResponseDto>> GetByEmployeeAndCategoryAsync(Guid employeeId, DocumentCategory category);
    Task DeleteAsync(Guid documentId, string performedBy);
}

public interface ICloudinaryService
{
    Task<(string PublicId, string SecureUrl)> UploadAsync(Stream fileStream, string fileName, string folder);
    Task<(string PublicId, string SecureUrl)> UploadPdfAsync(byte[] pdfBytes, string fileName, string folder);
    Task DeleteAsync(string publicId);
}

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, byte[]? attachment = null, string? attachmentName = null);
    Task SendWelcomeEmailAsync(string to, string employeeName, string password);
    Task SendPasswordResetEmailAsync(string to, string employeeName, string newPassword);
    Task SendLeaveStatusEmailAsync(string to, string employeeName, string leaveType, string status, string? remarks);
    Task SendSalarySlipEmailAsync(string to, string employeeName, int month, int year, byte[] pdfBytes);
    Task SendIncrementEmailAsync(string to, string employeeName, decimal oldCtc, decimal newCtc, decimal percentage);
}

public interface ISalarySlipService
{
    Task<byte[]> GeneratePdfAsync(Guid payrollRecordId);
}
