using Microsoft.Extensions.Diagnostics.HealthChecks;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace {{ PrefixName }}{{ SuffixName }}.Server.HealthChecks;

/// <summary>
/// Health check for database connectivity and availability
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(AppDbContext dbContext, ILogger<DatabaseHealthCheck> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Performing database health check");

            // Test database connectivity with a simple query
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            if (!canConnect)
            {
                _logger.LogWarning("Database health check failed: Cannot connect to database");
                return HealthCheckResult.Unhealthy(
                    "Cannot connect to database",
                    data: new Dictionary<string, object>
                    {
                        ["database"] = "unreachable",
                        ["timestamp"] = DateTimeOffset.UtcNow
                    });
            }

            // Test that we can execute a basic query
            var recordCount = await _dbContext.{{ PrefixName }}s.CountAsync(cancellationToken);
            
            var responseTime = DateTimeOffset.UtcNow;
            _logger.LogDebug("Database health check passed. Record count: {RecordCount}", recordCount);

            return HealthCheckResult.Healthy(
                "Database is accessible and responsive",
                data: new Dictionary<string, object>
                {
                    ["database"] = "healthy",
                    ["recordCount"] = recordCount,
                    ["connectionString"] = _dbContext.Database.GetConnectionString()?.Substring(0, 20) + "...",
                    ["timestamp"] = responseTime
                });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Database health check was cancelled");
            return HealthCheckResult.Unhealthy(
                "Database health check timed out",
                data: new Dictionary<string, object>
                {
                    ["database"] = "timeout",
                    ["timestamp"] = DateTimeOffset.UtcNow
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed with exception");
            return HealthCheckResult.Unhealthy(
                $"Database health check failed: {ex.Message}",
                ex,
                data: new Dictionary<string, object>
                {
                    ["database"] = "error",
                    ["exceptionType"] = ex.GetType().Name,
                    ["timestamp"] = DateTimeOffset.UtcNow
                });
        }
    }
}