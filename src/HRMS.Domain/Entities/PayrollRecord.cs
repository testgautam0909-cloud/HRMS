using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

public class PayrollRecord : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal HouseRentAllowance { get; set; }
    public decimal TransportAllowance { get; set; }
    public decimal MedicalAllowance { get; set; }
    public decimal SpecialAllowance { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal ProvidentFund { get; set; }
    public decimal ProfessionalTax { get; set; }
    public decimal IncomeTax { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal AttendanceDeduction { get; set; }
    public decimal UnpaidLeaveDeduction { get; set; }
    public decimal BonusAmount { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public int WorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public decimal TotalWorkHours { get; set; }
    public PayrollStatus Status { get; set; }
    public bool IsLocked { get; set; }
    public string? LockedBy { get; set; }
    public DateTime? LockedAt { get; set; }
    public string? PaidBy { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? OverrideReason { get; set; }
    public string? OverriddenBy { get; set; }
    public DateTime? OverriddenAt { get; set; }
    public uint RowVersion { get; set; }
    public SalarySlip? SalarySlip { get; set; }
}
