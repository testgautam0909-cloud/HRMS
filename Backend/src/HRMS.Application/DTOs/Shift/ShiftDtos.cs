using System.ComponentModel.DataAnnotations;

namespace HRMS.Application.DTOs.Shift;

public class ShiftDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public TimeSpan? BreakStartTime { get; set; }
    public TimeSpan? BreakEndTime { get; set; }
    public int GracePeriodMinutes { get; set; } = 15;
    public int EarlyDepartureMinutes { get; set; } = 15;
    public bool IsDefault { get; set; }
}

public class CreateShiftDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public TimeSpan StartTime { get; set; }
    
    [Required]
    public TimeSpan EndTime { get; set; }
    
    public TimeSpan? BreakStartTime { get; set; }
    public TimeSpan? BreakEndTime { get; set; }
    public int GracePeriodMinutes { get; set; } = 15;
    public int EarlyDepartureMinutes { get; set; } = 15;
    public bool IsDefault { get; set; }
}

public class UpdateShiftDto
{
    [Required]
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public TimeSpan StartTime { get; set; }
    
    [Required]
    public TimeSpan EndTime { get; set; }
    
    public TimeSpan? BreakStartTime { get; set; }
    public TimeSpan? BreakEndTime { get; set; }
    public int GracePeriodMinutes { get; set; } = 15;
    public int EarlyDepartureMinutes { get; set; } = 15;
    public bool IsDefault { get; set; }
}

public class ShiftAssignmentDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid ShiftId { get; set; }
    public DateTime AssignmentDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Reason { get; set; }
    public bool IsActive { get; set; }
    public string? EmployeeName { get; set; }
    public string? ShiftName { get; set; }
}

public class CreateShiftAssignmentDto
{
    [Required]
    public Guid EmployeeId { get; set; }
    
    [Required]
    public Guid ShiftId { get; set; }
    
    [Required]
    public DateTime AssignmentDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    public string? Reason { get; set; }
}

public class EmployeeShiftScheduleDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public List<EmployeeShiftDetailDto> ShiftDetails { get; set; } = new();
}

public class EmployeeShiftDetailDto
{
    public Guid ShiftId { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public TimeSpan? BreakStartTime { get; set; }
    public TimeSpan? BreakEndTime { get; set; }
    public DateTime AssignmentDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? Reason { get; set; }
}