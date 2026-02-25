namespace HRMS.Shared.Wrappers;

public class PagedResponse<T>
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public PaginationMeta? Pagination { get; set; }

    public static PagedResponse<T> CreateResponse(T data, int page, int pageSize, int totalCount, string message = "Request completed successfully.")
    {
        return new PagedResponse<T>
        {
            Success = true,
            StatusCode = 200,
            Message = message,
            Data = data,
            Pagination = new PaginationMeta
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            }
        };
    }
}

public class PaginationMeta
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
