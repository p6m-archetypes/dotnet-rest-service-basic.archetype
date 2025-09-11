using {{ PrefixName}}{{ SuffixName }}.API;
using {{ PrefixName}}{{ SuffixName }}.Core.Services;
using Microsoft.Extensions.Logging;

namespace {{ PrefixName}}{{ SuffixName }}.Core;

public class {{ PrefixName }}{{ SuffixName }}Core : I{{ PrefixName }}{{ SuffixName }}
{
    private readonly IValidationService _validationService;
    private readonly ILogger<{{ PrefixName}}{{ SuffixName }}Core> _logger;
       
    public {{ PrefixName}}{{ SuffixName }}Core(
        IValidationService validationService,
        ILogger<{{ PrefixName}}{{ SuffixName }}Core> logger) 
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

    public Task<Get{{ PrefixName }}Response> Get{{ PrefixName }}(Get{{ PrefixName }}Request request)
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

    public Task<Delete{{ PrefixName }}Response> Delete{{ PrefixName }}(Delete{{ PrefixName }}Request request)
    {
        return Task.FromResult(new Delete{{ PrefixName }}Response { Deleted = false });
    }
}
