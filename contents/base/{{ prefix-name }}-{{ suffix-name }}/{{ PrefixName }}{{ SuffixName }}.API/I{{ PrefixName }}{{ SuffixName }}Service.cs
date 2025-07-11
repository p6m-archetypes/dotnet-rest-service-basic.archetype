using {{ PrefixName }}{{ SuffixName }}.API.Dtos;

namespace {{ PrefixName }}{{ SuffixName }}.API;

public interface I{{ PrefixName }}{{ SuffixName }}Service
{
    Task<Create{{ PrefixName }}Response> Create{{ PrefixName }}Async({{ PrefixName }}Dto request);
    Task<Get{{ PrefixName }}sResponse> Get{{ PrefixName }}sAsync(Get{{ PrefixName }}sRequest request);
    Task<Get{{ PrefixName }}Response> Get{{ PrefixName }}Async(string id);
    Task<Update{{ PrefixName }}Response> Update{{ PrefixName }}Async({{ PrefixName }}Dto request);
    Task<Delete{{ PrefixName }}Response> Delete{{ PrefixName }}Async(string id);
}