using HRMS.Application.DTOs.Shift;
using HRMS.Application.Interfaces;
using HRMS.Shared.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ShiftController : ControllerBase
{
    private readonly IShiftService _service;

    public ShiftController(IShiftService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> GetAll()
    {
        var shifts = await _service.GetAllShiftsAsync();
        return Ok(ApiResponse<IEnumerable<ShiftDto>>.Ok(shifts, "Shifts retrieved successfully."));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,HR,Manager,Employee")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var shift = await _service.GetShiftByIdAsync(id);
        if (shift == null)
            return NotFound(ApiResponse<object>.Fail("Shift not found."));

        return Ok(ApiResponse<ShiftDto>.Ok(shift, "Shift retrieved successfully."));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Create([FromBody] CreateShiftDto dto)
    {
        var performedBy = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "system";
        var result = await _service.CreateShiftAsync(dto, performedBy);
        return Ok(ApiResponse<ShiftDto>.Ok(result, "Shift created successfully."));
    }

    [HttpPut]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Update([FromBody] UpdateShiftDto dto)
    {
        var performedBy = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "system";
        var result = await _service.UpdateShiftAsync(dto, performedBy);
        return Ok(ApiResponse<ShiftDto>.Ok(result, "Shift updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var performedBy = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "system";
        await _service.DeleteShiftAsync(id, performedBy);
        return Ok(ApiResponse<object>.Ok(null!, "Shift deleted successfully."));
    }

    [HttpGet("assignments")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<IActionResult> GetShiftAssignments()
    {
        var assignments = await _service.GetShiftAssignmentsAsync();
        return Ok(ApiResponse<IEnumerable<ShiftAssignmentDto>>.Ok(assignments, "Shift assignments retrieved successfully."));
    }

    [HttpGet("assignments/employee/{employeeId:guid}")]
    [Authorize(Roles = "Admin,HR,Manager,Employee")]
    public async Task<IActionResult> GetShiftAssignmentsByEmployee(Guid employeeId)
    {
        if (User.IsInRole("Employee"))
        {
            var currentEmployeeId = User.FindFirst("EmployeeId")?.Value;
            if (string.IsNullOrEmpty(currentEmployeeId) || Guid.Parse(currentEmployeeId) != employeeId)
            {
                return Forbid();
            }
        }

        var assignments = await _service.GetShiftAssignmentsByEmployeeAsync(employeeId);
        return Ok(ApiResponse<IEnumerable<ShiftAssignmentDto>>.Ok(assignments, "Employee shift assignments retrieved successfully."));
    }

    [HttpPost("assign")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<IActionResult> AssignShiftToEmployee([FromBody] CreateShiftAssignmentDto dto)
    {
        var performedBy = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "system";
        var result = await _service.AssignShiftToEmployeeAsync(dto, performedBy);
        return Ok(ApiResponse<ShiftAssignmentDto>.Ok(result, "Shift assigned to employee successfully."));
    }

    [HttpGet("schedule/employee/{employeeId:guid}")]
    [Authorize(Roles = "Admin,HR,Manager,Employee")]
    public async Task<IActionResult> GetEmployeeShiftSchedule(Guid employeeId)
    {
        var schedule = await _service.GetEmployeeShiftScheduleAsync(employeeId);
        return Ok(ApiResponse<EmployeeShiftScheduleDto>.Ok(schedule, "Employee shift schedule retrieved successfully."));
    }

    [HttpDelete("assignments/{id:guid}")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<IActionResult> DeleteAssignment(Guid id)
    {
        var performedBy = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "system";
        await _service.DeleteShiftAssignmentAsync(id, performedBy);
        return Ok(ApiResponse<object>.Ok(null!, "Shift assignment deleted successfully."));
    }
}