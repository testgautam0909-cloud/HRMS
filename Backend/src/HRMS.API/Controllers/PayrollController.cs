using System.Security.Claims;
using HRMS.Application.DTOs.Common;
using HRMS.Application.DTOs.Payroll;
using HRMS.Application.Interfaces;
using HRMS.Shared.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PayrollController : ControllerBase
{
    private readonly IPayrollService _service;
    private readonly ISalarySlipService _slipService;
    private readonly ICloudinaryService _cloudinary;

    public PayrollController(IPayrollService service, ISalarySlipService slipService, ICloudinaryService cloudinary)
    {
        _service = service;
        _slipService = slipService;
        _cloudinary = cloudinary;
    }

    [HttpPost("generate")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Generate([FromBody] PayrollGenerateDto dto)
    {
        var by = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
        var results = await _service.GeneratePayrollAsync(dto, by);
        return Ok(ApiResponse<IEnumerable<PayrollResponseDto>>.Ok(results, "Payroll generated."));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(ApiResponse<PayrollResponseDto>.Ok(await _service.GetByIdAsync(id)));
    }

    [HttpGet("employee/{employeeId:guid}")]
    public async Task<IActionResult> GetByEmployee(Guid employeeId, [FromQuery] int? month, [FromQuery] int? year)
    {
        if (month.HasValue && year.HasValue)
        {
            var result = await _service.GetByEmployeeAndPeriodAsync(employeeId, month.Value, year.Value);
            return Ok(ApiResponse<IEnumerable<PayrollResponseDto>>.Ok(new[] { result }));
        }
        
        var history = await _service.GetEmployeePayrollHistoryAsync(employeeId);
        return Ok(ApiResponse<IEnumerable<PayrollResponseDto>>.Ok(history));
    }

    [HttpGet]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> GetAll([FromQuery] Guid? employeeId, [FromQuery] int? month, [FromQuery] int? year, [FromQuery] PaginationParams pagination)
    {
        return Ok(await _service.GetPayrollsPagedAsync(employeeId, month, year, pagination));
    }

    [HttpPut("{id:guid}/pay")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MarkAsPaid(Guid id)
    {
        var by = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
        return Ok(ApiResponse<PayrollResponseDto>.Ok(await _service.MarkAsPaidAsync(id, by)));
    }

    [HttpPut("{id:guid}/override")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Override(Guid id, [FromBody] PayrollOverrideDto dto)
    {
        var by = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
        return Ok(ApiResponse<PayrollResponseDto>.Ok(await _service.OverridePayrollAsync(id, dto, by)));
    }

    [HttpPut("{id:guid}/relock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Relock(Guid id)
    {
        var by = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
        return Ok(ApiResponse<PayrollResponseDto>.Ok(await _service.RelockPayrollAsync(id, by)));
    }

    [HttpGet("summary")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> GetSummary([FromQuery] int month, [FromQuery] int year)
    {
        return Ok(ApiResponse<PayrollSummaryDto>.Ok(await _service.GetPayrollSummaryAsync(month, year)));
    }

    [HttpGet("{id:guid}/salary-slip")]
    public async Task<IActionResult> DownloadSalarySlip(Guid id)
    {
        var pdf = await _slipService.GeneratePdfAsync(id);
        return File(pdf, "application/pdf", $"SalarySlip_{id}.pdf");
    }

    [HttpPost("{id:guid}/salary-slip/upload")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> UploadSalarySlip(Guid id)
    {
        var pdf = await _slipService.GeneratePdfAsync(id);
        var payroll = await _service.GetByIdAsync(id);
        var fileName = $"SalarySlip_{payroll.EmployeeCode}_{payroll.Month}_{payroll.Year}.pdf";
        var (_, secureUrl) = await _cloudinary.UploadPdfAsync(pdf, fileName, "hrms/salary-slips");
        return Ok(ApiResponse<object>.Ok(new { Url = secureUrl }, "Salary slip uploaded to Cloudinary."));
    }
}
