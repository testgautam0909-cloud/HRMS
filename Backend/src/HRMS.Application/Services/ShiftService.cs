using AutoMapper;
using HRMS.Application.DTOs.Shift;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Domain.Interfaces;
using HRMS.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Application.Services;

public class ShiftService : IShiftService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ShiftService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ShiftDto>> GetAllShiftsAsync()
    {
        var shifts = await _unitOfWork.Shifts.GetAllAsync();
        return _mapper.Map<IEnumerable<ShiftDto>>(shifts);
    }

    public async Task<ShiftDto?> GetShiftByIdAsync(Guid id)
    {
        var shift = await _unitOfWork.Shifts.GetByIdAsync(id);
        return shift != null ? _mapper.Map<ShiftDto>(shift) : null;
    }

    public async Task<ShiftDto> CreateShiftAsync(CreateShiftDto dto, string performedBy)
    {
        // If setting this shift as default, unset existing default
        if (dto.IsDefault)
        {
            var existingDefault = await _unitOfWork.Shifts.FirstOrDefaultAsync(s => s.IsDefault);
            
            if (existingDefault != null)
            {
                existingDefault.IsDefault = false;
                _unitOfWork.Shifts.Update(existingDefault);
            }
        }

        var shift = _mapper.Map<Shift>(dto);
        shift.CreatedBy = performedBy;
        shift.UpdatedBy = performedBy;

        await _unitOfWork.Shifts.AddAsync(shift);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ShiftDto>(shift);
    }

    public async Task<ShiftDto> UpdateShiftAsync(UpdateShiftDto dto, string performedBy)
    {
        var shift = await _unitOfWork.Shifts.GetByIdAsync(dto.Id);
        if (shift == null)
            throw new NotFoundException("Shift", dto.Id);

        // If setting this shift as default, unset existing default
        if (dto.IsDefault)
        {
            var existingDefault = await _unitOfWork.Shifts.FirstOrDefaultAsync(s => s.Id != dto.Id && s.IsDefault);
            
            if (existingDefault != null)
            {
                existingDefault.IsDefault = false;
                _unitOfWork.Shifts.Update(existingDefault);
            }
        }

        _mapper.Map(dto, shift);
        shift.UpdatedBy = performedBy;

        _unitOfWork.Shifts.Update(shift);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ShiftDto>(shift);
    }

    public async Task DeleteShiftAsync(Guid id, string performedBy)
    {
        var shift = await _unitOfWork.Shifts.GetByIdAsync(id);
        if (shift == null)
            throw new NotFoundException("Shift", id);

        // Check if shift is assigned to any employees
        var activeAssignments = await _unitOfWork.ShiftAssignments.AnyAsync(sa => sa.ShiftId == id && sa.IsActive);

        if (activeAssignments)
            throw new ConflictException("Cannot delete shift as it is assigned to one or more employees.");

        _unitOfWork.Shifts.Remove(shift);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<ShiftAssignmentDto>> GetShiftAssignmentsAsync()
    {
        var assignments = await _unitOfWork.ShiftAssignments.FindAsync(sa => true);
        
        // Need to manually include related entities since FindAsync doesn't support Include
        var assignmentList = assignments.ToList();
        var employeeIds = assignmentList.Select(sa => sa.EmployeeId).Distinct().ToList();
        var shiftIds = assignmentList.Select(sa => sa.ShiftId).Distinct().ToList();
        
        var employees = await _unitOfWork.Employees.FindAsync(e => employeeIds.Contains(e.Id));
        var shifts = await _unitOfWork.Shifts.FindAsync(s => shiftIds.Contains(s.Id));
        
        var employeeDict = employees.ToDictionary(e => e.Id);
        var shiftDict = shifts.ToDictionary(s => s.Id);

        return assignmentList.Select(sa => new ShiftAssignmentDto
        {
            Id = sa.Id,
            EmployeeId = sa.EmployeeId,
            ShiftId = sa.ShiftId,
            AssignmentDate = sa.AssignmentDate,
            EndDate = sa.EndDate,
            Reason = sa.Reason,
            IsActive = sa.IsActive,
            EmployeeName = employeeDict.ContainsKey(sa.EmployeeId) ? 
                $"{employeeDict[sa.EmployeeId].FirstName} {employeeDict[sa.EmployeeId].LastName}" : "Unknown",
            ShiftName = shiftDict.ContainsKey(sa.ShiftId) ? 
                shiftDict[sa.ShiftId].Name : "Unknown"
        }).ToList();
    }

    public async Task<IEnumerable<ShiftAssignmentDto>> GetShiftAssignmentsByEmployeeAsync(Guid employeeId)
    {
        var assignments = await _unitOfWork.ShiftAssignments.FindAsync(sa => sa.EmployeeId == employeeId);
        
        // Need to manually include related entities
        var assignmentList = assignments.ToList();
        var shiftIds = assignmentList.Select(sa => sa.ShiftId).Distinct().ToList();
        var shifts = await _unitOfWork.Shifts.FindAsync(s => shiftIds.Contains(s.Id));
        var shiftDict = shifts.ToDictionary(s => s.Id);
        
        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);

        return assignmentList.Select(sa => new ShiftAssignmentDto
        {
            Id = sa.Id,
            EmployeeId = sa.EmployeeId,
            ShiftId = sa.ShiftId,
            AssignmentDate = sa.AssignmentDate,
            EndDate = sa.EndDate,
            Reason = sa.Reason,
            IsActive = sa.IsActive,
            EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Unknown",
            ShiftName = shiftDict.ContainsKey(sa.ShiftId) ? 
                shiftDict[sa.ShiftId].Name : "Unknown"
        }).ToList();
    }

    public async Task<ShiftAssignmentDto> AssignShiftToEmployeeAsync(CreateShiftAssignmentDto dto, string performedBy)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(dto.EmployeeId);
        if (employee == null)
            throw new NotFoundException("Employee", dto.EmployeeId);

        var shift = await _unitOfWork.Shifts.GetByIdAsync(dto.ShiftId);
        if (shift == null)
            throw new NotFoundException("Shift", dto.ShiftId);

        // Check if there's already an assignment for this employee on the same date
        var existingAssignment = await _unitOfWork.ShiftAssignments.FirstOrDefaultAsync(sa => 
            sa.EmployeeId == dto.EmployeeId 
            && sa.AssignmentDate.Date == dto.AssignmentDate.Date 
            && sa.IsActive);

        if (existingAssignment != null)
        {
            existingAssignment.IsActive = false;
            existingAssignment.UpdatedBy = performedBy;
            _unitOfWork.ShiftAssignments.Update(existingAssignment);
        }

        var assignment = new ShiftAssignment
        {
            EmployeeId = dto.EmployeeId,
            ShiftId = dto.ShiftId,
            AssignmentDate = dto.AssignmentDate,
            EndDate = dto.EndDate,
            Reason = dto.Reason,
            IsActive = true,
            CreatedBy = performedBy,
            UpdatedBy = performedBy
        };

        await _unitOfWork.ShiftAssignments.AddAsync(assignment);
        await _unitOfWork.SaveChangesAsync();

        return new ShiftAssignmentDto
        {
            Id = assignment.Id,
            EmployeeId = assignment.EmployeeId,
            ShiftId = assignment.ShiftId,
            AssignmentDate = assignment.AssignmentDate,
            EndDate = assignment.EndDate,
            Reason = assignment.Reason,
            IsActive = assignment.IsActive,
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            ShiftName = shift.Name
        };
    }

    public async Task<EmployeeShiftScheduleDto> GetEmployeeShiftScheduleAsync(Guid employeeId)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);
        if (employee == null)
            throw new NotFoundException("Employee", employeeId);

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

    public async Task DeleteShiftAssignmentAsync(Guid id, string performedBy)
    {
        var assignment = await _unitOfWork.ShiftAssignments.GetByIdAsync(id);
        if (assignment == null)
            throw new NotFoundException("ShiftAssignment", id);

        _unitOfWork.ShiftAssignments.Remove(assignment);
        await _unitOfWork.SaveChangesAsync();
    }
}