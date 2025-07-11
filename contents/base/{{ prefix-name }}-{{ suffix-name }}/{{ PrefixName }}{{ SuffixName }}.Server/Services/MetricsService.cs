using System.Diagnostics.Metrics;

namespace {{ PrefixName }}{{ SuffixName }}.Server.Services;

/// <summary>
/// Service for tracking custom business metrics
/// </summary>
public class MetricsService : IDisposable
{
    private readonly Meter _meter;
    private readonly Counter<long> _requestsCounter;
    private readonly Counter<long> _errorsCounter;
    private readonly Histogram<double> _requestDuration;
    private readonly Counter<long> _entitiesCreatedCounter;
    private readonly Counter<long> _entitiesUpdatedCounter;
    private readonly Counter<long> _entitiesDeletedCounter;
    private readonly UpDownCounter<long> _activeConnectionsCounter;
    private readonly Histogram<double> _databaseOperationDuration;
    private readonly Counter<long> _validationErrorsCounter;
    private readonly Counter<long> _authorizationFailuresCounter;

    public MetricsService()
    {
        _meter = new Meter("{{ PrefixName }}{{ SuffixName }}", "1.0.0");

        // Request metrics
        _requestsCounter = _meter.CreateCounter<long>(
            "project_prefix_requests_total",
            description: "Total number of gRPC requests");

        _errorsCounter = _meter.CreateCounter<long>(
            "project_prefix_errors_total",
            description: "Total number of request errors");

        _requestDuration = _meter.CreateHistogram<double>(
            "project_prefix_request_duration_seconds",
            unit: "s",
            description: "Duration of gRPC requests in seconds");

        // Business metrics
        _entitiesCreatedCounter = _meter.CreateCounter<long>(
            "project_prefix_entities_created_total",
            description: "Total number of entities created");

        _entitiesUpdatedCounter = _meter.CreateCounter<long>(
            "project_prefix_entities_updated_total",
            description: "Total number of entities updated");

        _entitiesDeletedCounter = _meter.CreateCounter<long>(
            "project_prefix_entities_deleted_total",
            description: "Total number of entities deleted");

        // Connection metrics
        _activeConnectionsCounter = _meter.CreateUpDownCounter<long>(
            "project_prefix_active_connections",
            description: "Number of active gRPC connections");

        // Database metrics
        _databaseOperationDuration = _meter.CreateHistogram<double>(
            "project_prefix_database_operation_duration_seconds",
            unit: "s",
            description: "Duration of database operations in seconds");

        // Error metrics
        _validationErrorsCounter = _meter.CreateCounter<long>(
            "project_prefix_validation_errors_total",
            description: "Total number of validation errors");

        _authorizationFailuresCounter = _meter.CreateCounter<long>(
            "project_prefix_authorization_failures_total",
            description: "Total number of authorization failures");

    }

    public void RecordRequest(string method, string status, double durationSeconds)
    {
        _requestsCounter.Add(1, new KeyValuePair<string, object?>("method", method), 
                                 new KeyValuePair<string, object?>("status", status));
        
        _requestDuration.Record(durationSeconds, new KeyValuePair<string, object?>("method", method),
                                                 new KeyValuePair<string, object?>("status", status));
    }

    public void RecordError(string method, string errorType)
    {
        _errorsCounter.Add(1, new KeyValuePair<string, object?>("method", method),
                              new KeyValuePair<string, object?>("error_type", errorType));
    }

    public void RecordEntityCreated()
    {
        _entitiesCreatedCounter.Add(1);
    }

    public void RecordEntityUpdated()
    {
        _entitiesUpdatedCounter.Add(1);
    }

    public void RecordEntityDeleted()
    {
        _entitiesDeletedCounter.Add(1);
    }

    public void RecordConnectionOpened()
    {
        _activeConnectionsCounter.Add(1);
    }

    public void RecordConnectionClosed()
    {
        _activeConnectionsCounter.Add(-1);
    }

    public void RecordDatabaseOperation(string operation, double durationSeconds, bool success)
    {
        _databaseOperationDuration.Record(durationSeconds, 
            new KeyValuePair<string, object?>("operation", operation),
            new KeyValuePair<string, object?>("success", success.ToString()));
    }

    public void RecordValidationError(string errorType)
    {
        _validationErrorsCounter.Add(1, new KeyValuePair<string, object?>("error_type", errorType));
    }

    public void RecordAuthorizationFailure(string operation, string reason)
    {
        _authorizationFailuresCounter.Add(1, 
            new KeyValuePair<string, object?>("operation", operation),
            new KeyValuePair<string, object?>("reason", reason));
    }


    public void Dispose()
    {
        _meter?.Dispose();
    }
}