namespace {{ PrefixName }}{{ SuffixName }}.Core.Exceptions;

/// <summary>
/// Exception thrown when data access operations fail
/// </summary>
public class DataAccessException : DomainException
{
    public string Operation { get; }

    public DataAccessException(string operation, string message)
        : base("DATA_ACCESS_ERROR", message)
    {
        Operation = operation;
    }

    public DataAccessException(string operation, string message, Exception innerException)
        : base("DATA_ACCESS_ERROR", message, innerException)
    {
        Operation = operation;
    }
}