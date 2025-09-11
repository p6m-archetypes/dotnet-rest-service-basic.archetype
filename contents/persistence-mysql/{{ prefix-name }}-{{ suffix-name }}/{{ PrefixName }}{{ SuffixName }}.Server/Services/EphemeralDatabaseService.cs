using Microsoft.Extensions.Options;
using Testcontainers.MySql;
using MySqlConnector;

namespace {{ PrefixName }}{{ SuffixName }}.Server.Services;

/// <summary>
/// Service for managing ephemeral MySQL database using Testcontainers
/// </summary>
public class EphemeralDatabaseService : IDisposable
{
    private readonly EphemeralDatabaseOptions _options;
    private readonly ILogger<EphemeralDatabaseService> _logger;
    private MySqlContainer? _container;
    private bool _disposed = false;
    private bool _stoppedExternally = false;
    
    public EphemeralDatabaseService(
        IOptions<EphemeralDatabaseOptions> options,
        ILogger<EphemeralDatabaseService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }
    
    /// <summary>
    /// Starts the MySQL container and returns the connection string
    /// </summary>
    public async Task<string> StartDatabaseAsync(CancellationToken cancellationToken = default)
    {
        if (_container != null)
        {
            _logger.LogDebug("Database container is already running, returning existing connection string");
            return _container.GetConnectionString();
        }
        
        _logger.LogInformation("Starting ephemeral MySQL container with image: {Image}", _options.Image);
        
        _container = new MySqlBuilder()
            .WithImage(_options.Image)
            .WithDatabase(_options.DatabaseName)
            .WithUsername(_options.Username)
            .WithPassword(_options.Password)
            .WithCleanUp(!_options.Reuse)
            .Build();
        
        try
        {
            await _container.StartAsync(cancellationToken);
            
            // Wait for the database to be ready to accept connections
            await WaitForDatabaseReadyAsync(cancellationToken);
            
            var connectionString = _container.GetConnectionString();
            var host = _container.Hostname;
            var port = _container.GetMappedPublicPort(MySqlBuilder.MySqlPort);
            var database = _options.DatabaseName;
            var username = _options.Username;
            var password = _options.Password;
            
            _logger.LogInformation(
                "MySQL container started successfully. Host: {Host}, Port: {Port}, Database: {Database}",
                host, port, database);
            
            return connectionString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start MySQL container");
            throw;
        }
    }
    
    /// <summary>
    /// Stops the MySQL container
    /// </summary>
    public async Task StopDatabaseAsync(CancellationToken cancellationToken = default)
    {
        if (_container == null)
        {
            if (!_stoppedExternally)
            {
                _logger.LogDebug("No database container to stop");
            }
            return;
        }
        
        try
        {
            _logger.LogInformation("Stopping MySQL container");
            await _container.StopAsync(cancellationToken);
            _logger.LogInformation("MySQL container stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop MySQL container");
        }
        finally
        {
            await _container.DisposeAsync();
            _container = null;
            _stoppedExternally = true;
        }
    }
    
    /// <summary>
    /// Gets the connection string for the running container
    /// </summary>
    public string? GetConnectionString()
    {
        return _container?.GetConnectionString();
    }
    
    /// <summary>
    /// Checks if the database container is running
    /// </summary>
    public bool IsRunning => _container?.State == DotNet.Testcontainers.Containers.TestcontainersStates.Running;
    
    /// <summary>
    /// Waits for the database to be ready to accept connections
    /// </summary>
    private async Task WaitForDatabaseReadyAsync(CancellationToken cancellationToken = default)
    {
        if (_container == null)
        {
            throw new InvalidOperationException("Container is not running");
        }
        
        var connectionString = _container.GetConnectionString();
        var maxAttempts = 30;
        var delayBetweenAttempts = TimeSpan.FromSeconds(1);
        
        _logger.LogDebug("Waiting for MySQL container to be ready to accept connections...");
        
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync(cancellationToken);
                await connection.CloseAsync();
                
                _logger.LogDebug("MySQL container is ready after {Attempts} attempts", attempt);
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                _logger.LogDebug("MySQL not ready yet (attempt {Attempt}/{MaxAttempts}): {Error}", 
                    attempt, maxAttempts, ex.Message);
                    
                await Task.Delay(delayBetweenAttempts, cancellationToken);
            }
        }
        
        throw new TimeoutException($"MySQL container did not become ready after {maxAttempts} attempts");
    }
    
    /// <summary>
    /// Displays the database connection information if available.
    /// </summary>
    public void DisplayConnectionInfo()
    {
        if (_container != null && _container.State == DotNet.Testcontainers.Containers.TestcontainersStates.Running)
        {
            var host = _container.Hostname;
            var port = _container.GetMappedPublicPort(MySqlBuilder.MySqlPort);
            var database = _options.DatabaseName;
            var username = _options.Username;
            var password = _options.Password;
            var connectionString = GetConnectionString();
            
            if (!string.IsNullOrEmpty(connectionString))
            {
                LogDatabaseConnectionInfo(host, port, database, username, password, connectionString);
            }
        }
    }

    /// <summary>
    /// Logs database connection information for development convenience
    /// </summary>
    private void LogDatabaseConnectionInfo(string host, int port, string database, string username, string password, string connectionString)
    {
        var separator = new string('=', 80);
        
        Console.WriteLine();
        Console.WriteLine(separator);
        Console.WriteLine("🐬 EPHEMERAL MYSQL DATABASE CONNECTION INFO");
        Console.WriteLine(separator);
        Console.WriteLine();
        Console.WriteLine("📋 Connection Details:");
        Console.WriteLine($"   Host:     {host}");
        Console.WriteLine($"   Port:     {port}");
        Console.WriteLine($"   Database: {database}");
        Console.WriteLine($"   Username: {username}");
        Console.WriteLine($"   Password: {password}");
        Console.WriteLine();
        Console.WriteLine("🔗 Connection Strings:");
        Console.WriteLine();
        Console.WriteLine("   .NET Connection String:");
        Console.WriteLine($"   {connectionString}");
        Console.WriteLine();
        Console.WriteLine("   JDBC URL (for DataGrip/IntelliJ):");
        Console.WriteLine($"   jdbc:mysql://{host}:{port}/{database}");
        Console.WriteLine();
        Console.WriteLine("💻 Connect via mysql:");
        Console.WriteLine($"   mysql -h {host} -P {port} -u {username} -p{password} {database}");
        Console.WriteLine($"   Password: {password}");
        Console.WriteLine();
        Console.WriteLine("🔧 DataGrip/Database Tool Settings:");
        Console.WriteLine($"   Type:     MySQL");
        Console.WriteLine($"   Host:     {host}");
        Console.WriteLine($"   Port:     {port}");
        Console.WriteLine($"   Database: {database}");
        Console.WriteLine($"   User:     {username}");
        Console.WriteLine($"   Password: {password}");
        Console.WriteLine();
        Console.WriteLine("ℹ️  Note: This is an ephemeral database that will be destroyed when the application stops.");
        Console.WriteLine(separator);
        Console.WriteLine();
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            if (_container != null)
            {
                // Note: We can't await in Dispose, so we use sync disposal
                try
                {
                    _container.StopAsync(CancellationToken.None).Wait(TimeSpan.FromSeconds(10));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to stop container during disposal");
                }
                finally
                {
                    _container.DisposeAsync().AsTask().Wait(TimeSpan.FromSeconds(5));
                    _container = null;
                    _stoppedExternally = true;
                }
            }
            _disposed = true;
        }
    }
}
