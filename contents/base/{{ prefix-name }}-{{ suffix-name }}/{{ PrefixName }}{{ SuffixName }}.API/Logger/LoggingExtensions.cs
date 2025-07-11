using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace {{ PrefixName }}{{ SuffixName }}.API.Logger;

/// <summary>
/// Extension methods for consistent logging patterns across the application.
/// Provides structured logging helpers for common scenarios.
/// </summary>
public static class LoggingExtensions
{
    #region Business Operation Logging
    
    /// <summary>
    /// Logs a business operation with structured context.
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="operation">The business operation being performed</param>
    /// <param name="entityId">The ID of the entity being operated on</param>
    /// <param name="entityType">The type of entity</param>
    /// <param name="userId">Optional user ID performing the operation</param>
    public static void LogBusinessOperation(this ILogger logger, string operation, object entityId, 
        string entityType, string? userId = null)
    {
        if (userId != null)
        {
            logger.LogInformation("Business operation: {Operation} on {EntityType} with ID: {EntityId} by user: {UserId}", 
                operation, entityType, entityId, userId);
        }
        else
        {
            logger.LogInformation("Business operation: {Operation} on {EntityType} with ID: {EntityId}", 
                operation, entityType, entityId);
        }
    }
    
    /// <summary>
    /// Logs a successful business operation.
    /// </summary>
    public static void LogBusinessSuccess(this ILogger logger, string operation, object entityId, 
        string entityType, TimeSpan? duration = null)
    {
        if (duration.HasValue)
        {
            logger.LogInformation("Business operation completed: {Operation} on {EntityType} with ID: {EntityId} in {Duration}ms", 
                operation, entityType, entityId, duration.Value.TotalMilliseconds);
        }
        else
        {
            logger.LogInformation("Business operation completed: {Operation} on {EntityType} with ID: {EntityId}", 
                operation, entityType, entityId);
        }
    }
    
    /// <summary>
    /// Logs a failed business operation.
    /// </summary>
    public static void LogBusinessFailure(this ILogger logger, string operation, object entityId, 
        string entityType, string reason, Exception? exception = null)
    {
        if (exception != null)
        {
            logger.LogError(exception, "Business operation failed: {Operation} on {EntityType} with ID: {EntityId} - {Reason}", 
                operation, entityType, entityId, reason);
        }
        else
        {
            logger.LogWarning("Business operation failed: {Operation} on {EntityType} with ID: {EntityId} - {Reason}", 
                operation, entityType, entityId, reason);
        }
    }
    
    #endregion
    
    #region Security Logging
    
    /// <summary>
    /// Logs a security event with structured context.
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="eventType">The type of security event</param>
    /// <param name="userId">The user ID associated with the event</param>
    /// <param name="details">Additional details about the event</param>
    /// <param name="remoteIp">Optional remote IP address</param>
    public static void LogSecurityEvent(this ILogger logger, string eventType, string? userId, 
        string details, string? remoteIp = null)
    {
        if (remoteIp != null)
        {
            logger.LogWarning("Security event: {EventType} for user: {UserId} from IP: {RemoteIp} - {Details}", 
                eventType, userId ?? "anonymous", remoteIp, details);
        }
        else
        {
            logger.LogWarning("Security event: {EventType} for user: {UserId} - {Details}", 
                eventType, userId ?? "anonymous", details);
        }
    }
    
    /// <summary>
    /// Logs an authentication event.
    /// </summary>
    public static void LogAuthentication(this ILogger logger, bool success, string? userId, 
        string method, string? remoteIp = null)
    {
        var eventType = success ? "AUTHENTICATION_SUCCESS" : "AUTHENTICATION_FAILURE";
        var logLevel = success ? LogLevel.Information : LogLevel.Warning;
        
        var message = remoteIp != null 
            ? "Authentication {EventType}: user {UserId} via {Method} from {RemoteIp}"
            : "Authentication {EventType}: user {UserId} via {Method}";
            
        logger.Log(logLevel, message, eventType, userId ?? "anonymous", method, remoteIp);
    }
    
    /// <summary>
    /// Logs an authorization event.
    /// </summary>
    public static void LogAuthorization(this ILogger logger, bool success, string? userId, 
        string operation, string resource)
    {
        var eventType = success ? "AUTHORIZATION_SUCCESS" : "AUTHORIZATION_FAILURE";
        var logLevel = success ? LogLevel.Debug : LogLevel.Warning;
        
        logger.Log(logLevel, "Authorization {EventType}: user {UserId} for operation {Operation} on {Resource}",
            eventType, userId ?? "anonymous", operation, resource);
    }
    
    #endregion
    
    #region Performance Logging
    
    /// <summary>
    /// Logs a performance metric with structured context.
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="operation">The operation being measured</param>
    /// <param name="duration">The duration of the operation</param>
    /// <param name="success">Whether the operation was successful</param>
    /// <param name="additionalContext">Optional additional context</param>
    public static void LogPerformanceMetric(this ILogger logger, string operation, TimeSpan duration, 
        bool success, object? additionalContext = null)
    {
        var status = success ? "SUCCESS" : "FAILURE";
        
        if (additionalContext != null)
        {
            logger.LogInformation("Performance: {Operation} completed in {Duration}ms with status {Status} - {@Context}", 
                operation, duration.TotalMilliseconds, status, additionalContext);
        }
        else
        {
            logger.LogInformation("Performance: {Operation} completed in {Duration}ms with status {Status}", 
                operation, duration.TotalMilliseconds, status);
        }
    }
    
