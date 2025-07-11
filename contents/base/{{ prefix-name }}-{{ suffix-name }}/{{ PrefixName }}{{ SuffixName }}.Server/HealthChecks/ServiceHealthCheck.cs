using Microsoft.Extensions.Diagnostics.HealthChecks;
using {{ PrefixName }}{{ SuffixName }}.Core.Services;

namespace {{ PrefixName }}{{ SuffixName }}.Server.HealthChecks;

/// <summary>
/// Health check for core service functionality and dependencies
/// </summary>
public class ServiceHealthCheck : IHealthCheck
{
    private readonly IValidationService _validationService;
    private readonly ILogger<ServiceHealthCheck> _logger;

    public ServiceHealthCheck(
        IValidationService validationService,
        ILogger<ServiceHealthCheck> logger)
    {
        _validationService = validationService;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Performing service health check");

            var healthData = new Dictionary<string, object>
            {
                ["timestamp"] = DateTimeOffset.UtcNow
            };

            // Test validation service
            try
            {
                var testGuid = Guid.NewGuid();
                var validatedGuid = _validationService.ValidateAndParseId(testGuid.ToString(), "TestId");
                
                if (validatedGuid == testGuid)
                {
                    healthData["validationService"] = "healthy";
                    _logger.LogDebug("Validation service is healthy");
                }
                else
                {
                    healthData["validationService"] = "inconsistent";
                    _logger.LogWarning("Validation service returned inconsistent result");
                    return Task.FromResult(HealthCheckResult.Degraded(
                        "Validation service is not working correctly",
                        data: healthData));
                }
            }
            catch (Exception ex)
            {
                healthData["validationService"] = "error";
                healthData["validationError"] = ex.Message;
                _logger.LogError(ex, "Validation service health check failed");
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    "Validation service is not working",
                    ex,
                    data: healthData));
            }

            // Core services are healthy
            healthData["coreServices"] = "healthy";
            _logger.LogDebug("Core services are healthy");

            return Task.FromResult(HealthCheckResult.Healthy(
                "All core services are healthy and responsive",
                data: healthData));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Service health check was cancelled");
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Service health check timed out",
                data: new Dictionary<string, object>
                {
                    ["services"] = "timeout",
                    ["timestamp"] = DateTimeOffset.UtcNow
                }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service health check failed with unexpected exception");
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Service health check failed: {ex.Message}",
                ex,
                data: new Dictionary<string, object>
                {
                    ["services"] = "error",
                    ["exceptionType"] = ex.GetType().Name,
                    ["timestamp"] = DateTimeOffset.UtcNow
                }));
        }
    }
}