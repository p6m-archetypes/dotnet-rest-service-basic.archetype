namespace {{ PrefixName }}{{ SuffixName }}.Core.Exceptions;

/// <summary>
/// Base class for all domain-specific exceptions
/// </summary>
public abstract class DomainException : Exception
{
    public string ErrorCode { get; }
    public object? ErrorDetails { get; }

    protected DomainException(string errorCode, string message, object? errorDetails = null) 
        : base(message)
    {
        ErrorCode = errorCode;
        ErrorDetails = errorDetails;
    }

    protected DomainException(string errorCode, string message, Exception innerException, object? errorDetails = null) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        ErrorDetails = errorDetails;
    }
}