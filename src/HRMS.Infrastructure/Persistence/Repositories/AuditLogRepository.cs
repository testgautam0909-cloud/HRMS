using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Persistence.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AuditLog> AddAsync(AuditLog auditLog)
    {
        auditLog.Id = Guid.NewGuid();
        auditLog.PerformedAt = DateTime.UtcNow;
        await _context.AuditLogs.AddAsync(auditLog);
        return auditLog;
    }

    public async Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetLogsPagedAsync(
        string? entityName, string? entityId, string? performedBy,
        DateTime? fromDate, DateTime? toDate, int page, int pageSize)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityName))
            query = query.Where(a => a.EntityName == entityName);

        if (!string.IsNullOrWhiteSpace(entityId))
            query = query.Where(a => a.EntityId == entityId);

        if (!string.IsNullOrWhiteSpace(performedBy))
            query = query.Where(a => a.PerformedBy == performedBy);

        if (fromDate.HasValue)
            query = query.Where(a => a.PerformedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(a => a.PerformedAt <= toDate.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.PerformedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityName, string entityId)
    {
        return await _context.AuditLogs
            .Where(a => a.EntityName == entityName && a.EntityId == entityId)
            .OrderByDescending(a => a.PerformedAt)
            .ToListAsync();
    }
}
