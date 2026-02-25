using System.Security.Claims;
using HRMS.Application.DTOs.Document;
using HRMS.Application.Interfaces;
using HRMS.Domain.Enums;
using HRMS.Shared.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _service;
    public DocumentController(IDocumentService service) => _service = service;

    [HttpPost]
    public async Task<IActionResult> Upload([FromForm] DocumentUploadDto dto)
    {
        var by = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
        var result = await _service.UploadAsync(dto, by);
        return Ok(ApiResponse<DocumentResponseDto>.Ok(result, "Document uploaded."));
    }

    [HttpGet("employee/{employeeId:guid}")]
    public async Task<IActionResult> GetByEmployee(Guid employeeId)
    {
        return Ok(ApiResponse<IEnumerable<DocumentResponseDto>>.Ok(await _service.GetByEmployeeAsync(employeeId)));
    }

    [HttpGet("employee/{employeeId:guid}/category/{category}")]
    public async Task<IActionResult> GetByCategory(Guid employeeId, DocumentCategory category)
    {
        return Ok(ApiResponse<IEnumerable<DocumentResponseDto>>.Ok(
            await _service.GetByEmployeeAndCategoryAsync(employeeId, category)));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var by = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
        await _service.DeleteAsync(id, by);
        return Ok(ApiResponse<object>.Ok(null!, "Document deleted."));
    }
}
