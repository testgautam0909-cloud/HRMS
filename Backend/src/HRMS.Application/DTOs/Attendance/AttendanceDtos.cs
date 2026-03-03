namespace HRMS.Application.DTOs.Attendance;

public class CheckInDto
{
    public DateTime Date { get; set; }
    public string? IpAddress { get; set; }
}

public class CheckOutDto
{
    public DateTime Date { get; set; }
}

public class AttendanceResponseDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public decimal? WorkHours { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CorrectionNote { get; set; }
}

public class AttendanceSummaryDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public int TotalWorkingDays { get; set; }
    public int DaysPresent { get; set; }
    public int DaysAbsent { get; set; }
    public int HalfDays { get; set; }
    public decimal TotalWorkHours { get; set; }
    public decimal AverageDailyHours { get; set; }
}

public class AttendanceCorrectionDto
{
    public Guid AttendanceId { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string CorrectionNote { get; set; } = string.Empty;
}
