using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using {{ PrefixName }}{{ SuffixName }}.Client;
using {{ PrefixName }}{{ SuffixName }}.Server;
using {{ PrefixName }}{{ SuffixName }}.Server.Controllers;

namespace {{ PrefixName }}{{ SuffixName }}.IntegrationTests;

public class ApplicationFixture: IDisposable
{
    private readonly {{ PrefixName }}{{ SuffixName }}Server _server;
    private readonly {{ PrefixName }}{{ SuffixName }}Client _client;
    private readonly HttpClient _httpClient;
    
    public ApplicationFixture()
    {
        _server = new {{ PrefixName }}{{ SuffixName }}Server()
            .WithEphemeral()
            .WithRandomPorts()
            .Start();
        
        var httpUrl = _server.getHttpUrl();
        if (string.IsNullOrEmpty(httpUrl))
        {
            throw new InvalidOperationException("Failed to get HTTP server URL");
        }
        
        
        _httpClient = new HttpClient();
        _client = {{ PrefixName }}{{ SuffixName }}Client.Of(_httpClient, httpUrl);
        
        // Wait for server to be ready
        WaitForServerReady(httpUrl).Wait();
        
        // Get authentication token for tests
        var tokenTask = GetAuthTokenAsync(httpUrl);
        tokenTask.Wait();
        var token = tokenTask.Result;
        
        _client.SetAuthorizationToken(token);
    }
    
    private async Task WaitForServerReady(string baseUrl)
    {
        var maxRetries = 30;
        var delay = TimeSpan.FromSeconds(1);
        
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{baseUrl}/health");
                if (response.IsSuccessStatusCode)
                {
                        return;
                }
            }
            catch
            {
                // Server not ready yet
            }
            
            if (i < maxRetries - 1)
            {
                await Task.Delay(delay);
            }
        }
        
        throw new InvalidOperationException("Server failed to start within timeout period");
    }
    
    private async Task<string> GetAuthTokenAsync(string baseUrl)
    {
        var tokenRequest = new TokenRequest
        {
            ClientId = "admin-client",
            ClientSecret = "admin-secret"
        };
        
        var tokenUrl = $"{baseUrl}/api/auth/token";
        
        var response = await _httpClient.PostAsJsonAsync(tokenUrl, tokenRequest);
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to get auth token from {tokenUrl}. Status: {response.StatusCode}, Content: {content}");
        }
        
        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return tokenResponse?.AccessToken ?? throw new InvalidOperationException("Failed to get auth token");
    }
    
    public {{ PrefixName }}{{ SuffixName }}Client GetClient() => _client;
    public {{ PrefixName }}{{ SuffixName }}Server GetServer() => _server;

    public void Dispose()
    {
        _httpClient?.Dispose();
        _server.Stop();
    }
}

[CollectionDefinition("ApplicationCollection")]
public class ApplicationCollection : ICollectionFixture<ApplicationFixture>
{
    // This class has no code; it's just a marker for the test collection
}