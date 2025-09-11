using Grpc.Core;
using {{ PrefixName }}{{ SuffixName }}.API;
using {{ PrefixName }}{{ SuffixName }}.API.Logger;
using {{ PrefixName }}{{ SuffixName }}.Core.Services;
using {{ PrefixName }}{{ SuffixName }}.Core.Exceptions;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Entities;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Models;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics; 

namespace {{ PrefixName }}{{ SuffixName }}.Core;

public class {{ PrefixName }}{{ SuffixName }}Core : I{{ PrefixName }}{{ SuffixName }}
{
    private readonly I{{ PrefixName }}Repository _{{ prefixName }}Repository;
    private readonly IValidationService _validationService;
    private readonly ILogger<{{ PrefixName }}{{ SuffixName }}Core> _logger;
       
    public {{ PrefixName }}{{ SuffixName }}Core(
        I{{ PrefixName }}Repository {{ prefixName }}Repository,
        IValidationService validationService,
        ILogger<{{ PrefixName }}{{ SuffixName }}Core> logger) 
    {
        _{{ prefixName }}Repository = {{ prefixName }}Repository;
        _validationService = validationService;
        _logger = logger;
    }

    public async Task<Create{{ PrefixName }}Response> Create{{ PrefixName }}({{ PrefixName }}Dto request)
    {
        using var scope = _logger.BeginScope("Operation: {Operation}, Entity: {EntityType}", 
            "Create{{ PrefixName }}", "{{ PrefixName }}");
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Validate input
            _validationService.ValidateCreateRequest(request);
            
            _logger.LogDebug("Creating {{ PrefixName }} entity: {Name}", request.Name);
            
            try
            {
                var {{ prefixName }} = new {{ PrefixName }}Entity
                {
                    Name = request.Name.Trim()
                };

                _{{ prefixName }}Repository.Save({{ prefixName }});
                await _{{ prefixName }}Repository.SaveChangesAsync();
                
                stopwatch.Stop();
                _logger.LogInformation("Successfully created {{ PrefixName }} entity {Id} in {Duration}ms", 
                    {{ prefixName }}.Id, stopwatch.ElapsedMilliseconds);
                
                return new Create{{ PrefixName }}Response
                {
                    {{ PrefixName }} = new {{ PrefixName }}Dto
                    {
                        Id = {{ prefixName }}.Id.ToString(),
                        Name = {{ prefixName }}.Name
                    }
                };
            }
            catch (DbUpdateException ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Database error creating {{ PrefixName }} entity {Name} after {Duration}ms", 
                    request.Name, stopwatch.ElapsedMilliseconds);
                throw new DataAccessException("Create", "Failed to save entity to database.", ex);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Unexpected error creating {{ PrefixName }} entity {Name} after {Duration}ms", 
                    request.Name, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
        catch (ValidationException ex)
        {
            stopwatch.Stop();
            _logger.LogWarning("Validation failed for Create{{ PrefixName }} {Name}: {Error} after {Duration}ms", 
                request?.Name, ex.Message, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (DataAccessException)
        {
            // Re-throw data access exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Create{{ PrefixName }} failed for {Name} after {Duration}ms", 
                request?.Name, stopwatch.ElapsedMilliseconds);
            throw new DataAccessException("Create", "An unexpected error occurred while creating the entity.", ex);
        }
    }

    public async Task<Get{{ PrefixName }}sResponse> Get{{ PrefixName }}s(Get{{ PrefixName }}sRequest request)
    { 
        using var scope = _logger.BeginScope("Operation: {Operation}, Entity: {EntityType}", 
            "Get{{ PrefixName }}s", "{{ PrefixName }}");
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Validate input
            _validationService.ValidatePaginationRequest(request);
            
            var startPage = Math.Max(1, request.StartPage);
            var pageSize = Math.Max(Math.Min(request.PageSize, 100), 1);
            
            _logger.LogDebug("Fetching {{ PrefixName }} entities: page {StartPage}, size {PageSize}", startPage, pageSize);
            
            try
            {
                PageRequest pageRequest = new PageRequest
                {
                    PageSize = pageSize,
                    StartPage = startPage
                };
                
                var page = await _{{ prefixName }}Repository.FindAsync(pageRequest);

                var response = new Get{{ PrefixName }}sResponse
                {
                    TotalElements = page.TotalElements,
                    TotalPages = (int)Math.Ceiling((double)page.TotalElements / pageSize)
                };
                response.{{ PrefixName }}s.AddRange(page.Items.Select({{ prefixName }} => new {{ PrefixName }}Dto
                {
                    Id = {{ prefixName }}.Id.ToString(),
                    Name = {{ prefixName }}.Name
                }));

                stopwatch.Stop();
                _logger.LogInformation("Fetched {Count} {{ PrefixName }} entities (total: {Total}) in {Duration}ms", 
                    page.Items.Count, page.TotalElements, stopwatch.ElapsedMilliseconds);
                
                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Database error fetching {{ PrefixName }} entities page {StartPage}, size {PageSize} after {Duration}ms", 
                    startPage, pageSize, stopwatch.ElapsedMilliseconds);
                throw new DataAccessException("Read", "Failed to retrieve entities from database.", ex);
            }
        }
        catch (ValidationException ex)
        {
            stopwatch.Stop();
            _logger.LogWarning("Validation failed for Get{{ PrefixName }}s page {StartPage}, size {PageSize}: {Error} after {Duration}ms", 
                request?.StartPage, request?.PageSize, ex.Message, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (DataAccessException)
        {
            // Re-throw data access exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Get{{ PrefixName }}s failed for page {StartPage}, size {PageSize} after {Duration}ms", 
                request?.StartPage, request?.PageSize, stopwatch.ElapsedMilliseconds);
            throw new DataAccessException("Read", "An unexpected error occurred while retrieving entities.", ex);
        }
    }

