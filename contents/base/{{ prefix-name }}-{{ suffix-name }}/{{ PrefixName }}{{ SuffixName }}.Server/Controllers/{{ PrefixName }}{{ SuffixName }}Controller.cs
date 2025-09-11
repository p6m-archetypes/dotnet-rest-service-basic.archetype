using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using {{ PrefixName }}{{ SuffixName }}.API.Dtos;
using {{ PrefixName }}{{ SuffixName }}.Core;
using System.Diagnostics;

namespace {{ PrefixName }}{{ SuffixName }}.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class {{ PrefixName }}{{ SuffixName }}Controller : ControllerBase
{
    private readonly ILogger<{{ PrefixName }}{{ SuffixName }}Controller> _logger;
    private readonly {{ PrefixName }}{{ SuffixName }}Core _service;
    
    public {{ PrefixName }}{{ SuffixName }}Controller({{ PrefixName }}{{ SuffixName }}Core service, ILogger<{{ PrefixName }}{{ SuffixName }}Controller> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Roles = "admin,write")]
    public async Task<ActionResult<Create{{ PrefixName }}Response>> Create{{ PrefixName }}([FromBody] {{ PrefixName }}Dto request)
    {
        using var scope = _logger.BeginScope("REST: {Method}, User: {UserId}", 
            nameof(Create{{ PrefixName }}), GetUserId());
            
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogDebug("REST Create{{ PrefixName }} started for {Name}", request.Name);
            
            var response = await _service.Create{{ PrefixName }}(request);
            
            stopwatch.Stop();
            _logger.LogInformation("REST Create{{ PrefixName }} completed successfully in {Duration}ms", 
                stopwatch.ElapsedMilliseconds);
                
            return CreatedAtAction(nameof(Get{{ PrefixName }}), new { id = response.{{ PrefixName }}.Id }, response);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "REST Create{{ PrefixName }} failed after {Duration}ms", 
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "admin,write,read")]
    public async Task<ActionResult<Get{{ PrefixName }}Response>> Get{{ PrefixName }}(string id)
    {
        using var scope = _logger.BeginScope("REST: {Method}, User: {UserId}, Id: {Id}", 
            nameof(Get{{ PrefixName }}), GetUserId(), id);
            
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogDebug("REST Get{{ PrefixName }} started for ID {Id}", id);
            
            var response = await _service.Get{{ PrefixName }}(id);
            
            stopwatch.Stop();
            _logger.LogInformation("REST Get{{ PrefixName }} completed successfully in {Duration}ms", 
                stopwatch.ElapsedMilliseconds);
                
            return Ok(response);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "REST Get{{ PrefixName }} failed for ID {Id} after {Duration}ms", 
                id, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    [HttpGet]
    [Authorize(Roles = "admin,write,read")]
    public async Task<ActionResult<Get{{ PrefixName }}sResponse>> Get{{ PrefixName }}s([FromQuery] Get{{ PrefixName }}sRequest request)
    {
        using var scope = _logger.BeginScope("REST: {Method}, User: {UserId}, Page: {StartPage}, Size: {PageSize}", 
            nameof(Get{{ PrefixName }}s), GetUserId(), request.StartPage, request.PageSize);
            
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogDebug("REST Get{{ PrefixName }}s started for page {StartPage}, size {PageSize}", 
                request.StartPage, request.PageSize);
            
            var response = await _service.Get{{ PrefixName }}s(request);
            
            stopwatch.Stop();
            _logger.LogInformation("REST Get{{ PrefixName }}s completed successfully in {Duration}ms - returned {Count}/{Total} items", 
                stopwatch.ElapsedMilliseconds, response.{{ PrefixName }}s.Count, response.TotalElements);
                
            return Ok(response);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "REST Get{{ PrefixName }}s failed for page {StartPage}, size {PageSize} after {Duration}ms", 
                request.StartPage, request.PageSize, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin,write")]
    public async Task<ActionResult<Update{{ PrefixName }}Response>> Update{{ PrefixName }}(string id, [FromBody] {{ PrefixName }}Dto request)
    {
        request.Id = id; // Ensure the ID from the route is used
        
        using var scope = _logger.BeginScope("REST: {Method}, User: {UserId}, Id: {Id}", 
            nameof(Update{{ PrefixName }}), GetUserId(), request.Id);
            
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogDebug("REST Update{{ PrefixName }} started for ID {Id}", request.Id);
            
            var response = await _service.Update{{ PrefixName }}(request);
            
            stopwatch.Stop();
            _logger.LogInformation("REST Update{{ PrefixName }} completed successfully in {Duration}ms", 
                stopwatch.ElapsedMilliseconds);
                
            return Ok(response);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "REST Update{{ PrefixName }} failed for ID {Id} after {Duration}ms", 
                request.Id, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<Delete{{ PrefixName }}Response>> Delete{{ PrefixName }}(string id)
    {
        using var scope = _logger.BeginScope("REST: {Method}, User: {UserId}, Id: {Id}", 
            nameof(Delete{{ PrefixName }}), GetUserId(), id);
            
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogDebug("REST Delete{{ PrefixName }} started for ID {Id}", id);
            
            var response = await _service.Delete{{ PrefixName }}(id);
            
            stopwatch.Stop();
            _logger.LogInformation("REST Delete{{ PrefixName }} completed successfully in {Duration}ms - deleted: {Deleted}", 
                stopwatch.ElapsedMilliseconds, response.Deleted);
                
            return Ok(response);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "REST Delete{{ PrefixName }} failed for ID {Id} after {Duration}ms", 
                id, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
    
    /// <summary>
    /// Extracts user ID from HTTP context.
    /// </summary>
    private string? GetUserId()
    {
        // Try to get user ID from claims
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("userId");
        if (userIdClaim != null)
            return userIdClaim.Value;
            
        // Try to get from headers as fallback
        if (Request.Headers.TryGetValue("X-User-Id", out var userId))
            return userId.FirstOrDefault();
            
        if (Request.Headers.TryGetValue("User-Id", out var userIdAlt))
            return userIdAlt.FirstOrDefault();
            
        return null;
    }
}
