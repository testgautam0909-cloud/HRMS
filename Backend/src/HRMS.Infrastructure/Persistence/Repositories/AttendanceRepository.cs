using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Persistence.Repositories;

public class AttendanceRepository : GenericRepository<AttendanceRecord>, IAttendanceRepository
{
    public AttendanceRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<AttendanceRecord?> GetTodayRecordAsync(Guid employeeId, DateTime date)
    {
        return await _dbSet.FirstOrDefaultAsync(a =>
            a.EmployeeId == employeeId && a.Date.Date == date.Date);
    }

    public async Task<IEnumerable<AttendanceRecord>> GetMonthlyRecordsAsync(Guid employeeId, int month, int year)
    {
        return await _dbSet
            .Where(a => a.EmployeeId == employeeId && a.Date.Month == month && a.Date.Year == year)
            .OrderBy(a => a.Date)
            .ToListAsync();
    }

    public async Task<bool> HasOpenCheckInAsync(Guid employeeId, DateTime date)
    {
        return await _dbSet.AnyAsync(a =>
            a.EmployeeId == employeeId &&
            a.Date.Date == date.Date &&
            a.CheckOutTime == null);
    }
}
