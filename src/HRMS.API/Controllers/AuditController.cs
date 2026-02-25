using HRMS.Application.DTOs.Audit;
using HRMS.Application.DTOs.Common;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _service;
    public AuditController(IAuditService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetLogs([FromQuery] AuditLogFilterDto filter, [FromQuery] PaginationParams pagination)
    {
        return Ok(await _service.GetLogsPagedAsync(filter, pagination));
    }

    [HttpGet("{entityName}/{entityId}")]
    public async Task<IActionResult> GetEntityLogs(string entityName, string entityId)
    {
        return Ok(await _service.GetEntityLogsAsync(entityName, entityId));
    }
}
