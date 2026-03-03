namespace HRMS.Shared.Helpers;

public static class PaginationHelper
{
    public static int CalculateTotalPages(int totalCount, int pageSize)
    {
        return (int)Math.Ceiling((double)totalCount / pageSize);
    }

    public static int NormalizePageSize(int pageSize, int maxPageSize = 100, int defaultPageSize = 20)
    {
        if (pageSize <= 0) return defaultPageSize;
        return Math.Min(pageSize, maxPageSize);
    }

    public static int NormalizePageNumber(int page)
    {
        return page <= 0 ? 1 : page;
    }
}
