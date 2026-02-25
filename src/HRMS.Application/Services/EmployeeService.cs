using AutoMapper;
using HRMS.Application.DTOs.Common;
using HRMS.Application.DTOs.Employee;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Domain.Interfaces;
using HRMS.Shared.Exceptions;
using HRMS.Shared.Helpers;
using HRMS.Shared.Wrappers;

namespace HRMS.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuditService _auditService;

    public EmployeeService(IUnitOfWork unitOfWork, IMapper mapper, IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _auditService = auditService;
    }

    public async Task<EmployeeResponseDto> CreateAsync(EmployeeCreateDto dto, string performedBy)
    {
        var existing = await _unitOfWork.Employees.GetByEmailAsync(dto.Email);
        if (existing != null) throw new ConflictException("An employee with this email already exists.");

        var dept = await _unitOfWork.Departments.GetByIdAsync(dto.DepartmentId);
        if (dept == null || dept.IsDeleted) throw new NotFoundException("Department", dto.DepartmentId);

        var desig = await _unitOfWork.Designations.GetByIdAsync(dto.DesignationId);
        if (desig == null || desig.IsDeleted) throw new NotFoundException("Designation", dto.DesignationId);

        var year = dto.JoiningDate.Year;
        var sequence = await _unitOfWork.Employees.GetNextSequenceForYearAsync(year);

        var employee = _mapper.Map<Employee>(dto);
        employee.EmployeeCode = StringHelper.GenerateEmployeeCode(year, sequence);
        employee.IsActive = true;
        employee.CreatedBy = performedBy;
        employee.UpdatedBy = performedBy;

        await _unitOfWork.Employees.AddAsync(employee);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync("Employee", employee.Id.ToString(), AuditAction.Created, performedBy,
            newValues: System.Text.Json.JsonSerializer.Serialize(new { employee.EmployeeCode, employee.Email }));

        var created = await _unitOfWork.Employees.GetWithDetailsAsync(employee.Id);
        return _mapper.Map<EmployeeResponseDto>(created);
    }

    public async Task<EmployeeResponseDto> GetByIdAsync(Guid id)
    {
        var employee = await _unitOfWork.Employees.GetWithDetailsAsync(id);
        if (employee == null) throw new NotFoundException("Employee", id);
        return _mapper.Map<EmployeeResponseDto>(employee);
    }

    public async Task<EmployeeProfileDto> GetProfileAsync(Guid id)
    {
        var employee = await _unitOfWork.Employees.GetWithDetailsAsync(id);
        if (employee == null) throw new NotFoundException("Employee", id);
        return _mapper.Map<EmployeeProfileDto>(employee);
    }

    public async Task<EmployeeResponseDto> GetByUserIdAsync(string userId)
    {
        var employee = await _unitOfWork.Employees.GetByUserIdAsync(userId);
        if (employee == null) throw new NotFoundException("Employee", userId);
        return _mapper.Map<EmployeeResponseDto>(employee);
    }

    public async Task<PagedResponse<IEnumerable<EmployeeSummaryDto>>> GetAllPagedAsync(
        string? searchTerm, Guid? departmentId, bool? isActive, PaginationParams pagination)
    {
        var (items, totalCount) = await _unitOfWork.Employees.GetEmployeesPagedAsync(
            searchTerm, departmentId, isActive, pagination.Page, pagination.PageSize);

        var dtos = _mapper.Map<IEnumerable<EmployeeSummaryDto>>(items);
        return PagedResponse<IEnumerable<EmployeeSummaryDto>>.CreateResponse(
            dtos, pagination.Page, pagination.PageSize, totalCount, "Employees fetched successfully.");
    }

    public async Task<EmployeeResponseDto> UpdateAsync(Guid id, EmployeeUpdateDto dto, string performedBy)
    {
        var employee = await _unitOfWork.Employees.GetWithDetailsAsync(id);
        if (employee == null) throw new NotFoundException("Employee", id);

        var oldValues = System.Text.Json.JsonSerializer.Serialize(new { employee.Phone, employee.Address, employee.EmergencyContact });

        if (dto.Phone != null) employee.Phone = dto.Phone;
        if (dto.Address != null) employee.Address = dto.Address;
        if (dto.EmergencyContact != null) employee.EmergencyContact = dto.EmergencyContact;
        if (dto.DepartmentId.HasValue) employee.DepartmentId = dto.DepartmentId.Value;
        if (dto.DesignationId.HasValue) employee.DesignationId = dto.DesignationId.Value;
        employee.UpdatedBy = performedBy;

        _unitOfWork.Employees.Update(employee);
        await _unitOfWork.SaveChangesAsync();

        var newValues = System.Text.Json.JsonSerializer.Serialize(new { employee.Phone, employee.Address, employee.EmergencyContact });
        await _auditService.LogAsync("Employee", id.ToString(), AuditAction.Updated, performedBy,
            oldValues: oldValues, newValues: newValues);

        var updated = await _unitOfWork.Employees.GetWithDetailsAsync(id);
        return _mapper.Map<EmployeeResponseDto>(updated);
    }

    public async Task DeactivateAsync(Guid id, EmployeeDeactivateDto dto, string performedBy)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id);
        if (employee == null) throw new NotFoundException("Employee", id);

        employee.IsActive = false;
        employee.DeactivatedAt = DateTime.UtcNow;
        employee.DeactivationReason = dto.Reason;
        employee.UpdatedBy = performedBy;

        _unitOfWork.Employees.Update(employee);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync("Employee", id.ToString(), AuditAction.Updated, performedBy,
            remarks: $"Deactivated: {dto.Reason}");
    }

    public async Task<ExperienceHistoryDto> AddExperienceAsync(Guid employeeId, ExperienceHistoryCreateDto dto, string performedBy)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);
        if (employee == null) throw new NotFoundException("Employee", employeeId);

        var experience = _mapper.Map<ExperienceHistory>(dto);
        experience.EmployeeId = employeeId;
        experience.CreatedBy = performedBy;
        experience.UpdatedBy = performedBy;

        await _unitOfWork.ExperienceHistories.AddAsync(experience);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<ExperienceHistoryDto>(experience);
    }

    public async Task<IEnumerable<DepartmentDto>> GetDepartmentsAsync()
    {
        var departments = await _unitOfWork.Departments.FindAsync(d => !d.IsDeleted);
        return _mapper.Map<IEnumerable<DepartmentDto>>(departments);
    }

    public async Task<DepartmentDto> CreateDepartmentAsync(DepartmentCreateDto dto, string performedBy)
    {
        var department = new Department
        {
            Name = dto.Name,
            Description = dto.Description,
            IsActive = true,
            CreatedBy = performedBy,
            UpdatedBy = performedBy
        };
        await _unitOfWork.Departments.AddAsync(department);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<DepartmentDto>(department);
    }

    public async Task<IEnumerable<DesignationDto>> GetDesignationsAsync()
    {
        var designations = await _unitOfWork.Designations.FindAsync(d => !d.IsDeleted);
        return _mapper.Map<IEnumerable<DesignationDto>>(designations);
    }

    public async Task<DesignationDto> CreateDesignationAsync(DesignationCreateDto dto, string performedBy)
    {
        var designation = new Designation
        {
            Title = dto.Title,
            IsActive = true,
            CreatedBy = performedBy,
            UpdatedBy = performedBy
        };
        await _unitOfWork.Designations.AddAsync(designation);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<DesignationDto>(designation);
    }
}
