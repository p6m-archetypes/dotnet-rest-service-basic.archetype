using {{ PrefixName }}{{ SuffixName }}.Persistence.Context;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Entities;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace {{ PrefixName }}{{ SuffixName }}.Persistence.Repositories;

public  abstract class BaseRepository<TEntity, TEntityId>
    where TEntity: AbstractEntity<TEntityId>
{
    protected readonly AppDbContext DbContext;
    protected readonly ILogger Logger;
    private readonly string _entityTypeName;

    protected BaseRepository(AppDbContext dbContext, ILogger logger)
    {
        DbContext = dbContext;
        Logger = logger;
        _entityTypeName = typeof(TEntity).Name;
    }
    
    public void Save(TEntity entity)
    {
        if(entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        Logger.LogDebug("Adding {EntityType} entity to context", _entityTypeName);
        
        try
        {
            DbContext.Set<TEntity>().Add(entity);
            Logger.LogDebug("Added {EntityType} entity to context", _entityTypeName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to add {EntityType} entity to context", _entityTypeName);
            throw;
        }
    }

    public IEnumerable<TEntity> GetAll()
    {
        Logger.LogDebug("Retrieving all {EntityType} entities", _entityTypeName);
        
        try
        {
            var entities = DbContext.Set<TEntity>().ToList();
            Logger.LogDebug("Retrieved {Count} {EntityType} entities", entities.Count, _entityTypeName);
            return entities;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to retrieve all {EntityType} entities", _entityTypeName);
            throw;
        }
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        Logger.LogDebug("Retrieving all {EntityType} entities asynchronously", _entityTypeName);
        
        try
        {
            var entities = await DbContext.Set<TEntity>().ToListAsync();
            Logger.LogDebug("Retrieved {Count} {EntityType} entities asynchronously", entities.Count(), _entityTypeName);
            return entities;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to retrieve all {EntityType} entities asynchronously", _entityTypeName);
            throw;
        }
    }

    public TEntity? FindById(string id)
    {
        Logger.LogDebug("Finding {EntityType} entity by ID: {EntityId}", _entityTypeName, id);
        
        try
        {
            var entity = DbContext.Set<TEntity>().Find(id);
            
            if (entity != null)
            {
                Logger.LogDebug("Found {EntityType} entity with ID: {EntityId}", _entityTypeName, id);
            }
            else
            {
                Logger.LogDebug("{EntityType} entity with ID: {EntityId} not found", _entityTypeName, id);
            }
                
            return entity;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to find {EntityType} entity with ID: {EntityId}", _entityTypeName, id);
            throw;
        }
    }
    
    public async Task<TEntity?> FindByIdAsync(Guid id)
    {
        Logger.LogDebug("Finding {EntityType} entity by ID asynchronously: {EntityId}", _entityTypeName, id);
        
        try
        {
            var entity = await DbContext.Set<TEntity>().FindAsync(id);
            
            if (entity != null)
            {
                Logger.LogDebug("Found {EntityType} entity with ID: {EntityId} asynchronously", _entityTypeName, id);
            }
            else
            {
                Logger.LogDebug("{EntityType} entity with ID: {EntityId} not found asynchronously", _entityTypeName, id);
            }
                
            return entity;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to find {EntityType} entity with ID: {EntityId} asynchronously", _entityTypeName, id);
            throw;
        }
    }

    public void Delete(TEntity entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }
        
        Logger.LogDebug("Removing {EntityType} entity from context", _entityTypeName);
        
        try
        {
            DbContext.Set<TEntity>().Remove(entity);
            Logger.LogDebug("Removed {EntityType} entity from context", _entityTypeName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to remove {EntityType} entity from context", _entityTypeName);
            throw;
        }
    }
    
    public async Task<Page<TEntity>> FindAsync(PageRequest request)
    {
        using var scope = Logger.BeginScope("Repository: {Repository}, Operation: {Operation}", 
            GetType().Name, "FindPaged");
            
        Logger.LogDebug("Finding {EntityType} entities with paging: page {StartPage}, size {PageSize}", 
            _entityTypeName, request.StartPage, request.PageSize);
        
        try
        {
            var totalRecords = await DbContext.Set<TEntity>().CountAsync();
            
            var entities = await DbContext.Set<TEntity>()
                .OrderBy((i) => i.Created)
                .Skip((request.StartPage - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();
            
            Logger.LogInformation("Found {Count} {EntityType} entities (total: {Total}) for page {StartPage}", 
                entities.Count, _entityTypeName, totalRecords, request.StartPage);

            return new Page<TEntity>
            {
                Items = entities,
                TotalElements = totalRecords
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to find {EntityType} entities with paging", _entityTypeName);
            throw;
        }
    }
    
    public void Update(TEntity entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }
        
        Logger.LogDebug("Updating {EntityType} entity in context", _entityTypeName);
        
        try
        {
            DbContext.Attach(entity);
            DbContext.Entry(entity).State = EntityState.Modified;
            Logger.LogDebug("Updated {EntityType} entity in context", _entityTypeName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to update {EntityType} entity in context", _entityTypeName);
            throw;
        }
    }

    public void Save()
    {
        Logger.LogDebug("Saving changes to database");
        
        try
        {
            var affectedRows = DbContext.SaveChanges();
            Logger.LogInformation("Saved {AffectedRows} changes to database", affectedRows);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to save changes to database");
            throw;
        }
    }

    public async Task SaveChangesAsync()
    {
        Logger.LogDebug("Saving changes to database asynchronously");
        
        try
        {
            var affectedRows = await DbContext.SaveChangesAsync();
            Logger.LogInformation("Saved {AffectedRows} changes to database asynchronously", affectedRows);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to save changes to database asynchronously");
            throw;
        }
    }
}