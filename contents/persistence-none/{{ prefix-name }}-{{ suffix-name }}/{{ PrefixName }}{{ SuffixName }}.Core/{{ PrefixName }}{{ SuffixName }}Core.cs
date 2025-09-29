using {{ PrefixName }}{{ SuffixName }}.API;
using {{ PrefixName }}{{ SuffixName }}.API.Dtos;
using {{ PrefixName }}{{ SuffixName }}.API.Logger;
using {{ PrefixName }}{{ SuffixName }}.Core.Services;
using {{ PrefixName }}{{ SuffixName }}.Core.Exceptions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace {{ PrefixName }}{{ SuffixName }}.Core;

public class {{ PrefixName }}{{ SuffixName }}Core : I{{ PrefixName }}{{ SuffixName }}Service
{
    private readonly IValidationService _validationService;
    private readonly ILogger<{{ PrefixName }}{{ SuffixName }}Core> _logger;
    private readonly ConcurrentDictionary<string, {{ PrefixName }}Dto> _inMemoryStore = new();
       
    public {{ PrefixName }}{{ SuffixName }}Core(
        IValidationService validationService,
        ILogger<{{ PrefixName }}{{ SuffixName }}Core> logger) 
    {
        _validationService = validationService;
        _logger = logger;
    }

    public Task<Create{{ PrefixName }}Response> Create{{ PrefixName }}({{ PrefixName }}Dto request)
    {
        var id = string.IsNullOrEmpty(request.Id) ? Guid.NewGuid().ToString() : request.Id;
        var {{ prefixName }} = new {{ PrefixName }}Dto
        {
            Id = id,
            Name = request.Name
        };
        _inMemoryStore[id] = {{ prefixName }};

        return Task.FromResult(new Create{{ PrefixName }}Response
        {
            {{ PrefixName }} = {{ prefixName }}
        });
    }

    public Task<Get{{ PrefixName }}sResponse> Get{{ PrefixName }}s(Get{{ PrefixName }}sRequest request)
    {
        var all{{ PrefixName }}s = _inMemoryStore.Values.ToList();
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;
        var startPage = request.StartPage > 0 ? request.StartPage : 1;

        var totalElements = all{{ PrefixName }}s.Count;
        var totalPages = (int)Math.Ceiling((double)totalElements / pageSize);

        var skip = (startPage - 1) * pageSize;
        var paged{{ PrefixName }}s = all{{ PrefixName }}s
            .Skip(skip)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(new Get{{ PrefixName }}sResponse
        {
            {{ PrefixName }}s = paged{{ PrefixName }}s,
            TotalElements = totalElements,
            TotalPages = totalPages,
        });
    }

    public Task<Get{{ PrefixName }}Response> Get{{ PrefixName }}(string id)
    { 
        if (!_inMemoryStore.TryGetValue(id, out var {{ prefixName }}))
        {
            throw new EntityNotFoundException("{{ PrefixName }}", id);
        }

        return Task.FromResult(new Get{{ PrefixName }}Response
        {
            {{ PrefixName }} = {{ prefixName }}
        });
    }

    public Task<Update{{ PrefixName }}Response> Update{{ PrefixName }}({{ PrefixName }}Dto {{ prefixName }})
    {
        if (string.IsNullOrEmpty({{ prefixName }}.Id) || !_inMemoryStore.ContainsKey({{ prefixName }}.Id))
        {
            throw new EntityNotFoundException("{{ PrefixName }}", {{ prefixName }}.Id ?? "null");
        }

        _inMemoryStore[{{ prefixName }}.Id] = {{ prefixName }};

        return Task.FromResult(new Update{{ PrefixName }}Response
        {
            {{ PrefixName }} = {{ prefixName }}
        });
    }

    public Task<Delete{{ PrefixName }}Response> Delete{{ PrefixName }}(string id)
    {
        if (!_inMemoryStore.TryRemove(id, out _))
        {
            throw new EntityNotFoundException("{{ PrefixName }}", id);
        }

        return Task.FromResult(new Delete{{ PrefixName }}Response { Deleted = true });
    }
}
