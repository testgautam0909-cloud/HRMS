using HRMS.Domain.Entities;

namespace HRMS.Domain.Interfaces.Repositories;

public interface IAttendanceRepository : IGenericRepository<AttendanceRecord>
{
    Task<AttendanceRecord?> GetTodayRecordAsync(Guid employeeId, DateTime date);
    Task<AttendanceRecord?> GetLatestOpenRecordAsync(Guid employeeId, DateTime date);
    Task<IEnumerable<AttendanceRecord>> GetMonthlyRecordsAsync(Guid employeeId, int month, int year);
    Task<bool> HasOpenCheckInAsync(Guid employeeId, DateTime date);
}
