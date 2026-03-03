using System.Security.Claims;
using HRMS.Application.DTOs.Common;
using HRMS.Application.DTOs.Salary;
using HRMS.Application.Interfaces;
using HRMS.Shared.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SalaryController : ControllerBase
{
    private readonly ISalaryService _service;
    public SalaryController(ISalaryService service) => _service = service;

    [HttpPost("structure")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> CreateStructure([FromBody] SalaryStructureCreateDto dto)
    {
        var by = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
        return Ok(ApiResponse<SalaryStructureResponseDto>.Ok(await _service.CreateStructureAsync(dto, by)));
    }

    [HttpGet("structure/{employeeId:guid}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> GetActive(Guid employeeId)
    {
        return Ok(ApiResponse<SalaryStructureResponseDto>.Ok(await _service.GetActiveStructureAsync(employeeId)));
    }

    [HttpGet("structure/{employeeId:guid}/history")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> GetHistory(Guid employeeId)
    {
        return Ok(ApiResponse<IEnumerable<SalaryStructureResponseDto>>.Ok(await _service.GetHistoryAsync(employeeId)));
    }

    [HttpPost("increment")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> RequestIncrement([FromBody] IncrementRequestDto dto)
    {
        var by = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
        return Ok(ApiResponse<IncrementResponseDto>.Ok(await _service.RequestIncrementAsync(dto, by)));
    }

    [HttpPut("increment/{id:guid}/approve-reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveRejectIncrement(Guid id, [FromBody] IncrementApproveRejectDto dto)
    {
        var by = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
        return Ok(ApiResponse<IncrementResponseDto>.Ok(await _service.ApproveRejectIncrementAsync(id, dto, by)));
    }

    [HttpGet("increments")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> GetIncrements([FromQuery] Guid? employeeId, [FromQuery] PaginationParams pagination)
    {
        return Ok(await _service.GetIncrementsPagedAsync(employeeId, pagination));
    }
}
