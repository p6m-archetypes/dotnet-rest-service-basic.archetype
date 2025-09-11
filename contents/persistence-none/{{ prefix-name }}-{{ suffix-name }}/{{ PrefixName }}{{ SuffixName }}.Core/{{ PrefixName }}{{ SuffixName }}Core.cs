using {{ PrefixName }}{{ SuffixName }}.API;
using {{ PrefixName }}{{ SuffixName }}.API.Dtos;
using {{ PrefixName }}{{ SuffixName }}.API.Logger;
using {{ PrefixName }}{{ SuffixName }}.Core.Services;
using {{ PrefixName }}{{ SuffixName }}.Core.Exceptions;
using Microsoft.Extensions.Logging;
using System.Diagnostics; 

namespace {{ PrefixName }}{{ SuffixName }}.Core;

public class {{ PrefixName }}{{ SuffixName }}Core : I{{ PrefixName }}{{ SuffixName }}Service
{
    private readonly IValidationService _validationService;
    private readonly ILogger<{{ PrefixName }}{{ SuffixName }}Core> _logger;
       
    public {{ PrefixName }}{{ SuffixName }}Core(
        IValidationService validationService,
        ILogger<{{ PrefixName }}{{ SuffixName }}Core> logger) 
    {
        _validationService = validationService;
        _logger = logger;
    }

    public Task<Create{{ PrefixName }}Response> Create{{ PrefixName }}({{ PrefixName }}Dto request)
    {
          return Task.FromResult(new Create{{ PrefixName }}Response
          {
              {{ PrefixName }} = new {{ PrefixName }}Dto
              {
                  Id = request.Id,
                  Name = request.Name
              }
          });
    }

    public Task<Get{{ PrefixName }}sResponse> Get{{ PrefixName }}s(Get{{ PrefixName }}sRequest request)
    {
        return Task.FromResult(new Get{{ PrefixName }}sResponse
        {
            TotalElements = 0,
            TotalPages = 0,
        });
    }

    public Task<Get{{ PrefixName }}Response> Get{{ PrefixName }}(string id)
    { 
        return Task.FromResult(new Get{{ PrefixName }}Response
        {
            {{ PrefixName }} = new {{ PrefixName }}Dto
            {
                Id = "{{ PrefixName }}Id",
                Name = "{{ PrefixName }}Name",
            }
        });
    }

    public Task<Update{{ PrefixName }}Response> Update{{ PrefixName }}({{ PrefixName }}Dto {{ prefixName }})
    {
        return Task.FromResult(new Update{{ PrefixName }}Response
        {
            {{ PrefixName }} = new {{ PrefixName }}Dto
            {
                Id = {{ prefixName }}.Id,
                Name = {{ prefixName }}.Name
            }
        });
    }

    public Task<Delete{{ PrefixName }}Response> Delete{{ PrefixName }}(string id)
    {
        return Task.FromResult(new Delete{{ PrefixName }}Response { Deleted = false });
    }
}
