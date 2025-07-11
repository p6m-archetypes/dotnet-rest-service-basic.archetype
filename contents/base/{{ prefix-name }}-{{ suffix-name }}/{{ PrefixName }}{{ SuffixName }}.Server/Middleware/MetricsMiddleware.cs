using System.Diagnostics;
using {{ PrefixName }}{{ SuffixName }}.Server.Services;

namespace {{ PrefixName }}{{ SuffixName }}.Server.Middleware;

public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly MetricsService _metricsService;
    private readonly ILogger<MetricsMiddleware> _logger;

    public MetricsMiddleware(RequestDelegate next, MetricsService metricsService, ILogger<MetricsMiddleware> logger)
    {
        _next = next;
        _metricsService = metricsService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "unknown";
        var method = context.Request.Method;
        
        // Skip metrics for health checks and metrics endpoint
        if (path.StartsWith("/health") || path == "/metrics")
        {
            await _next(context);
            return;
        }

        _metricsService.RecordConnectionOpened();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
            
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            
            _metricsService.RecordRequest($"{method} {path}", statusCode.ToString(), stopwatch.Elapsed.TotalSeconds);
            
            _logger.LogDebug("Request {Method} {Path} completed with status {StatusCode} in {Duration}ms",
                method, path, statusCode, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _metricsService.RecordRequest($"{method} {path}", "500", stopwatch.Elapsed.TotalSeconds);
            _metricsService.RecordError(method, "UnhandledException");
            
            _logger.LogError(ex, "Unhandled exception for {Method} {Path} after {Duration}ms",
                method, path, stopwatch.ElapsedMilliseconds);
            
            throw;
        }
        finally
        {
            _metricsService.RecordConnectionClosed();
        }
    }
}