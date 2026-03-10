using System;
using System.Collections.Generic;

namespace HRMS.Application.DTOs.Dashboard
{
    public class DashboardSummaryDto
    {
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int OnLeaveTodayCount { get; set; }
        public List<EmployeeAvatarDto> OnLeaveTodayAvatars { get; set; } = new();
        public double AttendanceRateToday { get; set; }
        public int PendingLeavesCount { get; set; }
        public bool PayrollProcessedThisMonth { get; set; }
        public double AvgWorkingHoursMonth { get; set; }
        public List<ChartDataDto> AttendanceTrend { get; set; } = new();
        public List<ChartDataDto> LeaveDistribution { get; set; } = new();
        public List<RecentActivityDto> RecentActivities { get; set; } = new();
    }

    public class EmployeeAvatarDto
    {
        public string Name { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string Initials { get; set; } = string.Empty;
    }

    public class ChartDataDto
    {
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; }
    }

    public class RecentActivityDto
    {
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } = string.Empty; // e.g., "Leave", "Attendance", "Payroll"
    }
}
