namespace HRMS.Application.DTOs.Common;

public class PaginationParams
{
    private int _page = 1;
    private int _pageSize = 20;

    public int Page
    {
        get => _page;
        set => _page = value <= 0 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value <= 0 ? 20 : (value > 100 ? 100 : value);
    }
}

public class DateRangeFilterDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
