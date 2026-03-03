using HRMS.Application.DTOs.Common;
using HRMS.Application.DTOs.Leave;
using HRMS.Domain.Enums;
using HRMS.Shared.Wrappers;

namespace HRMS.Application.Interfaces;

public interface ILeaveService
{
    Task<LeaveResponseDto> ApplyLeaveAsync(Guid employeeId, LeaveApplyDto dto, string performedBy);
    Task<LeaveResponseDto> ApproveRejectLeaveAsync(Guid leaveId, LeaveApproveRejectDto dto, string performedBy);
    Task<LeaveResponseDto> WithdrawLeaveAsync(Guid leaveId, Guid employeeId);
    Task<LeaveResponseDto> CancelLeaveAsync(Guid leaveId, string performedBy);
    Task<IEnumerable<LeaveBalanceDto>> GetBalancesAsync(Guid employeeId, int year);
    Task<PagedResponse<IEnumerable<LeaveResponseDto>>> GetLeavesPagedAsync(Guid? employeeId, LeaveStatus? status, PaginationParams pagination);
    Task<IEnumerable<LeaveTypeDto>> GetLeaveTypesAsync();
    Task<LeaveTypeDto> CreateLeaveTypeAsync(LeaveTypeCreateDto dto, string performedBy);
    Task AllocateLeaveBalancesAsync(Guid employeeId, int year, string performedBy);
}
