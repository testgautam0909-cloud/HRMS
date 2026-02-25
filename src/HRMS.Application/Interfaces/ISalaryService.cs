using HRMS.Application.DTOs.Common;
using HRMS.Application.DTOs.Salary;
using HRMS.Shared.Wrappers;

namespace HRMS.Application.Interfaces;

public interface ISalaryService
{
    Task<SalaryStructureResponseDto> CreateStructureAsync(SalaryStructureCreateDto dto, string performedBy);
    Task<SalaryStructureResponseDto> GetActiveStructureAsync(Guid employeeId);
    Task<IEnumerable<SalaryStructureResponseDto>> GetHistoryAsync(Guid employeeId);
    Task<IncrementResponseDto> RequestIncrementAsync(IncrementRequestDto dto, string performedBy);
    Task<IncrementResponseDto> ApproveRejectIncrementAsync(Guid incrementId, IncrementApproveRejectDto dto, string performedBy);
    Task<PagedResponse<IEnumerable<IncrementResponseDto>>> GetIncrementsPagedAsync(Guid? employeeId, PaginationParams pagination);
}