    /// <summary>
    /// Logs a slow operation warning.
    /// </summary>
    public static void LogSlowOperation(this ILogger logger, string operation, TimeSpan duration, 
        TimeSpan threshold, object? context = null)
    {
        if (context != null)
        {
            logger.LogWarning("Slow operation detected: {Operation} took {Duration}ms (threshold: {Threshold}ms) - {@Context}", 
                operation, duration.TotalMilliseconds, threshold.TotalMilliseconds, context);
        }
        else
        {
            logger.LogWarning("Slow operation detected: {Operation} took {Duration}ms (threshold: {Threshold}ms)", 
                operation, duration.TotalMilliseconds, threshold.TotalMilliseconds);
        }
    }
    
    #endregion
    
    #region gRPC Logging
    
    /// <summary>
    /// Logs a gRPC request with structured context.
    /// </summary>
    public static void LogGrpcRequest(this ILogger logger, string method, object? request = null, 
        string? userId = null, string? correlationId = null)
    {
        if (request != null)
        {
            logger.LogDebug("gRPC request: {Method} by user {UserId} with correlation {CorrelationId} - {@Request}", 
                method, userId ?? "anonymous", correlationId, request);
        }
        else
        {
            logger.LogDebug("gRPC request: {Method} by user {UserId} with correlation {CorrelationId}", 
                method, userId ?? "anonymous", correlationId);
        }
    }
    
    /// <summary>
    /// Logs a gRPC response with structured context.
    /// </summary>
    public static void LogGrpcResponse(this ILogger logger, string method, bool success, 
        TimeSpan duration, string? statusCode = null, object? response = null)
    {
        var logLevel = success ? LogLevel.Information : LogLevel.Warning;
        
        if (response != null && success)
        {
            logger.Log(logLevel, "gRPC response: {Method} completed in {Duration}ms with status {StatusCode} - {@Response}", 
                method, duration.TotalMilliseconds, statusCode ?? "OK", response);
        }
        else
        {
            logger.Log(logLevel, "gRPC response: {Method} completed in {Duration}ms with status {StatusCode}", 
                method, duration.TotalMilliseconds, statusCode ?? (success ? "OK" : "ERROR"));
        }
    }
    
    #endregion
    
    #region Database Logging
    
    /// <summary>
    /// Logs a database operation with structured context.
    /// </summary>
    public static void LogDatabaseOperation(this ILogger logger, string operation, string entityType, 
        object? entityId = null, TimeSpan? duration = null)
    {
        if (entityId != null && duration != null)
        {
            logger.LogDebug("Database operation: {Operation} on {EntityType} with ID {EntityId} completed in {Duration}ms", 
                operation, entityType, entityId, duration.Value.TotalMilliseconds);
        }
        else if (entityId != null)
        {
            logger.LogDebug("Database operation: {Operation} on {EntityType} with ID {EntityId}", 
                operation, entityType, entityId);
        }
        else
        {
            logger.LogDebug("Database operation: {Operation} on {EntityType}", operation, entityType);
        }
    }
    
    /// <summary>
    /// Logs a database error with structured context.
    /// </summary>
    public static void LogDatabaseError(this ILogger logger, Exception exception, string operation, 
        string entityType, object? entityId = null)
    {
        if (entityId != null)
        {
            logger.LogError(exception, "Database error during {Operation} on {EntityType} with ID {EntityId}: {Message}", 
                operation, entityType, entityId, exception.Message);
        }
        else
        {
            logger.LogError(exception, "Database error during {Operation} on {EntityType}: {Message}", 
                operation, entityType, exception.Message);
        }
    }
    
    #endregion
    
    #region Validation Logging
    
    /// <summary>
    /// Logs validation failures with structured context.
    /// </summary>
    public static void LogValidationFailure(this ILogger logger, string operation, object validationErrors, 
        object? input = null)
    {
        if (input != null)
        {
            logger.LogWarning("Validation failed for {Operation} - Errors: {@ValidationErrors}, Input: {@Input}", 
                operation, validationErrors, input);
        }
        else
        {
            logger.LogWarning("Validation failed for {Operation} - Errors: {@ValidationErrors}", 
                operation, validationErrors);
        }
    }
    
    #endregion
    
    #region Health Check Logging
    
    /// <summary>
    /// Logs health check results with structured context.
    /// </summary>
    public static void LogHealthCheck(this ILogger logger, string checkName, bool healthy, 
        TimeSpan duration, string? description = null)
    {
        var status = healthy ? "HEALTHY" : "UNHEALTHY";
        var logLevel = healthy ? LogLevel.Debug : LogLevel.Warning;
        
        if (description != null)
        {
            logger.Log(logLevel, "Health check: {CheckName} is {Status} (checked in {Duration}ms) - {Description}", 
                checkName, status, duration.TotalMilliseconds, description);
        }
        else
        {
            logger.Log(logLevel, "Health check: {CheckName} is {Status} (checked in {Duration}ms)", 
                checkName, status, duration.TotalMilliseconds);
        }
    }
    
    #endregion
}