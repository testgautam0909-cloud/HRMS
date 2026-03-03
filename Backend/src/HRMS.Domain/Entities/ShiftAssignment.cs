using HRMS.Domain.Entities;

namespace HRMS.Domain.Entities;

public class ShiftAssignment : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public Guid ShiftId { get; set; }
    public Shift Shift { get; set; } = null!;
    public DateTime AssignmentDate { get; set; } 
    public DateTime? EndDate { get; set; } 
    public string? Reason { get; set; } 
    public bool IsActive { get; set; } = true;
}