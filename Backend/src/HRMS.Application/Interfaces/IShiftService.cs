using HRMS.Application.DTOs.Shift;

namespace HRMS.Application.Interfaces;

public interface IShiftService
{
    Task<IEnumerable<ShiftDto>> GetAllShiftsAsync();
    Task<ShiftDto?> GetShiftByIdAsync(Guid id);
    Task<ShiftDto> CreateShiftAsync(CreateShiftDto dto, string performedBy);
    Task<ShiftDto> UpdateShiftAsync(UpdateShiftDto dto, string performedBy);
    Task DeleteShiftAsync(Guid id, string performedBy);
    Task<IEnumerable<ShiftAssignmentDto>> GetShiftAssignmentsAsync();
    Task<IEnumerable<ShiftAssignmentDto>> GetShiftAssignmentsByEmployeeAsync(Guid employeeId);
    Task<ShiftAssignmentDto> AssignShiftToEmployeeAsync(CreateShiftAssignmentDto dto, string performedBy);
    Task<EmployeeShiftScheduleDto> GetEmployeeShiftScheduleAsync(Guid employeeId);
}