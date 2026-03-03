using AutoMapper;
using HRMS.Application.DTOs.Common;
using HRMS.Application.DTOs.Payroll;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Domain.Interfaces;
using HRMS.Shared.Exceptions;
using HRMS.Shared.Wrappers;

namespace HRMS.Application.Services;

public class PayrollService : IPayrollService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PayrollService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PayrollResponseDto>> GeneratePayrollAsync(PayrollGenerateDto dto, string performedBy)
    {
        var employees = dto.EmployeeIds != null && dto.EmployeeIds.Any()
            ? (await Task.WhenAll(dto.EmployeeIds.Select(id => _unitOfWork.Employees.GetByIdAsync(id))))
                .Where(e => e != null && e.IsActive).ToList()
            : (await _unitOfWork.Employees.FindAsync(e => e.IsActive)).Cast<Employee?>().ToList();

        var results = new List<PayrollRecord>();

        foreach (var emp in employees)
        {
            if (emp == null) continue;

            var existing = await _unitOfWork.Payrolls.GetByEmployeeAndPeriodAsync(emp.Id, dto.Month, dto.Year);
            if (existing != null) continue;

            var salary = await _unitOfWork.Salaries.GetActiveStructureAsync(emp.Id);
            if (salary == null) continue;

            var records = (await _unitOfWork.Attendances.GetMonthlyRecordsAsync(emp.Id, dto.Month, dto.Year)).ToList();
            var workingDays = Shared.Helpers.DateHelper.GetWorkingDaysInMonth(dto.Year, dto.Month);
            var present = records.Count(r => r.Status == AttendanceStatus.Present);
            var halfDays = records.Count(r => r.Status == AttendanceStatus.HalfDay);
            var effectiveDays = present + (halfDays * 0.5m);
            var absentDays = Math.Max(0, workingDays - (int)effectiveDays);
            var totalHours = records.Where(r => r.WorkHours.HasValue).Sum(r => r.WorkHours!.Value);

            var dailyRate = salary.GrossSalary / workingDays;
            var attendanceDeduction = Math.Round(absentDays * dailyRate, 2);
            var grossSalary = salary.GrossSalary - attendanceDeduction;
            var netSalary = grossSalary - salary.TotalDeductions;

            var payroll = new PayrollRecord
            {
                EmployeeId = emp.Id,
                Month = dto.Month,
                Year = dto.Year,
                BasicSalary = salary.BasicSalary,
                HouseRentAllowance = salary.HouseRentAllowance,
                TransportAllowance = salary.TransportAllowance,
                MedicalAllowance = salary.MedicalAllowance,
                SpecialAllowance = salary.SpecialAllowance,
                GrossSalary = Math.Round(grossSalary, 2),
                ProvidentFund = salary.ProvidentFund,
                ProfessionalTax = salary.ProfessionalTax,
                IncomeTax = salary.IncomeTax,
                OtherDeductions = salary.OtherDeductions,
                AttendanceDeduction = attendanceDeduction,
                UnpaidLeaveDeduction = 0,
                BonusAmount = 0,
                TotalDeductions = salary.TotalDeductions,
                NetSalary = Math.Round(netSalary, 2),
                WorkingDays = workingDays,
                PresentDays = (int)effectiveDays,
                AbsentDays = absentDays,
                TotalWorkHours = totalHours,
                Status = PayrollStatus.Generated,
                IsLocked = false,
                CreatedBy = performedBy,
                UpdatedBy = performedBy
            };

            await _unitOfWork.Payrolls.AddAsync(payroll);
            results.Add(payroll);
        }

        await _unitOfWork.SaveChangesAsync();
        
        return _mapper.Map<IEnumerable<PayrollResponseDto>>(results);
    }

    public async Task<PayrollResponseDto> GetByIdAsync(Guid id)
    {
        var r = await _unitOfWork.Payrolls.GetByIdAsync(id);
        if (r == null) throw new NotFoundException("PayrollRecord", id);
        return _mapper.Map<PayrollResponseDto>(r);
    }

    public async Task<PayrollResponseDto> GetByEmployeeAndPeriodAsync(Guid employeeId, int month, int year)
    {
        var r = await _unitOfWork.Payrolls.GetByEmployeeAndPeriodAsync(employeeId, month, year);
        if (r == null) throw new NotFoundException("PayrollRecord", $"{employeeId}/{month}/{year}");
        return _mapper.Map<PayrollResponseDto>(r);
    }

    public async Task<PagedResponse<IEnumerable<PayrollResponseDto>>> GetPayrollsPagedAsync(
        Guid? employeeId, int? month, int? year, PaginationParams pagination)
    {
        var (items, total) = await _unitOfWork.Payrolls.GetPayrollsPagedAsync(
            employeeId, month, year, pagination.Page, pagination.PageSize);
        var dtos = _mapper.Map<IEnumerable<PayrollResponseDto>>(items);
        return PagedResponse<IEnumerable<PayrollResponseDto>>.CreateResponse(
            dtos, pagination.Page, pagination.PageSize, total, "Payroll records fetched.");
    }

    public async Task<PayrollResponseDto> MarkAsPaidAsync(Guid payrollId, string performedBy)
    {
        var r = await _unitOfWork.Payrolls.GetByIdAsync(payrollId);
        if (r == null) throw new NotFoundException("PayrollRecord", payrollId);
        if (r.Status == PayrollStatus.Paid) throw new ConflictException("Already paid.");

        r.Status = PayrollStatus.Paid;
        r.PaidAt = DateTime.UtcNow;
        r.PaidBy = performedBy;
        r.IsLocked = true;
        r.UpdatedBy = performedBy;
        _unitOfWork.Payrolls.Update(r);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<PayrollResponseDto>(r);
    }

    public async Task<PayrollResponseDto> OverridePayrollAsync(Guid payrollId, PayrollOverrideDto dto, string performedBy)
    {
        var r = await _unitOfWork.Payrolls.GetByIdAsync(payrollId);
        if (r == null) throw new NotFoundException("PayrollRecord", payrollId);
        if (r.IsLocked) throw new ConflictException("Cannot override a locked record.");

        if (dto.BonusAmount.HasValue) r.BonusAmount += dto.BonusAmount.Value;
        if (dto.PenaltyAmount.HasValue) r.OtherDeductions += dto.PenaltyAmount.Value;
        if (dto.OverrideReason != null) r.OverrideReason = dto.OverrideReason;

        r.GrossSalary = r.BasicSalary + r.HouseRentAllowance + r.TransportAllowance +
            r.MedicalAllowance + r.SpecialAllowance + r.BonusAmount - r.AttendanceDeduction;
        r.TotalDeductions = r.ProvidentFund + r.ProfessionalTax + r.IncomeTax + r.OtherDeductions;
        r.NetSalary = r.GrossSalary - r.TotalDeductions;
        r.Status = PayrollStatus.Overridden;
        r.OverriddenBy = performedBy;
        r.OverriddenAt = DateTime.UtcNow;
        r.UpdatedBy = performedBy;

        _unitOfWork.Payrolls.Update(r);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<PayrollResponseDto>(r);
    }

    public async Task<PayrollResponseDto> RelockPayrollAsync(Guid payrollId, string performedBy)
    {
        var r = await _unitOfWork.Payrolls.GetByIdAsync(payrollId);
        if (r == null) throw new NotFoundException("PayrollRecord", payrollId);

        r.IsLocked = true;
        r.LockedBy = performedBy;
        r.LockedAt = DateTime.UtcNow;
        r.UpdatedBy = performedBy;
        _unitOfWork.Payrolls.Update(r);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<PayrollResponseDto>(r);
    }

    public async Task<PayrollSummaryDto> GetPayrollSummaryAsync(int month, int year)
    {
        var (all, _) = await _unitOfWork.Payrolls.GetPayrollsPagedAsync(null, month, year, 1, 10000);
        var list = all.ToList();

        return new PayrollSummaryDto
        {
            Month = month,
            Year = year,
            TotalEmployees = list.Count,
            TotalGrossSalary = list.Sum(p => p.GrossSalary),
            TotalDeductions = list.Sum(p => p.TotalDeductions),
            TotalNetSalary = list.Sum(p => p.NetSalary),
            TotalPaid = list.Count(p => p.Status == PayrollStatus.Paid),
            TotalPending = list.Count(p => p.Status == PayrollStatus.Generated)
        };
    }
}
