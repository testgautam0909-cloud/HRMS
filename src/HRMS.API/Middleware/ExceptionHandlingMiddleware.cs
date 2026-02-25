using System.Net;
using System.Text.Json;
using HRMS.Shared.Exceptions;
using HRMS.Shared.Wrappers;

namespace HRMS.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            NotFoundException nf => (HttpStatusCode.NotFound, ApiResponse<object>.Fail(nf.Message)),
            Shared.Exceptions.ValidationException ve => (HttpStatusCode.BadRequest, ApiResponse<object>.Fail("Validation failed.", ve.Errors)),
            ConflictException ce => (HttpStatusCode.Conflict, ApiResponse<object>.Fail(ce.Message)),
            UnauthorizedException ue => (HttpStatusCode.Unauthorized, ApiResponse<object>.Fail(ue.Message)),
            ForbiddenException fe => (HttpStatusCode.Forbidden, ApiResponse<object>.Fail(fe.Message)),
            _ => (HttpStatusCode.InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred."))
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        else
            _logger.LogWarning("Handled exception: {Type} - {Message}", exception.GetType().Name, exception.Message);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}
