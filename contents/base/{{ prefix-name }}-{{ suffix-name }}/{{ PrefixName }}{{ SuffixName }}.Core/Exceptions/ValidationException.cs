namespace {{ PrefixName }}{{ SuffixName }}.Core.Exceptions;

/// <summary>
/// Exception thrown when input validation fails
/// </summary>
public class ValidationException : DomainException
{
    public Dictionary<string, string[]> ValidationErrors { get; }

    public ValidationException(string field, string error)
        : base("VALIDATION_ERROR", $"Validation failed for field '{field}': {error}")
    {
        ValidationErrors = new Dictionary<string, string[]>
        {
            { field, new[] { error } }
        };
    }

    public ValidationException(Dictionary<string, string[]> validationErrors)
        : base("VALIDATION_ERROR", "One or more validation errors occurred.", validationErrors)
    {
        ValidationErrors = validationErrors;
    }

    public ValidationException(string message, Dictionary<string, string[]> validationErrors)
        : base("VALIDATION_ERROR", message, validationErrors)
    {
        ValidationErrors = validationErrors;
    }
}