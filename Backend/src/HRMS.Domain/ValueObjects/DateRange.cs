namespace HRMS.Domain.ValueObjects;

public record DateRange
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }

    public DateRange(DateTime startDate, DateTime endDate)
    {
        if (endDate < startDate)
            throw new ArgumentException("End date must be greater than or equal to start date.");

        StartDate = startDate;
        EndDate = endDate;
    }

    public int TotalDays => (EndDate.Date - StartDate.Date).Days + 1;

    public bool Overlaps(DateRange other)
    {
        return StartDate <= other.EndDate && other.StartDate <= EndDate;
    }

    public bool Contains(DateTime date)
    {
        return date.Date >= StartDate.Date && date.Date <= EndDate.Date;
    }
}
