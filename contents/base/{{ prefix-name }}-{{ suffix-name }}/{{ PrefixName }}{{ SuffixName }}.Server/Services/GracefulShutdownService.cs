namespace {{ PrefixName }}{{ SuffixName }}.Server.Services;

public class GracefulShutdownService : IHostedService
{
    private readonly ILogger<GracefulShutdownService> _logger;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IServiceProvider _serviceProvider;

    public GracefulShutdownService(
        ILogger<GracefulShutdownService> logger,
        IHostApplicationLifetime lifetime,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _lifetime = lifetime;
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _lifetime.ApplicationStopping.Register(OnStopping);
        _lifetime.ApplicationStopped.Register(OnStopped);
        
        _logger.LogInformation("Graceful shutdown service started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Graceful shutdown service stopping");
        return Task.CompletedTask;
    }

    private void OnStopping()
    {
        _logger.LogInformation("Application is shutting down gracefully...");
        
        // Give ongoing requests time to complete
        var shutdownTimeout = TimeSpan.FromSeconds(30);
        var cancellationTokenSource = new CancellationTokenSource(shutdownTimeout);
        
        try
        {
            // Perform cleanup tasks
            PerformCleanupAsync(cancellationTokenSource.Token).GetAwaiter().GetResult();
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Graceful shutdown timeout exceeded. Forcing shutdown.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during graceful shutdown");
        }
    }

    private void OnStopped()
    {
        _logger.LogInformation("Application has stopped");
    }

    private async Task PerformCleanupAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting cleanup operations...");{% if persistence != 'None' %}

        // Close database connections gracefully
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<{{ PrefixName }}{{ SuffixName }}.Persistence.Context.AppDbContext>();
        if (dbContext != null)
        {
            await dbContext.DisposeAsync();
            _logger.LogDebug("Database context disposed");
        }{% endif %}

        // Allow some time for in-flight requests to complete
        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        
        _logger.LogInformation("Cleanup operations completed");
    }
}
