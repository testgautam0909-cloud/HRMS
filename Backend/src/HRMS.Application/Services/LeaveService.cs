using AutoMapper;
using HRMS.Application.DTOs.Common;
using HRMS.Application.DTOs.Leave;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Domain.Interfaces;
using HRMS.Shared.Exceptions;
using HRMS.Shared.Helpers;
using HRMS.Shared.Wrappers;

namespace HRMS.Application.Services;

public class LeaveService : ILeaveService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;

    public LeaveService(IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _emailService = emailService;
    }

    public async Task<LeaveResponseDto> ApplyLeaveAsync(Guid employeeId, LeaveApplyDto dto, string performedBy)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);
        if (employee == null) throw new NotFoundException("Employee", employeeId);

        var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(dto.LeaveTypeId);
        if (leaveType == null) throw new NotFoundException("LeaveType", dto.LeaveTypeId);

        var hasOverlap = await _unitOfWork.Leaves.HasOverlappingLeaveAsync(employeeId, dto.StartDate, dto.EndDate);
        if (hasOverlap) throw new ConflictException("Leave application overlaps with an existing leave.");

        if (leaveType.IsPaid)
        {
            var balance = await _unitOfWork.LeaveBalances.GetBalanceAsync(employeeId, dto.LeaveTypeId, dto.StartDate.Year);
            if (balance == null || balance.Remaining < dto.Duration)
                throw new Shared.Exceptions.ValidationException(
                    new Dictionary<string, string[]> { { "Duration", new[] { "Insufficient leave balance." } } });
        }

        var application = new LeaveApplication
        {
            EmployeeId = employeeId,
            LeaveTypeId = dto.LeaveTypeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Duration = dto.Duration,
            Reason = dto.Reason,
            Status = LeaveStatus.Submitted,
            CreatedBy = performedBy,
            UpdatedBy = performedBy
        };

        await _unitOfWork.Leaves.AddAsync(application);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<LeaveResponseDto>(application);
    }

    public async Task<LeaveResponseDto> ApproveRejectLeaveAsync(Guid leaveId, LeaveApproveRejectDto dto, string performedBy)
    {
        var leave = await _unitOfWork.Leaves.GetByIdAsync(leaveId);
        if (leave == null) throw new NotFoundException("LeaveApplication", leaveId);

        if (leave.Status != LeaveStatus.Submitted)
            throw new Shared.Exceptions.ValidationException(
                new Dictionary<string, string[]> { { "Status", new[] { "Only submitted leaves can be approved or rejected." } } });

        var employee = await _unitOfWork.Employees.GetByIdAsync(leave.EmployeeId);

        if (dto.IsApproved)
        {
            leave.Status = LeaveStatus.Approved;
            leave.ApprovedBy = performedBy;
            leave.ApprovedAt = DateTime.UtcNow;
            leave.Remarks = dto.Remarks;

            var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(leave.LeaveTypeId);
            if (leaveType != null && leaveType.IsPaid)
            {
                var balance = await _unitOfWork.LeaveBalances.GetBalanceAsync(leave.EmployeeId, leave.LeaveTypeId, leave.StartDate.Year);
                if (balance != null)
                {
                    balance.Used += leave.Duration;
                    balance.Remaining -= leave.Duration;
                    _unitOfWork.LeaveBalances.Update(balance);
                }
            }

        }
        else
        {
            leave.Status = LeaveStatus.Rejected;
            leave.RejectionReason = dto.Remarks;
            leave.Remarks = dto.Remarks;

        }

        leave.UpdatedBy = performedBy;
        _unitOfWork.Leaves.Update(leave);
        await _unitOfWork.SaveChangesAsync();

        if (employee != null)
        {
            var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(leave.LeaveTypeId);
            _ = _emailService.SendLeaveStatusEmailAsync(
                employee.Email, $"{employee.FirstName} {employee.LastName}",
                leaveType?.Name ?? "Leave", leave.Status.ToString(), dto.Remarks);
        }

        return _mapper.Map<LeaveResponseDto>(leave);
    }

    public async Task<LeaveResponseDto> WithdrawLeaveAsync(Guid leaveId, Guid employeeId)
    {
        var leave = await _unitOfWork.Leaves.GetByIdAsync(leaveId);
        if (leave == null) throw new NotFoundException("LeaveApplication", leaveId);
        if (leave.EmployeeId != employeeId) throw new ForbiddenException();
        if (leave.Status != LeaveStatus.Submitted)
            throw new Shared.Exceptions.ValidationException(
                new Dictionary<string, string[]> { { "Status", new[] { "Only submitted leaves can be withdrawn." } } });

        leave.Status = LeaveStatus.Withdrawn;
        leave.UpdatedBy = employeeId.ToString();
        _unitOfWork.Leaves.Update(leave);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<LeaveResponseDto>(leave);
    }

    public async Task<LeaveResponseDto> CancelLeaveAsync(Guid leaveId, string performedBy)
    {
        var leave = await _unitOfWork.Leaves.GetByIdAsync(leaveId);
        if (leave == null) throw new NotFoundException("LeaveApplication", leaveId);
        if (leave.Status != LeaveStatus.Approved)
            throw new Shared.Exceptions.ValidationException(
                new Dictionary<string, string[]> { { "Status", new[] { "Only approved leaves can be cancelled." } } });

        leave.Status = LeaveStatus.Cancelled;
        leave.UpdatedBy = performedBy;

        var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(leave.LeaveTypeId);
        if (leaveType != null && leaveType.IsPaid)
        {
            var balance = await _unitOfWork.LeaveBalances.GetBalanceAsync(leave.EmployeeId, leave.LeaveTypeId, leave.StartDate.Year);
            if (balance != null)
            {
                balance.Used -= leave.Duration;
                balance.Remaining += leave.Duration;
                _unitOfWork.LeaveBalances.Update(balance);
            }
        }

        _unitOfWork.Leaves.Update(leave);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<LeaveResponseDto>(leave);
    }

    public async Task<IEnumerable<LeaveBalanceDto>> GetBalancesAsync(Guid employeeId, int year)
    {
        var balances = await _unitOfWork.LeaveBalances.GetAllBalancesAsync(employeeId, year);
        return _mapper.Map<IEnumerable<LeaveBalanceDto>>(balances);
    }

    public async Task<PagedResponse<IEnumerable<LeaveResponseDto>>> GetLeavesPagedAsync(
        Guid? employeeId, LeaveStatus? status, PaginationParams pagination)
    {
        var (items, totalCount) = await _unitOfWork.Leaves.GetLeavesPagedAsync(
            employeeId, status, pagination.Page, pagination.PageSize);

        var dtos = _mapper.Map<IEnumerable<LeaveResponseDto>>(items);
        return PagedResponse<IEnumerable<LeaveResponseDto>>.CreateResponse(
            dtos, pagination.Page, pagination.PageSize, totalCount, "Leave applications fetched successfully.");
    }

    public async Task<IEnumerable<LeaveTypeDto>> GetLeaveTypesAsync()
    {
        var types = await _unitOfWork.LeaveTypes.GetActiveTypesAsync();
        return _mapper.Map<IEnumerable<LeaveTypeDto>>(types);
    }

    public async Task<LeaveTypeDto> CreateLeaveTypeAsync(LeaveTypeCreateDto dto, string performedBy)
    {
        var leaveType = _mapper.Map<LeaveType>(dto);
        leaveType.IsActive = true;
        leaveType.CreatedBy = performedBy;
        leaveType.UpdatedBy = performedBy;

        await _unitOfWork.LeaveTypes.AddAsync(leaveType);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<LeaveTypeDto>(leaveType);
    }

    public async Task AllocateLeaveBalancesAsync(Guid employeeId, int year, string performedBy)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);
        if (employee == null) throw new NotFoundException("Employee", employeeId);

        var leaveTypes = await _unitOfWork.LeaveTypes.GetActiveTypesAsync();

        foreach (var lt in leaveTypes)
        {
            var existing = await _unitOfWork.LeaveBalances.GetBalanceAsync(employeeId, lt.Id, year);
            if (existing != null) continue;

            var allocation = lt.DefaultDays;
            if (employee.JoiningDate.Year == year)
            {
                allocation = (int)DateHelper.CalculateProRatedAllocation(lt.DefaultDays, employee.JoiningDate.Month);
            }

            var balance = new LeaveBalance
            {
                EmployeeId = employeeId,
                LeaveTypeId = lt.Id,
                Year = year,
                TotalAllocated = allocation,
                Used = 0,
                Remaining = allocation,
                CreatedBy = performedBy,
                UpdatedBy = performedBy
            };

            await _unitOfWork.LeaveBalances.AddAsync(balance);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<LeaveBalance>> GetLeaveBalancesForEmployeeAsync(Guid employeeId, int year)
    {
        return await _unitOfWork.LeaveBalances.GetAllBalancesAsync(employeeId, year);
    }
}
