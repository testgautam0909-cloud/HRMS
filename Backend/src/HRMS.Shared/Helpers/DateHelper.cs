namespace HRMS.Shared.Helpers;

public static class DateHelper
{
    public static int GetWorkingDaysInMonth(int year, int month)
    {
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var workingDays = 0;
        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(year, month, day);
            if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                workingDays++;
        }
        return workingDays;
    }

    public static decimal CalculateWorkHours(DateTime checkIn, DateTime checkOut)
    {
        var duration = checkOut - checkIn;
        return Math.Round((decimal)duration.TotalHours, 2);
    }

    public static int CalculateLeaveDays(DateTime startDate, DateTime endDate)
    {
        return (endDate.Date - startDate.Date).Days + 1;
    }

    public static decimal CalculateProRatedAllocation(int totalAllocation, int joiningMonth)
    {
        var remainingMonths = 12 - joiningMonth + 1;
        return Math.Round((decimal)totalAllocation * remainingMonths / 12, 1);
    }
}
