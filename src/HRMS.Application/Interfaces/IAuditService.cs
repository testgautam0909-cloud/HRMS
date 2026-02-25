using HRMS.Application.DTOs.Audit;
using HRMS.Application.DTOs.Common;
using HRMS.Domain.Enums;
using HRMS.Shared.Wrappers;

namespace HRMS.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(string entityName, string entityId, AuditAction action, string performedBy, string? oldValues = null, string? newValues = null, string? ipAddress = null, string? remarks = null);
    Task<PagedResponse<IEnumerable<AuditLogResponseDto>>> GetLogsPagedAsync(AuditLogFilterDto filter, PaginationParams pagination);
    Task<IEnumerable<AuditLogResponseDto>> GetEntityLogsAsync(string entityName, string entityId);
}
