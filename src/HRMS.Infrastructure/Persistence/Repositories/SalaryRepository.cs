using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Persistence.Repositories;

public class SalaryRepository : GenericRepository<SalaryStructure>, ISalaryRepository
{
    public SalaryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<SalaryStructure?> GetActiveStructureAsync(Guid employeeId)
    {
        return await _dbSet.FirstOrDefaultAsync(s =>
            s.EmployeeId == employeeId && s.IsActive);
    }

    public async Task<IEnumerable<SalaryStructure>> GetHistoryAsync(Guid employeeId)
    {
        return await _dbSet
            .Where(s => s.EmployeeId == employeeId)
            .OrderByDescending(s => s.EffectiveDate)
            .ToListAsync();
    }

    public async Task<bool> HasOverlappingStructureAsync(Guid employeeId, DateTime effectiveDate, Guid? excludeId = null)
    {
        var query = _dbSet.Where(s =>
            s.EmployeeId == employeeId &&
            s.IsActive &&
            s.EffectiveDate <= effectiveDate &&
            (s.EndDate == null || s.EndDate >= effectiveDate));

        if (excludeId.HasValue)
            query = query.Where(s => s.Id != excludeId.Value);

        return await query.AnyAsync();
    }
}

public class IncrementRepository : GenericRepository<IncrementRequest>, IIncrementRepository
{
    public IncrementRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<IncrementRequest>> GetByEmployeeAsync(Guid employeeId)
    {
        return await _dbSet
            .Where(i => i.EmployeeId == employeeId)
            .OrderByDescending(i => i.RequestDate)
            .ToListAsync();
    }

    public async Task<(IEnumerable<IncrementRequest> Items, int TotalCount)> GetIncrementsPagedAsync(
        Guid? employeeId, int page, int pageSize)
    {
        var query = _dbSet
            .Include(i => i.Employee)
            .AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(i => i.EmployeeId == employeeId.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(i => i.RequestDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
