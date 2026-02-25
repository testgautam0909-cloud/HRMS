using HRMS.Application.DTOs.Attendance;
using HRMS.Application.DTOs.Common;
using HRMS.Shared.Wrappers;

namespace HRMS.Application.Interfaces;

public interface IAttendanceService
{
    Task<AttendanceResponseDto> CheckInAsync(Guid employeeId, CheckInDto dto, string performedBy);
    Task<AttendanceResponseDto> CheckOutAsync(Guid employeeId, CheckOutDto dto, string performedBy);
    Task<IEnumerable<AttendanceResponseDto>> GetMonthlyRecordsAsync(Guid employeeId, int month, int year);
    Task<AttendanceSummaryDto> GetMonthlySummaryAsync(Guid employeeId, int month, int year);
    Task<AttendanceResponseDto> CorrectAttendanceAsync(AttendanceCorrectionDto dto, string performedBy);
    Task<PagedResponse<IEnumerable<AttendanceResponseDto>>> GetAllAttendancePagedAsync(int month, int year, PaginationParams pagination);
}
