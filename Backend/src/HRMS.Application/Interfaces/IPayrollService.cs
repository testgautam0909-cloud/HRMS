using HRMS.Application.DTOs.Common;
using HRMS.Application.DTOs.Payroll;
using HRMS.Shared.Wrappers;

namespace HRMS.Application.Interfaces;

public interface IPayrollService
{
    Task<IEnumerable<PayrollResponseDto>> GeneratePayrollAsync(PayrollGenerateDto dto, string performedBy);
    Task<PayrollResponseDto> GetByIdAsync(Guid id);
    Task<PayrollResponseDto> GetByEmployeeAndPeriodAsync(Guid employeeId, int month, int year);
    Task<PagedResponse<IEnumerable<PayrollResponseDto>>> GetPayrollsPagedAsync(Guid? employeeId, int? month, int? year, PaginationParams pagination);
    Task<PayrollResponseDto> MarkAsPaidAsync(Guid payrollId, string performedBy);
    Task<PayrollResponseDto> OverridePayrollAsync(Guid payrollId, PayrollOverrideDto dto, string performedBy);
    Task<PayrollResponseDto> RelockPayrollAsync(Guid payrollId, string performedBy);
    Task<PayrollSummaryDto> GetPayrollSummaryAsync(int month, int year);
}