    public async Task<Get{{ PrefixName }}Response> Get{{ PrefixName }}(Get{{ PrefixName }}Request request)
    {
        using var scope = _logger.BeginScope("Operation: {Operation}, Entity: {EntityType}, Id: {Id}", 
            "Get{{ PrefixName }}", "{{ PrefixName }}", request.Id);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Validate and parse ID
            var entityId = _validationService.ValidateAndParseId(request.Id);
            
            _logger.LogDebug("Fetching {{ PrefixName }} entity by ID: {Id}", entityId);
            
            try
            {
                var {{ prefixName }} = await _{{ prefixName }}Repository.FindByIdAsync(entityId);
                if ({{ prefixName }} == null)
                {
                    stopwatch.Stop();
                    _logger.LogWarning("{{ PrefixName }} entity not found: {Id} after {Duration}ms", 
                        entityId, stopwatch.ElapsedMilliseconds);
                    throw new EntityNotFoundException("{{ PrefixName }}", entityId.ToString());
                }

                stopwatch.Stop();
                _logger.LogDebug("Found {{ PrefixName }} entity {Id} ({Name}) in {Duration}ms", 
                    {{ prefixName }}.Id, {{ prefixName }}.Name, stopwatch.ElapsedMilliseconds);
                
                return new Get{{ PrefixName }}Response
                {
                    {{ PrefixName }} = new {{ PrefixName }}Dto
                    {
                        Id = {{ prefixName }}.Id.ToString(),
                        Name = {{ prefixName }}.Name
                    }
                };
            }
            catch (EntityNotFoundException)
            {
                // Re-throw entity not found exceptions as-is
                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Database error fetching {{ PrefixName }} entity {Id} after {Duration}ms", 
                    entityId, stopwatch.ElapsedMilliseconds);
                throw new DataAccessException("Read", "Failed to retrieve entity from database.", ex);
            }
        }
        catch (ValidationException ex)
        {
            stopwatch.Stop();
            _logger.LogWarning("Validation failed for Get{{ PrefixName }} {Id}: {Error} after {Duration}ms", 
                request?.Id, ex.Message, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (EntityNotFoundException)
        {
            // Re-throw entity not found exceptions as-is
            throw;
        }
        catch (DataAccessException)
        {
            // Re-throw data access exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Get{{ PrefixName }} failed for {Id} after {Duration}ms", 
                request?.Id, stopwatch.ElapsedMilliseconds);
            throw new DataAccessException("Read", "An unexpected error occurred while retrieving the entity.", ex);
        }
    }

    public async Task<Update{{ PrefixName }}Response> Update{{ PrefixName }}({{ PrefixName }}Dto {{ prefixName }})
    {
        using var scope = _logger.BeginScope("Operation: {Operation}, Entity: {EntityType}, Id: {Id}", 
            "Update{{ PrefixName }}", "{{ PrefixName }}", {{ prefixName }}.Id);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Validate input
            _validationService.ValidateUpdateRequest({{ prefixName }});
            var entityId = _validationService.ValidateAndParseId({{ prefixName }}.Id);
            
            _logger.LogDebug("Updating {{ PrefixName }} entity: {Id} - {Name}", entityId, {{ prefixName }}.Name);
            
            try
            {
                var entity = await _{{ prefixName }}Repository.FindByIdAsync(entityId);
                if (entity == null)
                {
                    stopwatch.Stop();
                    _logger.LogWarning("{{ PrefixName }} entity not found for update: {Id} after {Duration}ms", 
                        entityId, stopwatch.ElapsedMilliseconds);
                    throw new EntityNotFoundException("{{ PrefixName }}", entityId.ToString());
                }

                // Check for business rules
                if (entity.Name == {{ prefixName }}.Name.Trim())
                {
                    stopwatch.Stop();
                    _logger.LogDebug("No changes detected for {{ PrefixName }} entity {Id} after {Duration}ms", 
                        entityId, stopwatch.ElapsedMilliseconds);
                    
                    return new Update{{ PrefixName }}Response
                    {
                        {{ PrefixName }} = new {{ PrefixName }}Dto
                        {
                            Id = entity.Id.ToString(),
                            Name = entity.Name
                        }
                    };
                }

                var oldName = entity.Name;
                entity.Name = {{ prefixName }}.Name.Trim();

                _{{ prefixName }}Repository.Update(entity);
                await _{{ prefixName }}Repository.SaveChangesAsync();

                stopwatch.Stop();
                _logger.LogInformation("Updated {{ PrefixName }} entity {Id} from '{OldName}' to '{NewName}' in {Duration}ms", 
                    entity.Id, oldName, entity.Name, stopwatch.ElapsedMilliseconds);

                return new Update{{ PrefixName }}Response
                {
                    {{ PrefixName }} = new {{ PrefixName }}Dto
                    {
                        Id = entity.Id.ToString(),
                        Name = entity.Name
                    }
                };
            }
            catch (EntityNotFoundException)
            {
                // Re-throw entity not found exceptions as-is
                throw;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                stopwatch.Stop();
                _logger.LogWarning(ex, "Concurrency conflict updating {{ PrefixName }} entity {Id} after {Duration}ms", 
                    entityId, stopwatch.ElapsedMilliseconds);
                throw new DataAccessException("Update", "The entity was modified by another user. Please refresh and try again.", ex);
            }
            catch (DbUpdateException ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Database error updating {{ PrefixName }} entity {Id} after {Duration}ms", 
                    entityId, stopwatch.ElapsedMilliseconds);
                throw new DataAccessException("Update", "Failed to update entity in database.", ex);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Unexpected error updating {{ PrefixName }} entity {Id} after {Duration}ms", 
                    entityId, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
        catch (ValidationException ex)
        {
            stopwatch.Stop();
            _logger.LogWarning("Validation failed for Update{{ PrefixName }} {Id}: {Error} after {Duration}ms", 
                {{ prefixName }}?.Id, ex.Message, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (EntityNotFoundException)
        {
            // Re-throw entity not found exceptions as-is
            throw;
        }
        catch (DataAccessException)
        {
            // Re-throw data access exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Update{{ PrefixName }} failed for {Id} after {Duration}ms", 
                {{ prefixName }}?.Id, stopwatch.ElapsedMilliseconds);
            throw new DataAccessException("Update", "An unexpected error occurred while updating the entity.", ex);
        }
    }

    public async Task<Delete{{ PrefixName }}Response> Delete{{ PrefixName }}(Delete{{ PrefixName }}Request request)
    {
        using var scope = _logger.BeginScope("Operation: {Operation}, Entity: {EntityType}, Id: {Id}", 
            "Delete{{ PrefixName }}", "{{ PrefixName }}", request.Id);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Validate and parse ID
            var entityId = _validationService.ValidateAndParseId(request.Id);
            
            _logger.LogDebug("Deleting {{ PrefixName }} entity by ID: {Id}", entityId);
            
            try
            {
                var {{ prefixName }} = await _{{ prefixName }}Repository.FindByIdAsync(entityId);
                if ({{ prefixName }} == null)
                {
                    stopwatch.Stop();
                    _logger.LogWarning("{{ PrefixName }} entity not found for deletion: {Id} after {Duration}ms", 
                        entityId, stopwatch.ElapsedMilliseconds);
                    throw new EntityNotFoundException("{{ PrefixName }}", entityId.ToString());
                }

                var entityName = {{ prefixName }}.Name; // Capture before deletion
                _{{ prefixName }}Repository.Delete({{ prefixName }});
                await _{{ prefixName }}Repository.SaveChangesAsync();

                stopwatch.Stop();
                _logger.LogInformation("Deleted {{ PrefixName }} entity {Id} ('{Name}') in {Duration}ms", 
                    {{ prefixName }}.Id, entityName, stopwatch.ElapsedMilliseconds);

                return new Delete{{ PrefixName }}Response { Deleted = true };
            }
            catch (EntityNotFoundException)
            {
                // Re-throw entity not found exceptions as-is
                throw;
            }
            catch (DbUpdateException ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Database error deleting {{ PrefixName }} entity {Id} after {Duration}ms", 
                    entityId, stopwatch.ElapsedMilliseconds);
                throw new DataAccessException("Delete", "Failed to delete entity from database.", ex);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Unexpected error deleting {{ PrefixName }} entity {Id} after {Duration}ms", 
                    entityId, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
        catch (ValidationException ex)
        {
            stopwatch.Stop();
            _logger.LogWarning("Validation failed for Delete{{ PrefixName }} {Id}: {Error} after {Duration}ms", 
                request?.Id, ex.Message, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (EntityNotFoundException)
        {
            // Re-throw entity not found exceptions as-is
            throw;
        }
        catch (DataAccessException)
        {
            // Re-throw data access exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Delete{{ PrefixName }} failed for {Id} after {Duration}ms", 
                request?.Id, stopwatch.ElapsedMilliseconds);
            throw new DataAccessException("Delete", "An unexpected error occurred while deleting the entity.", ex);
        }
    }
}
