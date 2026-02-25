using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Persistence.Repositories;

public class PayrollRepository : GenericRepository<PayrollRecord>, IPayrollRepository
{
    public PayrollRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<PayrollRecord?> GetByEmployeeAndPeriodAsync(Guid employeeId, int month, int year)
    {
        return await _dbSet
            .Include(p => p.SalarySlip)
            .FirstOrDefaultAsync(p =>
                p.EmployeeId == employeeId && p.Month == month && p.Year == year);
    }

    public async Task<bool> ExistsForPeriodAsync(int month, int year)
    {
        return await _dbSet.AnyAsync(p => p.Month == month && p.Year == year);
    }

    public async Task<IEnumerable<PayrollRecord>> GetByPeriodAsync(int month, int year)
    {
        return await _dbSet
            .Include(p => p.Employee)
            .Include(p => p.SalarySlip)
            .Where(p => p.Month == month && p.Year == year)
            .ToListAsync();
    }

    public async Task<(IEnumerable<PayrollRecord> Items, int TotalCount)> GetPayrollsPagedAsync(
        Guid? employeeId, int? month, int? year, int page, int pageSize)
    {
        var query = _dbSet
            .Include(p => p.Employee)
            .Include(p => p.SalarySlip)
            .AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(p => p.EmployeeId == employeeId.Value);

        if (month.HasValue)
            query = query.Where(p => p.Month == month.Value);

        if (year.HasValue)
            query = query.Where(p => p.Year == year.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}

public class SalarySlipRepository : GenericRepository<SalarySlip>, ISalarySlipRepository
{
    public SalarySlipRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<SalarySlip?> GetByPayrollIdAsync(Guid payrollRecordId)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.PayrollRecordId == payrollRecordId);
    }
}
