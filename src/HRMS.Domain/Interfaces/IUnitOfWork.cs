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
    IAuditLogRepository AuditLogs { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
