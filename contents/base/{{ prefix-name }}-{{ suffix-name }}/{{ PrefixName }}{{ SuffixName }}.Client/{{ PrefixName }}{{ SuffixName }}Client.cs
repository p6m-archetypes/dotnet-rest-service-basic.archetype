using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using {{ PrefixName }}{{ SuffixName }}.API;
using {{ PrefixName }}{{ SuffixName }}.API.Dtos;

namespace {{ PrefixName }}{{ SuffixName }}.Client;

public class {{ PrefixName }}{{ SuffixName }}Client : I{{ PrefixName }}{{ SuffixName }}Service
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly bool _ownsHttpClient;

    private {{ PrefixName }}{{ SuffixName }}Client(HttpClient httpClient, string baseUrl, bool ownsHttpClient = false)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl.TrimEnd('/');
        _ownsHttpClient = ownsHttpClient;
    }

    public static {{ PrefixName }}{{ SuffixName }}Client Of(string baseUrl)
    {
        var httpClient = new HttpClient();
        return new {{ PrefixName }}{{ SuffixName }}Client(httpClient, baseUrl, ownsHttpClient: true);
    }

    public static {{ PrefixName }}{{ SuffixName }}Client Of(HttpClient httpClient, string baseUrl)
    {
        return new {{ PrefixName }}{{ SuffixName }}Client(httpClient, baseUrl, ownsHttpClient: false);
    }

    public async Task<Create{{ PrefixName }}Response> Create{{ PrefixName }}({{ PrefixName }}Dto request)
    {
        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/{{ PrefixName }}{{ SuffixName }}", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Create{{ PrefixName }}Response>() ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<Get{{ PrefixName }}sResponse> Get{{ PrefixName }}s(Get{{ PrefixName }}sRequest request)
    {
        var queryString = $"?startPage={request.StartPage}&pageSize={request.PageSize}";
        var response = await _httpClient.GetAsync($"{_baseUrl}/api/{{ PrefixName }}{{ SuffixName }}{queryString}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Get{{ PrefixName }}sResponse>() ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<Get{{ PrefixName }}Response> Get{{ PrefixName }}(string id)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/api/{{ PrefixName }}{{ SuffixName }}/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Get{{ PrefixName }}Response>() ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<Update{{ PrefixName }}Response> Update{{ PrefixName }}({{ PrefixName }}Dto request)
    {
        if (string.IsNullOrEmpty(request.Id))
            throw new ArgumentException("Id is required for update operation", nameof(request));
            
        var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/api/{{ PrefixName }}{{ SuffixName }}/{request.Id}", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Update{{ PrefixName }}Response>() ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<Delete{{ PrefixName }}Response> Delete{{ PrefixName }}(string id)
    {
        var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/{{ PrefixName }}{{ SuffixName }}/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Delete{{ PrefixName }}Response>() ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public void SetAuthorizationToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient?.Dispose();
        }
    }
}
