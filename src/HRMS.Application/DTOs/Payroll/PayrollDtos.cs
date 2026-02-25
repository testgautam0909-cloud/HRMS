namespace HRMS.Application.DTOs.Payroll;

public class PayrollGenerateDto
{
    public int Month { get; set; }
    public int Year { get; set; }
}

public class PayrollResponseDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
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
    public string Status { get; set; } = string.Empty;
    public bool IsLocked { get; set; }
    public string? SalarySlipUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PayrollSummaryDto
{
    public int Month { get; set; }
    public int Year { get; set; }
    public int TotalEmployees { get; set; }
    public decimal TotalGrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNetSalary { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class PayrollOverrideDto
{
    public string Reason { get; set; } = string.Empty;
}
