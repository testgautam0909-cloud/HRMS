using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Interfaces.Repositories;

public interface IAuditLogRepository
{
    Task<AuditLog> AddAsync(AuditLog auditLog);
    Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetLogsPagedAsync(
        string? entityName, string? entityId, string? performedBy,
        DateTime? fromDate, DateTime? toDate, int page, int pageSize);
    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityName, string entityId);
}
