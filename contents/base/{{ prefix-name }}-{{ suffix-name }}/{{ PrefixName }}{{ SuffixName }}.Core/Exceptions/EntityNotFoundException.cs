namespace {{ PrefixName }}{{ SuffixName }}.Core.Exceptions;

/// <summary>
/// Exception thrown when a requested entity is not found
/// </summary>
public class EntityNotFoundException : DomainException
{
    public string EntityType { get; }
    public string EntityId { get; }

    public EntityNotFoundException(string entityType, string entityId)
        : base("ENTITY_NOT_FOUND", $"{entityType} with ID '{entityId}' was not found.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    public EntityNotFoundException(string entityType, string entityId, Exception innerException)
        : base("ENTITY_NOT_FOUND", $"{entityType} with ID '{entityId}' was not found.", innerException)
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}