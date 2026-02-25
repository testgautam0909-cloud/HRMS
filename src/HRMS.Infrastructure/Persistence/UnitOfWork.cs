using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces;
using HRMS.Domain.Interfaces.Repositories;
using HRMS.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace HRMS.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    private IEmployeeRepository? _employees;
    private IAttendanceRepository? _attendances;
    private ILeaveRepository? _leaves;
    private ILeaveBalanceRepository? _leaveBalances;
    private ILeaveTypeRepository? _leaveTypes;
    private ISalaryRepository? _salaries;
    private IIncrementRepository? _increments;
    private IPayrollRepository? _payrolls;
    private ISalarySlipRepository? _salarySlips;
    private IDocumentRepository? _documents;
    private IChatRepository? _chats;
    private IAuditLogRepository? _auditLogs;
    private IRefreshTokenRepository? _refreshTokens;
    private IGenericRepository<Department>? _departments;
    private IGenericRepository<Designation>? _designations;
    private IGenericRepository<ExperienceHistory>? _experienceHistories;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IEmployeeRepository Employees => _employees ??= new EmployeeRepository(_context);
    public IAttendanceRepository Attendances => _attendances ??= new AttendanceRepository(_context);
    public ILeaveRepository Leaves => _leaves ??= new LeaveRepository(_context);
    public ILeaveBalanceRepository LeaveBalances => _leaveBalances ??= new LeaveBalanceRepository(_context);
    public ILeaveTypeRepository LeaveTypes => _leaveTypes ??= new LeaveTypeRepository(_context);
    public ISalaryRepository Salaries => _salaries ??= new SalaryRepository(_context);
    public IIncrementRepository Increments => _increments ??= new IncrementRepository(_context);
    public IPayrollRepository Payrolls => _payrolls ??= new PayrollRepository(_context);
    public ISalarySlipRepository SalarySlips => _salarySlips ??= new SalarySlipRepository(_context);
    public IDocumentRepository Documents => _documents ??= new DocumentRepository(_context);
    public IChatRepository Chats => _chats ??= new ChatRepository(_context);
    public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_context);
    public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(_context);
    public IGenericRepository<Department> Departments => _departments ??= new GenericRepository<Department>(_context);
    public IGenericRepository<Designation> Designations => _designations ??= new GenericRepository<Designation>(_context);
    public IGenericRepository<ExperienceHistory> ExperienceHistories => _experienceHistories ??= new GenericRepository<ExperienceHistory>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
