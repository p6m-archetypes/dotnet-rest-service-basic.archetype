using {{ PrefixName }}{{ SuffixName }}.API.Dtos;

namespace {{ PrefixName }}{{ SuffixName }}.Core.Services;

/// <summary>
/// Service for validating requests and business rules
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validates a {{ PrefixName }} creation request
    /// </summary>
    void ValidateCreateRequest({{ PrefixName }}Dto request);

    /// <summary>
    /// Validates a {{ PrefixName }} update request
    /// </summary>
    void ValidateUpdateRequest({{ PrefixName }}Dto request);

    /// <summary>
    /// Validates pagination parameters
    /// </summary>
    void ValidatePaginationRequest(Get{{ PrefixName }}sRequest request);

    /// <summary>
    /// Validates an entity ID
    /// </summary>
    Guid ValidateAndParseId(string id, string fieldName = "Id");
}