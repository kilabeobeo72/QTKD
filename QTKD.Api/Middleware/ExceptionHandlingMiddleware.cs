using System.Net;
using System.Text.Json;

namespace QTKD.Api.Middleware;

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
            _logger.LogError(ex, "Đã xảy ra lỗi khi xử lý request {Method} {Path}", context.Request.Method, context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, errorCode, message) = exception switch
        {
            KeyNotFoundException => (
                (int)HttpStatusCode.NotFound,
                "NOT_FOUND",
                exception.Message
            ),
            ArgumentException => (
                (int)HttpStatusCode.BadRequest,
                "BAD_REQUEST",
                exception.Message
            ),
            InvalidOperationException => (
                (int)HttpStatusCode.BadRequest,
                "BUSINESS_RULE_VIOLATION",
                exception.Message
            ),
            UnauthorizedAccessException => (
                (int)HttpStatusCode.Forbidden,
                "FORBIDDEN",
                exception.Message
            ),
            _ => (
                (int)HttpStatusCode.InternalServerError,
                "INTERNAL_SERVER_ERROR",
                "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau."
            )
        };

        context.Response.StatusCode = statusCode;

        var response = new
        {
            statusCode,
            errorCode,
            message,
            timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
