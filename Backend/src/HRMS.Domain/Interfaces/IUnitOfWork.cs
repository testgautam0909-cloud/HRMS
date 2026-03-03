using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces.Repositories;

namespace HRMS.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IEmployeeRepository Employees { get; }
    IAttendanceRepository Attendances { get; }
    ILeaveRepository Leaves { get; }
    ILeaveBalanceRepository LeaveBalances { get; }
    ILeaveTypeRepository LeaveTypes { get; }
    ISalaryRepository Salaries { get; }
    IIncrementRepository Increments { get; }
    IPayrollRepository Payrolls { get; }
    ISalarySlipRepository SalarySlips { get; }
    IDocumentRepository Documents { get; }
    IChatRepository Chats { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IGenericRepository<Department> Departments { get; }
    IGenericRepository<Designation> Designations { get; }
    IGenericRepository<ExperienceHistory> ExperienceHistories { get; }
    IGenericRepository<Shift> Shifts { get; }
    IGenericRepository<ShiftAssignment> ShiftAssignments { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
