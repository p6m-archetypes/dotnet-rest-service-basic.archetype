namespace {{ PrefixName }}{{ SuffixName }}.Core.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public class BusinessRuleException : DomainException
{
    public string RuleName { get; }

    public BusinessRuleException(string ruleName, string message)
        : base("BUSINESS_RULE_VIOLATION", message)
    {
        RuleName = ruleName;
    }

    public BusinessRuleException(string ruleName, string message, Exception innerException)
        : base("BUSINESS_RULE_VIOLATION", message, innerException)
    {
        RuleName = ruleName;
    }
}