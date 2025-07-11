using {{ PrefixName }}{{ SuffixName }}.API.Dtos;
using {{ PrefixName }}{{ SuffixName }}.Core.Exceptions;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace {{ PrefixName }}{{ SuffixName }}.Core.Services;

/// <summary>
/// Service for validating requests and business rules
/// </summary>
public class ValidationService : IValidationService
{
    private readonly ILogger<ValidationService> _logger;
    private static readonly Regex NamePattern = new(@"^[a-zA-Z0-9\s\-_\.]{1,100}$", RegexOptions.Compiled);

    public ValidationService(ILogger<ValidationService> logger)
    {
        _logger = logger;
    }

    public void ValidateCreateRequest({{ PrefixName }}Dto request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request == null)
        {
            throw new ValidationException("request", "Request cannot be null.");
        }

        // Validate Name
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors["Name"] = new[] { "Name is required and cannot be empty." };
        }
        else if (request.Name.Length > 100)
        {
            errors["Name"] = new[] { "Name cannot exceed 100 characters." };
        }
        else if (!NamePattern.IsMatch(request.Name))
        {
            errors["Name"] = new[] { "Name contains invalid characters. Only letters, numbers, spaces, hyphens, underscores, and dots are allowed." };
        }

        // ID should not be provided for create requests
        if (!string.IsNullOrEmpty(request.Id))
        {
            errors["Id"] = new[] { "ID should not be provided for create requests." };
        }

        if (errors.Any())
        {
            _logger.LogWarning("Validation failed for create request: {@ValidationErrors}", errors);
            throw new ValidationException(errors);
        }

        _logger.LogDebug("Create request validation passed for: {Name}", request.Name);
    }

    public void ValidateUpdateRequest({{ PrefixName }}Dto request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request == null)
        {
            throw new ValidationException("request", "Request cannot be null.");
        }

        // Validate ID
        if (string.IsNullOrWhiteSpace(request.Id))
        {
            errors["Id"] = new[] { "ID is required for update requests." };
        }
        else
        {
            try
            {
                Guid.Parse(request.Id);
            }
            catch (FormatException)
            {
                errors["Id"] = new[] { "ID must be a valid GUID format." };
            }
        }

        // Validate Name
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors["Name"] = new[] { "Name is required and cannot be empty." };
        }
        else if (request.Name.Length > 100)
        {
            errors["Name"] = new[] { "Name cannot exceed 100 characters." };
        }
        else if (!NamePattern.IsMatch(request.Name))
        {
            errors["Name"] = new[] { "Name contains invalid characters. Only letters, numbers, spaces, hyphens, underscores, and dots are allowed." };
        }

        if (errors.Any())
        {
            _logger.LogWarning("Validation failed for update request: {@ValidationErrors}", errors);
            throw new ValidationException(errors);
        }

        _logger.LogDebug("Update request validation passed for: {Id} - {Name}", request.Id, request.Name);
    }

    public void ValidatePaginationRequest(Get{{ PrefixName }}sRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request == null)
        {
            throw new ValidationException("request", "Request cannot be null.");
        }

        if (request.StartPage < 1)
        {
            errors["StartPage"] = new[] { "StartPage must be greater than 0." };
        }

        if (request.PageSize < 1)
        {
            errors["PageSize"] = new[] { "PageSize must be greater than 0." };
        }
        else if (request.PageSize > 1000)
        {
            errors["PageSize"] = new[] { "PageSize cannot exceed 1000." };
        }

        if (errors.Any())
        {
            _logger.LogWarning("Validation failed for pagination request: {@ValidationErrors}", errors);
            throw new ValidationException(errors);
        }

        _logger.LogDebug("Pagination request validation passed: StartPage={StartPage}, PageSize={PageSize}", 
            request.StartPage, request.PageSize);
    }

    public Guid ValidateAndParseId(string id, string fieldName = "Id")
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ValidationException(fieldName, $"{fieldName} is required and cannot be empty.");
        }

        try
        {
            var parsedId = Guid.Parse(id);
            
            if (parsedId == Guid.Empty)
            {
                throw new ValidationException(fieldName, $"{fieldName} cannot be an empty GUID.");
            }

            return parsedId;
        }
        catch (FormatException ex)
        {
            _logger.LogWarning(ex, "Invalid GUID format for {FieldName}: {Id}", fieldName, id);
            throw new ValidationException(fieldName, $"{fieldName} must be a valid GUID format.");
        }
    }
}