using HRMS.Domain.Entities;

namespace HRMS.Domain.Interfaces.Repositories;

public interface IPayrollRepository : IGenericRepository<PayrollRecord>
{
    Task<PayrollRecord?> GetByEmployeeAndPeriodAsync(Guid employeeId, int month, int year);
    Task<bool> ExistsForPeriodAsync(int month, int year);
    Task<IEnumerable<PayrollRecord>> GetByPeriodAsync(int month, int year);
    Task<(IEnumerable<PayrollRecord> Items, int TotalCount)> GetPayrollsPagedAsync(
        Guid? employeeId, int? month, int? year, int page, int pageSize);
}

public interface ISalarySlipRepository : IGenericRepository<SalarySlip>
{
    Task<SalarySlip?> GetByPayrollIdAsync(Guid payrollRecordId);
}
