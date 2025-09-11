namespace {{ PrefixName }}{{ SuffixName }}.Server.Services;

/// <summary>
/// Configuration options for ephemeral database using Testcontainers
/// </summary>
public class EphemeralDatabaseOptions
{
    public const string SectionName = "Ephemeral:Database";
    
    /// <summary>
    /// Docker image for SQL Server container
    /// </summary>
    public string Image { get; set; } = "mcr.microsoft.com/mssql/server:2022-latest";
    
    /// <summary>
    /// Database name to create
    /// </summary>
    public string DatabaseName { get; set; } = "example_service_db";
    
    /// <summary>
    /// SQL Server username (default is 'sa' for SQL Server)
    /// </summary>
    public string Username { get; set; } = "sa";
    
    /// <summary>
    /// SQL Server password (must meet complexity requirements)
    /// </summary>
    public string Password { get; set; } = "YourStrong@Passw0rd";
    
    /// <summary>
    /// Whether to reuse existing container instances
    /// </summary>
    public bool Reuse { get; set; } = false;
}
