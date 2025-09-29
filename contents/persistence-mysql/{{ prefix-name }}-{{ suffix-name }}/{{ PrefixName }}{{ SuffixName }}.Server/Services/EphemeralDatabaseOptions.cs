namespace {{ PrefixName }}{{ SuffixName }}.Server.Services;

/// <summary>
/// Configuration options for ephemeral database using Testcontainers
/// </summary>
public class EphemeralDatabaseOptions
{
    public const string SectionName = "Ephemeral:Database";
    
    /// <summary>
    /// Docker image for MySQL container
    /// </summary>
    public string Image { get; set; } = "mysql:8.0";
    
    /// <summary>
    /// Database name to create
    /// </summary>
    public string DatabaseName { get; set; } = "{{ prefix_name }}_{{ suffix_name }}";
    
    /// <summary>
    /// MySQL username
    /// </summary>
    public string Username { get; set; } = "root";
    
    /// <summary>
    /// MySQL password
    /// </summary>
    public string Password { get; set; } = "testpassword";
    
    /// <summary>
    /// Whether to reuse existing container instances
    /// </summary>
    public bool Reuse { get; set; } = false;
}
