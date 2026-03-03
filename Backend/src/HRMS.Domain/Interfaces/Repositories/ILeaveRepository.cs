using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Interfaces.Repositories;

public interface ILeaveRepository : IGenericRepository<LeaveApplication>
{
    Task<IEnumerable<LeaveApplication>> GetByEmployeeAsync(Guid employeeId);
    Task<bool> HasOverlappingLeaveAsync(Guid employeeId, DateTime startDate, DateTime endDate, Guid? excludeId = null);
    Task<(IEnumerable<LeaveApplication> Items, int TotalCount)> GetLeavesPagedAsync(
        Guid? employeeId, LeaveStatus? status, int page, int pageSize);
}

public interface ILeaveBalanceRepository : IGenericRepository<LeaveBalance>
{
    Task<LeaveBalance?> GetBalanceAsync(Guid employeeId, Guid leaveTypeId, int year);
    Task<IEnumerable<LeaveBalance>> GetAllBalancesAsync(Guid employeeId, int year);
}

public interface ILeaveTypeRepository : IGenericRepository<LeaveType>
{
    Task<IEnumerable<LeaveType>> GetActiveTypesAsync();
}
