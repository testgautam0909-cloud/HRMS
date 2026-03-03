using AutoMapper;
using HRMS.Application.DTOs.Attendance;
using HRMS.Application.DTOs.Common;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Domain.Interfaces;
using HRMS.Shared.Exceptions;
using HRMS.Shared.Helpers;
using HRMS.Shared.Wrappers;

namespace HRMS.Application.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AttendanceService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<AttendanceResponseDto> CheckInAsync(Guid employeeId, CheckInDto dto, string performedBy)
    {
        if (dto.Date.Date > DateTime.UtcNow.Date)
            throw new Shared.Exceptions.ValidationException(
                new Dictionary<string, string[]> { { "Date", new[] { "Cannot check in for a future date." } } });

        var existing = await _unitOfWork.Attendances.GetTodayRecordAsync(employeeId, dto.Date);
        if (existing != null)
            throw new ConflictException("Employee already has a check-in record for this date.");

        var record = new AttendanceRecord
        {
            EmployeeId = employeeId,
            Date = dto.Date.Date,
            CheckInTime = DateTime.UtcNow,
            Status = AttendanceStatus.Present,
            IpAddress = dto.IpAddress,
            CreatedBy = performedBy,
            UpdatedBy = performedBy
        };

        await _unitOfWork.Attendances.AddAsync(record);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<AttendanceResponseDto>(record);
    }

    public async Task<AttendanceResponseDto> CheckOutAsync(Guid employeeId, CheckOutDto dto, string performedBy)
    {
        var record = await _unitOfWork.Attendances.GetTodayRecordAsync(employeeId, dto.Date);
        if (record == null)
            throw new NotFoundException("AttendanceRecord", $"Employee {employeeId} on {dto.Date:yyyy-MM-dd}");

        if (record.CheckOutTime != null)
            throw new ConflictException("Employee has already checked out for this date.");

        record.CheckOutTime = DateTime.UtcNow;
        record.WorkHours = DateHelper.CalculateWorkHours(record.CheckInTime, record.CheckOutTime.Value);

        record.Status = record.WorkHours switch
        {
            >= 4 => AttendanceStatus.Present,
            >= 2 => AttendanceStatus.HalfDay,
            _ => AttendanceStatus.ShortHours
        };

        record.UpdatedBy = performedBy;
        _unitOfWork.Attendances.Update(record);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<AttendanceResponseDto>(record);
    }

    public async Task<IEnumerable<AttendanceResponseDto>> GetMonthlyRecordsAsync(Guid employeeId, int month, int year)
    {
        var records = await _unitOfWork.Attendances.GetMonthlyRecordsAsync(employeeId, month, year);
        return _mapper.Map<IEnumerable<AttendanceResponseDto>>(records);
    }

    public async Task<AttendanceSummaryDto> GetMonthlySummaryAsync(Guid employeeId, int month, int year)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);
        if (employee == null) throw new NotFoundException("Employee", employeeId);

        var records = await _unitOfWork.Attendances.GetMonthlyRecordsAsync(employeeId, month, year);
        var recordList = records.ToList();

        var workingDays = DateHelper.GetWorkingDaysInMonth(year, month);
        var present = recordList.Count(r => r.Status == AttendanceStatus.Present);
        var halfDays = recordList.Count(r => r.Status == AttendanceStatus.HalfDay);
        var totalHours = recordList.Where(r => r.WorkHours.HasValue).Sum(r => r.WorkHours!.Value);

        return new AttendanceSummaryDto
        {
            EmployeeId = employeeId,
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            Month = month,
            Year = year,
            TotalWorkingDays = workingDays,
            DaysPresent = present,
            DaysAbsent = workingDays - present - halfDays,
            HalfDays = halfDays,
            TotalWorkHours = totalHours,
            AverageDailyHours = recordList.Count > 0 ? Math.Round(totalHours / recordList.Count, 2) : 0
        };
    }

    public async Task<AttendanceResponseDto> CorrectAttendanceAsync(AttendanceCorrectionDto dto, string performedBy)
    {
        var record = await _unitOfWork.Attendances.GetByIdAsync(dto.AttendanceId);
        if (record == null) throw new NotFoundException("AttendanceRecord", dto.AttendanceId);

        if (dto.CheckInTime.HasValue) record.CheckInTime = dto.CheckInTime.Value;
        if (dto.CheckOutTime.HasValue)
        {
            record.CheckOutTime = dto.CheckOutTime.Value;
            record.WorkHours = DateHelper.CalculateWorkHours(record.CheckInTime, record.CheckOutTime.Value);
            record.Status = record.WorkHours switch
            {
                >= 4 => AttendanceStatus.Present,
                >= 2 => AttendanceStatus.HalfDay,
                _ => AttendanceStatus.ShortHours
            };
        }

        record.CorrectionNote = dto.CorrectionNote;
        record.CorrectedBy = performedBy;
        record.UpdatedBy = performedBy;

        _unitOfWork.Attendances.Update(record);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<AttendanceResponseDto>(record);
    }

    public async Task<PagedResponse<IEnumerable<AttendanceResponseDto>>> GetAllAttendancePagedAsync(
        int month, int year, PaginationParams pagination)
    {
        var (items, totalCount) = await _unitOfWork.Attendances.GetPagedAsync(
            a => a.Date.Month == month && a.Date.Year == year,
            q => q.OrderBy(a => a.Date),
            pagination.Page,
            pagination.PageSize,
            a => a.Employee);

        var dtos = _mapper.Map<IEnumerable<AttendanceResponseDto>>(items);
        return PagedResponse<IEnumerable<AttendanceResponseDto>>.CreateResponse(
            dtos, pagination.Page, pagination.PageSize, totalCount, "Attendance records fetched successfully.");
    }
}
