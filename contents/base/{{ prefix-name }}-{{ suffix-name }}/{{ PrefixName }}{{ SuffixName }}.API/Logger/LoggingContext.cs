using Serilog.Context;
using System.Collections.Concurrent;

namespace {{ PrefixName }}{{ SuffixName }}.API.Logger;

/// <summary>
/// Provides context management for logging similar to SLF4J's MDC (Mapped Diagnostic Context).
/// Allows adding contextual information that will be included in all log messages within the scope.
/// </summary>
public static class LoggingContext
{
    private static readonly AsyncLocal<ConcurrentDictionary<string, object?>> _context = new();
    
    /// <summary>
    /// Gets the current logging context dictionary.
    /// </summary>
    private static ConcurrentDictionary<string, object?> Context => 
        _context.Value ??= new ConcurrentDictionary<string, object?>();
    
    /// <summary>
    /// Adds or updates a property in the logging context.
    /// This property will be included in all subsequent log messages in this execution context.
    /// </summary>
    /// <param name="key">The property key</param>
    /// <param name="value">The property value</param>
    /// <returns>A disposable that removes the property when disposed</returns>
    public static IDisposable PushProperty(string key, object? value)
    {
        Context[key] = value;
        return LogContext.PushProperty(key, value);
    }
    
    /// <summary>
    /// Adds multiple properties to the logging context.
    /// </summary>
    /// <param name="properties">Dictionary of properties to add</param>
    /// <returns>A disposable that removes all properties when disposed</returns>
    public static IDisposable PushProperties(IDictionary<string, object?> properties)
    {
        var disposables = new List<IDisposable>();
        
        foreach (var (key, value) in properties)
        {
            Context[key] = value;
            disposables.Add(LogContext.PushProperty(key, value));
        }
        
        return new CompositeDisposable(disposables);
    }
    
    /// <summary>
    /// Gets a property value from the current context.
    /// </summary>
    /// <param name="key">The property key</param>
    /// <returns>The property value or null if not found</returns>
    public static object? GetProperty(string key)
    {
        return Context.TryGetValue(key, out var value) ? value : null;
    }
    
    /// <summary>
    /// Gets a typed property value from the current context.
    /// </summary>
    /// <typeparam name="T">The expected type of the property</typeparam>
    /// <param name="key">The property key</param>
    /// <returns>The typed property value or default(T) if not found or not convertible</returns>
    public static T? GetProperty<T>(string key)
    {
        var value = GetProperty(key);
        return value is T typedValue ? typedValue : default(T);
    }
    
    /// <summary>
    /// Removes a property from the current context.
    /// </summary>
    /// <param name="key">The property key to remove</param>
    /// <returns>True if the property was removed, false if it didn't exist</returns>
    public static bool RemoveProperty(string key)
    {
        return Context.TryRemove(key, out _);
    }
    
    /// <summary>
    /// Clears all properties from the current context.
    /// </summary>
    public static void Clear()
    {
        Context.Clear();
    }
    
    /// <summary>
    /// Gets all properties in the current context.
    /// </summary>
    /// <returns>A read-only dictionary of all context properties</returns>
    public static IReadOnlyDictionary<string, object?> GetAllProperties()
    {
        return Context.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
    
    /// <summary>
    /// Creates a scope with standard request context properties.
    /// </summary>
    /// <param name="requestId">The request ID</param>
    /// <param name="userId">The user ID (optional)</param>
    /// <param name="operation">The operation being performed (optional)</param>
    /// <param name="correlationId">The correlation ID (optional)</param>
    /// <returns>A disposable scope that removes the properties when disposed</returns>
    public static IDisposable BeginRequestScope(string requestId, string? userId = null, 
        string? operation = null, string? correlationId = null)
    {
        var properties = new Dictionary<string, object?>
        {
            ["RequestId"] = requestId,
            ["UserId"] = userId,
            ["Operation"] = operation,
            ["CorrelationId"] = correlationId ?? requestId
        };
        
        return PushProperties(properties.Where(p => p.Value != null).ToDictionary(p => p.Key, p => p.Value));
    }
    
    /// <summary>
    /// Creates a scope with business operation context.
    /// </summary>
    /// <param name="operation">The business operation</param>
    /// <param name="entityType">The entity type</param>
    /// <param name="entityId">The entity ID (optional)</param>
    /// <param name="userId">The user ID (optional)</param>
    /// <returns>A disposable scope that removes the properties when disposed</returns>
    public static IDisposable BeginBusinessOperationScope(string operation, string entityType, 
        object? entityId = null, string? userId = null)
    {
        var properties = new Dictionary<string, object?>
        {
            ["BusinessOperation"] = operation,
            ["EntityType"] = entityType,
            ["EntityId"] = entityId,
            ["UserId"] = userId
        };
        
        return PushProperties(properties.Where(p => p.Value != null).ToDictionary(p => p.Key, p => p.Value));
    }
}

/// <summary>
/// Helper class to dispose multiple disposables together.
/// </summary>
internal class CompositeDisposable : IDisposable
{
    private readonly List<IDisposable> _disposables;
    private bool _disposed = false;
    
    public CompositeDisposable(List<IDisposable> disposables)
    {
        _disposables = disposables;
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            foreach (var disposable in _disposables)
            {
                try
                {
                    disposable.Dispose();
                }
                catch
                {
                    // Ignore disposal errors
                }
            }
            _disposed = true;
        }
    }
}