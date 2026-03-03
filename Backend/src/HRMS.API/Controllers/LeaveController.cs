using System.Security.Claims;
using HRMS.Application.DTOs.Common;
using HRMS.Application.DTOs.Leave;
using HRMS.Application.Interfaces;
using HRMS.Domain.Enums;
using HRMS.Shared.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class LeaveController : ControllerBase
{
    private readonly ILeaveService _service;
    public LeaveController(ILeaveService service) => _service = service;

    [HttpPost("apply")]
    public async Task<IActionResult> Apply([FromBody] LeaveApplyDto dto)
    {
        var empId = Guid.Parse(User.FindFirst("EmployeeId")?.Value!);
        var by = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
        return Ok(ApiResponse<LeaveResponseDto>.Ok(await _service.ApplyLeaveAsync(empId, dto, by)));
    }

    [HttpPut("{id:guid}/approve-reject")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> ApproveReject(Guid id, [FromBody] LeaveApproveRejectDto dto)
    {
        var by = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
        return Ok(ApiResponse<LeaveResponseDto>.Ok(await _service.ApproveRejectLeaveAsync(id, dto, by)));
    }

    [HttpPut("{id:guid}/withdraw")]
    public async Task<IActionResult> Withdraw(Guid id)
    {
        var empId = Guid.Parse(User.FindFirst("EmployeeId")?.Value!);
        return Ok(ApiResponse<LeaveResponseDto>.Ok(await _service.WithdrawLeaveAsync(id, empId)));
    }

    [HttpPut("{id:guid}/cancel")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var by = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
        return Ok(ApiResponse<LeaveResponseDto>.Ok(await _service.CancelLeaveAsync(id, by)));
    }

    [HttpGet("balance/{employeeId:guid}")]
    public async Task<IActionResult> GetBalance(Guid employeeId, [FromQuery] int year)
    {
        return Ok(ApiResponse<IEnumerable<LeaveBalanceDto>>.Ok(await _service.GetBalancesAsync(employeeId, year)));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? employeeId, [FromQuery] LeaveStatus? status, [FromQuery] PaginationParams pagination)
    {
        return Ok(await _service.GetLeavesPagedAsync(employeeId, status, pagination));
    }

    [HttpGet("types")]
    public async Task<IActionResult> GetTypes()
    {
        return Ok(ApiResponse<IEnumerable<LeaveTypeDto>>.Ok(await _service.GetLeaveTypesAsync()));
    }

    [HttpPost("types")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateType([FromBody] LeaveTypeCreateDto dto)
    {
        var by = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
        return Ok(ApiResponse<LeaveTypeDto>.Ok(await _service.CreateLeaveTypeAsync(dto, by)));
    }

    // Temporary endpoint to allocate leave balances for testing
    [HttpPost("allocate-balance/{employeeId:guid}/{year:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AllocateLeaveBalanceForEmployee(Guid employeeId, int year)
    {
        var by = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
        await _service.AllocateLeaveBalancesAsync(employeeId, year, by);
        return Ok(ApiResponse<object>.Ok(null!, "Leave balances allocated successfully."));
    }
}
