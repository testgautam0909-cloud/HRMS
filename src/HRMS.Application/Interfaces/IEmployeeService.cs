using HRMS.Application.DTOs.Common;
using HRMS.Application.DTOs.Employee;
using HRMS.Shared.Wrappers;

namespace HRMS.Application.Interfaces;

public interface IEmployeeService
{
    Task<EmployeeResponseDto> CreateAsync(EmployeeCreateDto dto, string performedBy);
    Task<EmployeeResponseDto> GetByIdAsync(Guid id);
    Task<EmployeeProfileDto> GetProfileAsync(Guid id);
    Task<EmployeeResponseDto> GetByUserIdAsync(string userId);
    Task<PagedResponse<IEnumerable<EmployeeSummaryDto>>> GetAllPagedAsync(string? searchTerm, Guid? departmentId, bool? isActive, PaginationParams pagination);
    Task<EmployeeResponseDto> UpdateAsync(Guid id, EmployeeUpdateDto dto, string performedBy);
    Task DeactivateAsync(Guid id, EmployeeDeactivateDto dto, string performedBy);
    Task<ExperienceHistoryDto> AddExperienceAsync(Guid employeeId, ExperienceHistoryCreateDto dto, string performedBy);
    Task<IEnumerable<DepartmentDto>> GetDepartmentsAsync();
    Task<DepartmentDto> CreateDepartmentAsync(DepartmentCreateDto dto, string performedBy);
    Task<IEnumerable<DesignationDto>> GetDesignationsAsync();
    Task<DesignationDto> CreateDesignationAsync(DesignationCreateDto dto, string performedBy);
}
