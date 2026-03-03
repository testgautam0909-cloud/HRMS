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
            NotFoundException nf => (HttpStatusCode.NotFound, ApiResponse<object>.Fail(nf.Message, (int)HttpStatusCode.NotFound)),
            Shared.Exceptions.ValidationException ve => (HttpStatusCode.BadRequest, ApiResponse<object>.Fail("Validation failed.", ve.Errors, (int)HttpStatusCode.BadRequest)),
            ConflictException ce => (HttpStatusCode.Conflict, ApiResponse<object>.Fail(ce.Message, (int)HttpStatusCode.Conflict)),
            UnauthorizedException ue => (HttpStatusCode.Unauthorized, ApiResponse<object>.Fail(ue.Message, (int)HttpStatusCode.Unauthorized)),
            ForbiddenException fe => (HttpStatusCode.Forbidden, ApiResponse<object>.Fail(fe.Message, (int)HttpStatusCode.Forbidden)),
            _ => (HttpStatusCode.InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred.", (int)HttpStatusCode.InternalServerError))
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
