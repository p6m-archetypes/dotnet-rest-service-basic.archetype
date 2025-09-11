using {{ PrefixName }}{{ SuffixName }}.API.Dtos;

namespace {{ PrefixName }}{{ SuffixName }}.API;

public interface I{{ PrefixName }}{{ SuffixName }}Service
{
    Task<Create{{ PrefixName }}Response> Create{{ PrefixName }}({{ PrefixName }}Dto request);
    Task<Get{{ PrefixName }}sResponse> Get{{ PrefixName }}s(Get{{ PrefixName }}sRequest request);
    Task<Get{{ PrefixName }}Response> Get{{ PrefixName }}(string id);
    Task<Update{{ PrefixName }}Response> Update{{ PrefixName }}({{ PrefixName }}Dto request);
    Task<Delete{{ PrefixName }}Response> Delete{{ PrefixName }}(string id);
}
