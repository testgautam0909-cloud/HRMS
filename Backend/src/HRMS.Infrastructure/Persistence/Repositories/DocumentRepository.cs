using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Persistence.Repositories;

public class DocumentRepository : GenericRepository<EmployeeDocument>, IDocumentRepository
{
    public DocumentRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<EmployeeDocument>> GetByEmployeeAsync(Guid employeeId)
    {
        return await _dbSet
            .Where(d => d.EmployeeId == employeeId && d.IsActive)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<EmployeeDocument>> GetByEmployeeAndCategoryAsync(
        Guid employeeId, DocumentCategory category)
    {
        return await _dbSet
            .Where(d => d.EmployeeId == employeeId && d.Category == category && d.IsActive)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }
}
