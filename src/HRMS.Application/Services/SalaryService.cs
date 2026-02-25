using AutoMapper;
using HRMS.Application.DTOs.Common;
using HRMS.Application.DTOs.Salary;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Domain.Interfaces;
using HRMS.Shared.Exceptions;
using HRMS.Shared.Wrappers;

namespace HRMS.Application.Services;

public class SalaryService : ISalaryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuditService _auditService;

    public SalaryService(IUnitOfWork unitOfWork, IMapper mapper, IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _auditService = auditService;
    }

    public async Task<SalaryStructureResponseDto> CreateStructureAsync(SalaryStructureCreateDto dto, string performedBy)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(dto.EmployeeId);
        if (employee == null) throw new NotFoundException("Employee", dto.EmployeeId);

        var existingActive = await _unitOfWork.Salaries.GetActiveStructureAsync(dto.EmployeeId);
        if (existingActive != null)
        {
            existingActive.IsActive = false;
            existingActive.EndDate = dto.EffectiveDate.AddDays(-1);
            existingActive.UpdatedBy = performedBy;
            _unitOfWork.Salaries.Update(existingActive);
        }

        var gross = dto.BasicSalary + dto.HouseRentAllowance + dto.TransportAllowance + dto.MedicalAllowance + dto.SpecialAllowance;
        var deductions = dto.ProvidentFund + dto.ProfessionalTax + dto.IncomeTax + dto.OtherDeductions;

        var structure = new SalaryStructure
        {
            EmployeeId = dto.EmployeeId,
            BasicSalary = dto.BasicSalary,
            HouseRentAllowance = dto.HouseRentAllowance,
            TransportAllowance = dto.TransportAllowance,
            MedicalAllowance = dto.MedicalAllowance,
            SpecialAllowance = dto.SpecialAllowance,
            GrossSalary = gross,
            ProvidentFund = dto.ProvidentFund,
            ProfessionalTax = dto.ProfessionalTax,
            IncomeTax = dto.IncomeTax,
            OtherDeductions = dto.OtherDeductions,
            TotalDeductions = deductions,
            NetSalary = gross - deductions,
            CTC = gross + (dto.BasicSalary * 0.12m),
            EffectiveDate = dto.EffectiveDate,
            IsActive = true,
            CreatedBy = performedBy,
            UpdatedBy = performedBy
        };

        await _unitOfWork.Salaries.AddAsync(structure);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<SalaryStructureResponseDto>(structure);
    }

    public async Task<SalaryStructureResponseDto> GetActiveStructureAsync(Guid employeeId)
    {
        var s = await _unitOfWork.Salaries.GetActiveStructureAsync(employeeId);
        if (s == null) throw new NotFoundException("SalaryStructure", employeeId);
        return _mapper.Map<SalaryStructureResponseDto>(s);
    }

    public async Task<IEnumerable<SalaryStructureResponseDto>> GetHistoryAsync(Guid employeeId)
    {
        var h = await _unitOfWork.Salaries.GetHistoryAsync(employeeId);
        return _mapper.Map<IEnumerable<SalaryStructureResponseDto>>(h);
    }

    public async Task<IncrementResponseDto> RequestIncrementAsync(IncrementRequestDto dto, string performedBy)
    {
        var cs = await _unitOfWork.Salaries.GetActiveStructureAsync(dto.EmployeeId);
        if (cs == null) throw new NotFoundException("SalaryStructure", dto.EmployeeId);

        var request = new IncrementRequest
        {
            EmployeeId = dto.EmployeeId,
            CurrentCTC = cs.CTC,
            IncrementPercentage = dto.IncrementPercentage,
            NewCTC = Math.Round(cs.CTC * (1 + dto.IncrementPercentage / 100), 2),
            Justification = dto.Justification,
            RequestDate = dto.RequestDate,
            Status = IncrementStatus.Pending,
            CreatedBy = performedBy,
            UpdatedBy = performedBy
        };

        await _unitOfWork.Increments.AddAsync(request);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<IncrementResponseDto>(request);
    }

    public async Task<IncrementResponseDto> ApproveRejectIncrementAsync(Guid id, IncrementApproveRejectDto dto, string performedBy)
    {
        var r = await _unitOfWork.Increments.GetByIdAsync(id);
        if (r == null) throw new NotFoundException("IncrementRequest", id);
        if (r.Status != IncrementStatus.Pending) throw new ConflictException("Only pending requests can be processed.");

        if (dto.IsApproved)
        {
            r.Status = IncrementStatus.Approved;
            r.ApprovedBy = performedBy;
            r.ApprovedAt = DateTime.UtcNow;
        }
        else
        {
            r.Status = IncrementStatus.Rejected;
        }
        r.Remarks = dto.Remarks;
        r.UpdatedBy = performedBy;
        _unitOfWork.Increments.Update(r);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<IncrementResponseDto>(r);
    }

    public async Task<PagedResponse<IEnumerable<IncrementResponseDto>>> GetIncrementsPagedAsync(Guid? employeeId, PaginationParams pagination)
    {
        var (items, total) = await _unitOfWork.Increments.GetIncrementsPagedAsync(employeeId, pagination.Page, pagination.PageSize);
        var dtos = _mapper.Map<IEnumerable<IncrementResponseDto>>(items);
        return PagedResponse<IEnumerable<IncrementResponseDto>>.CreateResponse(dtos, pagination.Page, pagination.PageSize, total, "OK");
    }
}
