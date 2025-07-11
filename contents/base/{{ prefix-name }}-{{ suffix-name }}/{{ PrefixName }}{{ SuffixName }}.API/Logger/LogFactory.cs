using Microsoft.Extensions.Logging;

namespace {{ PrefixName }}{{ SuffixName }}.API.Logger;

/// <summary>
/// Factory for creating loggers with consistent configuration.
/// Provides a facade pattern similar to SLF4J for consistent logging across the application.
/// </summary>
public static class LogFactory
{
    /// <summary>
    /// Creates a logger for the specified type.
    /// Preferred method for type-safe logger creation.
    /// </summary>
    /// <typeparam name="T">The type to create the logger for</typeparam>
    /// <returns>A configured Serilog logger</returns>
    public static Serilog.ILogger GetLogger<T>() => GetLogger(typeof(T));
    
    /// <summary>
    /// Creates a logger for the specified type.
    /// </summary>
    /// <param name="type">The type to create the logger for</param>
    /// <returns>A configured Serilog logger</returns>
    public static Serilog.ILogger GetLogger(Type type) => GetLogger(type.FullName ?? type.Name);
    
    /// <summary>
    /// Creates a logger with the specified name.
    /// Use this method when you need a custom logger name.
    /// </summary>
    /// <param name="name">The logger name</param>
    /// <returns>A configured Serilog logger</returns>
    public static Serilog.ILogger GetLogger(string name)
    {
        return Serilog.Log.ForContext("SourceContext", name);
    }
    
    /// <summary>
    /// Creates a logger for the specified type (legacy method for backward compatibility).
    /// Consider using GetLogger<T>() instead.
    /// </summary>
    /// <param name="name">The logger name</param>
    /// <returns>A configured Serilog logger</returns>
    [Obsolete("Use GetLogger<T>() or GetLogger(Type) instead for better type safety")]
    public static Serilog.ILogger CreateLogger(string name) => GetLogger(name);
}