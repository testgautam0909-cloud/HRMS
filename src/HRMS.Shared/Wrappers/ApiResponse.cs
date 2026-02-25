namespace HRMS.Shared.Wrappers;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public IDictionary<string, string[]>? Errors { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Request completed successfully.", int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            StatusCode = statusCode,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> FailResponse(string message, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            StatusCode = statusCode,
            Message = message
        };
    }

    public static ApiResponse<T> FailResponse(string message, IDictionary<string, string[]> errors, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            Errors = errors
        };
    }

    public static ApiResponse<T> Ok(T data, string message = "Request completed successfully.", int statusCode = 200)
        => SuccessResponse(data, message, statusCode);

    public static ApiResponse<T> Fail(string message, int statusCode = 400)
        => FailResponse(message, statusCode);

    public static ApiResponse<T> Fail(string message, IDictionary<string, string[]> errors, int statusCode = 400)
        => FailResponse(message, errors, statusCode);
}
