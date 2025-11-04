using System.Net;
using System.Text.Json;
using {{ PrefixName }}{{ SuffixName }}.Core.Exceptions;
using {{ PrefixName }}{{ SuffixName }}.Server.Services;

namespace {{ PrefixName }}{{ SuffixName }}.Server.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly MetricsService _metricsService;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, MetricsService metricsService)
    {
        _next = next;
        _logger = logger;
        _metricsService = metricsService;
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
        var (statusCode, message, errorType) = exception switch
        {
            EntityNotFoundException notFound => (HttpStatusCode.NotFound, notFound.Message, "NotFound"),
            ValidationException validation => (HttpStatusCode.BadRequest, validation.Message, "InvalidArgument"),
            BusinessRuleException businessRule => (HttpStatusCode.BadRequest, businessRule.Message, "BusinessRuleViolation"),
            InvalidOperationException invalid => (HttpStatusCode.BadRequest, invalid.Message, "InvalidOperation"),
            UnauthorizedAccessException unauthorized => (HttpStatusCode.Unauthorized, "Authentication required", "Unauthenticated"),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred", "Internal")
        };

        _metricsService.RecordError(context.Request.Method, errorType);
        
        _logger.LogError(exception, "Exception handled: {ErrorType} - {Message}", errorType, message);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = new
            {
                message,
                type = errorType,
                timestamp = DateTime.UtcNow,
                path = context.Request.Path.Value,
                correlationId = context.TraceIdentifier
            }
        };

        var jsonResponse = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(jsonResponse);
    }
}
