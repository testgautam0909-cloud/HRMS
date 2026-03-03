using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Persistence.Repositories;

public class LeaveRepository : GenericRepository<LeaveApplication>, ILeaveRepository
{
    public LeaveRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<LeaveApplication>> GetByEmployeeAsync(Guid employeeId)
    {
        return await _dbSet
            .Include(l => l.LeaveType)
            .Where(l => l.EmployeeId == employeeId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> HasOverlappingLeaveAsync(Guid employeeId, DateTime startDate, DateTime endDate, Guid? excludeId = null)
    {
        var query = _dbSet.Where(l =>
            l.EmployeeId == employeeId &&
            l.Status != LeaveStatus.Rejected &&
            l.Status != LeaveStatus.Withdrawn &&
            l.Status != LeaveStatus.Cancelled &&
            l.StartDate <= endDate &&
            l.EndDate >= startDate);

        if (excludeId.HasValue)
        {
            query = query.Where(l => l.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<(IEnumerable<LeaveApplication> Items, int TotalCount)> GetLeavesPagedAsync(
        Guid? employeeId, LeaveStatus? status, int page, int pageSize)
    {
        var query = _dbSet
            .Include(l => l.LeaveType)
            .Include(l => l.Employee)
            .AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(l => l.EmployeeId == employeeId.Value);

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}

public class LeaveBalanceRepository : GenericRepository<LeaveBalance>, ILeaveBalanceRepository
{
    public LeaveBalanceRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<LeaveBalance?> GetBalanceAsync(Guid employeeId, Guid leaveTypeId, int year)
    {
        return await _dbSet.FirstOrDefaultAsync(lb =>
            lb.EmployeeId == employeeId &&
            lb.LeaveTypeId == leaveTypeId &&
            lb.Year == year);
    }

    public async Task<IEnumerable<LeaveBalance>> GetAllBalancesAsync(Guid employeeId, int year)
    {
        return await _dbSet
            .Include(lb => lb.LeaveType)
            .Where(lb => lb.EmployeeId == employeeId && lb.Year == year)
            .ToListAsync();
    }
}

public class LeaveTypeRepository : GenericRepository<LeaveType>, ILeaveTypeRepository
{
    public LeaveTypeRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<LeaveType>> GetActiveTypesAsync()
    {
        return await _dbSet.Where(lt => lt.IsActive).ToListAsync();
    }
}
