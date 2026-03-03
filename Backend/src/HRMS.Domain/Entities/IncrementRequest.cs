using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

public class IncrementRequest : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public decimal CurrentCTC { get; set; }
    public decimal IncrementPercentage { get; set; }
    public decimal NewCTC { get; set; }
    public string Justification { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public IncrementStatus Status { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Remarks { get; set; }
    public Guid? NewSalaryStructureId { get; set; }
    public SalaryStructure? NewSalaryStructure { get; set; }
}
