using AutoMapper;
using HRMS.Application.DTOs.Common;
using HRMS.Application.DTOs.Employee;
using HRMS.Application.DTOs.Shift;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Domain.Interfaces;
using HRMS.Shared.Exceptions;
using HRMS.Shared.Helpers;
using HRMS.Shared.Wrappers;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public EmployeeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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

        var created = await _unitOfWork.Employees.GetWithDetailsAsync(employee.Id);
        return _mapper.Map<EmployeeResponseDto>(created);
    }

    public async Task<EmployeeResponseDto> GetByIdAsync(Guid id)
    {
        var employee = await _unitOfWork.Employees.GetWithDetailsAsync(id);
        if (employee == null) throw new NotFoundException("Employee", id);
        var dto = _mapper.Map<EmployeeResponseDto>(employee);
        dto.CurrentShift = await GetCurrentShiftNameAsync(id);
        return dto;
    }

    public async Task<EmployeeProfileDto> GetProfileAsync(Guid id)
    {
        var employee = await _unitOfWork.Employees.GetWithDetailsAsync(id);
        if (employee == null) throw new NotFoundException("Employee", id);
        var dto = _mapper.Map<EmployeeProfileDto>(employee);
        dto.CurrentShift = await GetCurrentShiftNameAsync(id);
        return dto;
    }

    public async Task<EmployeeResponseDto> GetByUserIdAsync(string userId)
    {
        var employee = await _unitOfWork.Employees.GetByUserIdAsync(userId);
        if (employee == null) throw new NotFoundException("Employee", userId);
        var dto = _mapper.Map<EmployeeResponseDto>(employee);
        dto.CurrentShift = await GetCurrentShiftNameAsync(employee.Id);
        return dto;
    }

    public async Task<PagedResponse<IEnumerable<EmployeeSummaryDto>>> GetAllPagedAsync(
        string? searchTerm, Guid? departmentId, bool? isActive, PaginationParams pagination)
    {
        var (items, totalCount) = await _unitOfWork.Employees.GetEmployeesPagedAsync(
            searchTerm, departmentId, isActive, pagination.Page, pagination.PageSize);

        var dtos = _mapper.Map<IEnumerable<EmployeeSummaryDto>>(items).ToList();
        
        if (dtos.Any())
        {
            var employeeIds = dtos.Select(d => d.Id).ToList();
            var now = DateTime.UtcNow.Date;
            var assignments = await _unitOfWork.ShiftAssignments.FindAsync(sa =>
                employeeIds.Contains(sa.EmployeeId) &&
                sa.IsActive &&
                sa.AssignmentDate.Date <= now &&
                (sa.EndDate == null || sa.EndDate.Value.Date >= now));
            
            var assignmentList = assignments.ToList();
            var shiftIds = assignmentList.Select(sa => sa.ShiftId).Distinct().ToList();
            var shifts = await _unitOfWork.Shifts.FindAsync(s => shiftIds.Contains(s.Id));
            var shiftDict = shifts.ToDictionary(s => s.Id);
            
            var defaultShift = await _unitOfWork.Shifts.FirstOrDefaultAsync(s => s.IsDefault);

            foreach (var dto in dtos)
            {
                var assignment = assignmentList.FirstOrDefault(sa => sa.EmployeeId == dto.Id);
                if (assignment != null && shiftDict.TryGetValue(assignment.ShiftId, out var shift))
                {
                    dto.CurrentShift = shift.Name;
                }
                else
                {
                    dto.CurrentShift = defaultShift?.Name;
                }
            }
        }

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

    public async Task<EmployeeShiftScheduleDto> GetEmployeeShiftScheduleAsync(Guid employeeId)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);
        if (employee == null) throw new NotFoundException("Employee", employeeId);

        var assignments = await _unitOfWork.ShiftAssignments.FindAsync(sa => sa.EmployeeId == employeeId);
        
        // Need to manually include shifts
        var assignmentList = assignments.ToList();
        var shiftIds = assignmentList.Select(sa => sa.ShiftId).Distinct().ToList();
        var shifts = await _unitOfWork.Shifts.FindAsync(s => shiftIds.Contains(s.Id));
        var shiftDict = shifts.ToDictionary(s => s.Id);

        var schedule = new EmployeeShiftScheduleDto
        {
            EmployeeId = employeeId,
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            ShiftDetails = assignmentList.Select(sa => new EmployeeShiftDetailDto
            {
                ShiftId = sa.ShiftId,
                ShiftName = shiftDict.ContainsKey(sa.ShiftId) ? shiftDict[sa.ShiftId].Name : "Unknown",
                StartTime = shiftDict.ContainsKey(sa.ShiftId) ? shiftDict[sa.ShiftId].StartTime : TimeSpan.Zero,
                EndTime = shiftDict.ContainsKey(sa.ShiftId) ? shiftDict[sa.ShiftId].EndTime : TimeSpan.Zero,
                BreakStartTime = shiftDict.ContainsKey(sa.ShiftId) ? shiftDict[sa.ShiftId].BreakStartTime : null,
                BreakEndTime = shiftDict.ContainsKey(sa.ShiftId) ? shiftDict[sa.ShiftId].BreakEndTime : null,
                AssignmentDate = sa.AssignmentDate,
                EndDate = sa.EndDate,
                IsActive = sa.IsActive,
                Reason = sa.Reason
            }).ToList()
        };

        return schedule;
    }

    private async Task<string?> GetCurrentShiftNameAsync(Guid employeeId)
    {
        var now = DateTime.UtcNow.Date;
        var assignment = await _unitOfWork.ShiftAssignments.FirstOrDefaultAsync(sa =>
            sa.EmployeeId == employeeId &&
            sa.IsActive &&
            sa.AssignmentDate.Date <= now &&
            (sa.EndDate == null || sa.EndDate.Value.Date >= now));

        if (assignment != null)
        {
            var shift = await _unitOfWork.Shifts.GetByIdAsync(assignment.ShiftId);
            return shift?.Name;
        }

        var defaultShift = await _unitOfWork.Shifts.FirstOrDefaultAsync(s => s.IsDefault);
        return defaultShift?.Name;
    }
}