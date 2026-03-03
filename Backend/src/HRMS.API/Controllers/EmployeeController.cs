using System.Security.Claims;
using HRMS.Application.DTOs.Common;
using HRMS.Application.DTOs.Employee;
using HRMS.Application.DTOs.Shift;
using HRMS.Application.Interfaces;
using HRMS.Shared.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _service;
    public EmployeeController(IEmployeeService service) => _service = service;

    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Create([FromBody] EmployeeCreateDto dto)
    {
        var by = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
        var result = await _service.CreateAsync(dto, by);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<EmployeeResponseDto>.Ok(result, "Created."));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(ApiResponse<EmployeeResponseDto>.Ok(await _service.GetByIdAsync(id)));
    }

    [HttpGet("{id:guid}/profile")]
    public async Task<IActionResult> GetProfile(Guid id)
    {
        return Ok(ApiResponse<EmployeeProfileDto>.Ok(await _service.GetProfileAsync(id)));
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        return Ok(ApiResponse<EmployeeResponseDto>.Ok(await _service.GetByUserIdAsync(userId)));
    }

    [HttpGet]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] Guid? departmentId, [FromQuery] bool? isActive, [FromQuery] PaginationParams pagination)
    {
        return Ok(await _service.GetAllPagedAsync(search, departmentId, isActive, pagination));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] EmployeeUpdateDto dto)
    {
        var by = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
        return Ok(ApiResponse<EmployeeResponseDto>.Ok(await _service.UpdateAsync(id, dto, by)));
    }

    [HttpPut("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] EmployeeDeactivateDto dto)
    {
        var by = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
        await _service.DeactivateAsync(id, dto, by);
        return Ok(ApiResponse<object>.Ok(null!, "Employee deactivated."));
    }

    [HttpPost("{id:guid}/experience")]
    public async Task<IActionResult> AddExperience(Guid id, [FromBody] ExperienceHistoryCreateDto dto)
    {
        var by = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
        return Ok(ApiResponse<ExperienceHistoryDto>.Ok(await _service.AddExperienceAsync(id, dto, by)));
    }

    [HttpGet("departments")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDepartments()
    {
        return Ok(ApiResponse<IEnumerable<DepartmentDto>>.Ok(await _service.GetDepartmentsAsync()));
    }

    [HttpPost("departments")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateDepartment([FromBody] DepartmentCreateDto dto)
    {
        var by = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
        return Ok(ApiResponse<DepartmentDto>.Ok(await _service.CreateDepartmentAsync(dto, by)));
    }

    [HttpGet("designations")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDesignations()
    {
        return Ok(ApiResponse<IEnumerable<DesignationDto>>.Ok(await _service.GetDesignationsAsync()));
    }

    [HttpPost("designations")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateDesignation([FromBody] DesignationCreateDto dto)
    {
        var by = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
        return Ok(ApiResponse<DesignationDto>.Ok(await _service.CreateDesignationAsync(dto, by)));
    }

    [HttpGet("employment-types")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetEmploymentTypes()
    {
        var types = Enum.GetValues(typeof(HRMS.Domain.Enums.EmploymentType))
            .Cast<HRMS.Domain.Enums.EmploymentType>()
            .Select(e => new { Id = (int)e, Name = e.ToString() });
        return Ok(ApiResponse<object>.Ok(types));
    }

    [HttpGet("genders")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetGenders()
    {
        var genders = Enum.GetValues(typeof(HRMS.Domain.Enums.Gender))
            .Cast<HRMS.Domain.Enums.Gender>()
            .Select(e => new { Id = (int)e, Name = e.ToString() });
        return Ok(ApiResponse<object>.Ok(genders));
    }

    [HttpGet("{id:guid}/shift-schedule")]
    [Authorize(Roles = "Admin,HR,Manager,Employee")]
    public async Task<IActionResult> GetEmployeeShiftSchedule(Guid id)
    {
        var schedule = await _service.GetEmployeeShiftScheduleAsync(id);
        return Ok(ApiResponse<EmployeeShiftScheduleDto>.Ok(schedule, "Employee shift schedule retrieved successfully."));
    }
}
