using HRMS.Application.DTOs.Dashboard;
using HRMS.Application.Interfaces;
using HRMS.Domain.Interfaces;
using HRMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            var today = DateTime.UtcNow.Date;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            
            var totalEmployees = await _unitOfWork.Employees.CountAsync();
            var activeEmployees = await _unitOfWork.Employees.CountAsync(e => e.IsActive);
            
            // On Leave Today
            var onLeaveToday = await _unitOfWork.Leaves
                .GetAllQueryable()
                .Where(l => l.Status == Domain.Enums.LeaveStatus.Approved && l.StartDate <= today && l.EndDate >= today)
                .Include(l => l.Employee)
                .ToListAsync();

            var leaveAvatars = onLeaveToday.Select(l => new EmployeeAvatarDto
            {
                Name = $"{l.Employee.FirstName} {l.Employee.LastName}",
                AvatarUrl = null,
                Initials = GetInitials($"{l.Employee.FirstName} {l.Employee.LastName}")
            }).ToList();

            // Attendance Rate Today
            var attendanceToday = await _unitOfWork.Attendances
                .CountAsync(a => a.Date == today && a.Status == Domain.Enums.AttendanceStatus.Present);
            
            double attendanceRate = activeEmployees > 0 ? (double)attendanceToday / activeEmployees * 100 : 0;

            // Pending Leaves
            var pendingLeavesCount = await _unitOfWork.Leaves
                .CountAsync(l => l.Status == Domain.Enums.LeaveStatus.Submitted);

            // Payroll status for current month
            var payrollProcessed = await _unitOfWork.Payrolls
                .AnyAsync(p => p.Month == today.Month && p.Year == today.Year);

            // Work Hours this month (average)
            var monthlyAttendance = await _unitOfWork.Attendances
                .GetAllQueryable()
                .Where(a => a.Date >= startOfMonth && a.WorkHours.HasValue)
                .ToListAsync();

            double avgHours = monthlyAttendance.Any() ? (double)monthlyAttendance.Average(a => (double)a.WorkHours!) : 0;

            // Leave Distribution Data
            var leaveTypes = await _unitOfWork.Leaves
                .GetAllQueryable()
                .Where(l => l.Status == Domain.Enums.LeaveStatus.Approved)
                .GroupBy(l => l.LeaveType.Name)
                .Select(g => new ChartDataDto { Label = g.Key, Value = g.Count() })
                .ToListAsync();

            // Attendance Trend (Last 7 days)
            var sevenDaysAgo = today.AddDays(-7);
            var trend = await _unitOfWork.Attendances
                .GetAllQueryable()
                .Where(a => a.Date >= sevenDaysAgo && a.Date <= today)
                .GroupBy(a => a.Date)
                .Select(g => new ChartDataDto { 
                    Label = g.Key.ToString("ddd"), 
                    Value = g.Count(a => a.Status == Domain.Enums.AttendanceStatus.Present) 
                })
                .OrderBy(t => t.Label)
                .ToListAsync();

            return new DashboardSummaryDto
            {
                TotalEmployees = totalEmployees,
                ActiveEmployees = activeEmployees,
                OnLeaveTodayCount = onLeaveToday.Count,
                OnLeaveTodayAvatars = leaveAvatars,
                AttendanceRateToday = Math.Round(attendanceRate, 1),
                PendingLeavesCount = pendingLeavesCount,
                PayrollProcessedThisMonth = payrollProcessed,
                AvgWorkingHoursMonth = Math.Round(avgHours, 1),
                LeaveDistribution = leaveTypes,
                AttendanceTrend = trend,
                RecentActivities = new List<RecentActivityDto>() // Placeholder for now
            };
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "??";
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
        }
    }
}
