using AutoMapper;
using HRMS.Application.DTOs.Document;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Domain.Interfaces;
using HRMS.Shared.Exceptions;

namespace HRMS.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICloudinaryService _cloudinary;
    private readonly IAuditService _auditService;

    public DocumentService(IUnitOfWork unitOfWork, IMapper mapper, ICloudinaryService cloudinary, IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cloudinary = cloudinary;
        _auditService = auditService;
    }

    public async Task<DocumentResponseDto> UploadAsync(DocumentUploadDto dto, string performedBy)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(dto.EmployeeId);
        if (employee == null) throw new NotFoundException("Employee", dto.EmployeeId);

        using var stream = dto.File.OpenReadStream();
        var (publicId, secureUrl) = await _cloudinary.UploadAsync(
            stream, dto.File.FileName, $"hrms/documents/{employee.EmployeeCode}");

        var doc = new EmployeeDocument
        {
            EmployeeId = dto.EmployeeId,
            FileName = dto.File.FileName,
            FileType = dto.File.ContentType,
            FileSizeKB = dto.File.Length / 1024,
            Category = (DocumentCategory)dto.Category,
            CloudinaryPublicId = publicId,
            SecureUrl = secureUrl,
            UploadedBy = performedBy,
            UploadedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedBy = performedBy,
            UpdatedBy = performedBy
        };

        await _unitOfWork.Documents.AddAsync(doc);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync("EmployeeDocument", doc.Id.ToString(), AuditAction.Created, performedBy,
            remarks: $"Uploaded {dto.File.FileName}");

        return _mapper.Map<DocumentResponseDto>(doc);
    }

    public async Task<IEnumerable<DocumentResponseDto>> GetByEmployeeAsync(Guid employeeId)
    {
        var docs = await _unitOfWork.Documents.GetByEmployeeAsync(employeeId);
        return _mapper.Map<IEnumerable<DocumentResponseDto>>(docs);
    }

    public async Task<IEnumerable<DocumentResponseDto>> GetByEmployeeAndCategoryAsync(Guid employeeId, DocumentCategory category)
    {
        var docs = await _unitOfWork.Documents.GetByEmployeeAndCategoryAsync(employeeId, category);
        return _mapper.Map<IEnumerable<DocumentResponseDto>>(docs);
    }

    public async Task DeleteAsync(Guid documentId, string performedBy)
    {
        var doc = await _unitOfWork.Documents.GetByIdAsync(documentId);
        if (doc == null) throw new NotFoundException("EmployeeDocument", documentId);

        await _cloudinary.DeleteAsync(doc.CloudinaryPublicId);
        doc.IsActive = false;
        doc.UpdatedBy = performedBy;
        _unitOfWork.Documents.Update(doc);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync("EmployeeDocument", documentId.ToString(), AuditAction.Deleted, performedBy,
            remarks: $"Soft-deleted {doc.FileName}");
    }
}
