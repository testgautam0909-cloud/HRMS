using HRMS.Domain.Entities;

namespace HRMS.Domain.Entities;

public class Shift : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public TimeSpan? BreakStartTime { get; set; }
    public TimeSpan? BreakEndTime { get; set; }
    public int GracePeriodMinutes { get; set; } = 15;
    public int EarlyDepartureMinutes { get; set; } = 15; 
    public bool IsDefault { get; set; } = false; 
    
    public ICollection<ShiftAssignment> ShiftAssignments { get; set; } = new List<ShiftAssignment>();
}