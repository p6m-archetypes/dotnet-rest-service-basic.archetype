using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace {{ PrefixName }}{{ SuffixName }}.Server.Services;

/// <summary>
/// Hosted service that starts the ephemeral database during application startup
/// and displays connection information for development convenience.
/// </summary>
public class EphemeralDatabaseHostedService : IHostedService
{
    private readonly EphemeralDatabaseService _ephemeralDatabaseService;
    private readonly ILogger<EphemeralDatabaseHostedService> _logger;

    public EphemeralDatabaseHostedService(
        EphemeralDatabaseService ephemeralDatabaseService,
        ILogger<EphemeralDatabaseHostedService> logger)
    {
        _ephemeralDatabaseService = ephemeralDatabaseService;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting ephemeral database...");
            await _ephemeralDatabaseService.StartDatabaseAsync(cancellationToken);
            _logger.LogInformation("Ephemeral database started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start ephemeral database");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Stopping ephemeral database...");
            await _ephemeralDatabaseService.StopDatabaseAsync(cancellationToken);
            _logger.LogInformation("Ephemeral database stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop ephemeral database");
            // Don't throw during shutdown
        }
    }
}