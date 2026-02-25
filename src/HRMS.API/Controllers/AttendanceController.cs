using System.Security.Claims;
using HRMS.Application.DTOs.Attendance;
using HRMS.Application.DTOs.Common;
using HRMS.Application.Interfaces;
using HRMS.Shared.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _service;
    public AttendanceController(IAttendanceService service) => _service = service;

    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInDto dto)
    {
        var empId = Guid.Parse(User.FindFirst("EmployeeId")?.Value!);
        var by = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
        return Ok(ApiResponse<AttendanceResponseDto>.Ok(await _service.CheckInAsync(empId, dto, by)));
    }

    [HttpPost("check-out")]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutDto dto)
    {
        var empId = Guid.Parse(User.FindFirst("EmployeeId")?.Value!);
        var by = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
        return Ok(ApiResponse<AttendanceResponseDto>.Ok(await _service.CheckOutAsync(empId, dto, by)));
    }

    [HttpGet("monthly/{employeeId:guid}")]
    public async Task<IActionResult> GetMonthly(Guid employeeId, [FromQuery] int month, [FromQuery] int year)
    {
        return Ok(ApiResponse<IEnumerable<AttendanceResponseDto>>.Ok(await _service.GetMonthlyRecordsAsync(employeeId, month, year)));
    }

    [HttpGet("summary/{employeeId:guid}")]
    public async Task<IActionResult> GetSummary(Guid employeeId, [FromQuery] int month, [FromQuery] int year)
    {
        return Ok(ApiResponse<AttendanceSummaryDto>.Ok(await _service.GetMonthlySummaryAsync(employeeId, month, year)));
    }

    [HttpPut("correct")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Correct([FromBody] AttendanceCorrectionDto dto)
    {
        var by = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
        return Ok(ApiResponse<AttendanceResponseDto>.Ok(await _service.CorrectAttendanceAsync(dto, by)));
    }

    [HttpGet]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> GetAll([FromQuery] int month, [FromQuery] int year, [FromQuery] PaginationParams pagination)
    {
        return Ok(await _service.GetAllAttendancePagedAsync(month, year, pagination));
    }
}
