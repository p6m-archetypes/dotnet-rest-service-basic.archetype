using Microsoft.AspNetCore.Mvc;
using {{ PrefixName }}{{ SuffixName }}.Server.Services;

namespace {{ PrefixName }}{{ SuffixName }}.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthenticationService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("token")]
    public async Task<IActionResult> GetToken([FromBody] TokenRequest request)
    {
        try
        {
            // In a real implementation, you'd validate credentials against a user store
            if (!IsValidCredentials(request.ClientId, request.ClientSecret))
            {
                _logger.LogWarning("Invalid credentials for client: {ClientId}", request.ClientId);
                return Unauthorized(new { error = "invalid_credentials" });
            }

            var roles = GetRolesForClient(request.ClientId);
            var token = await _authService.GenerateTokenAsync(request.ClientId, roles);

            _logger.LogInformation("Generated token for client: {ClientId}", request.ClientId);

            return Ok(new TokenResponse
            {
                AccessToken = token,
                TokenType = "Bearer",
                ExpiresIn = 3600 // 1 hour
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating token for client: {ClientId}", request.ClientId);
            return StatusCode(500, new { error = "internal_server_error" });
        }
    }

    private static bool IsValidCredentials(string clientId, string clientSecret)
    {
        // Simple demo validation - in production, use proper credential validation
        return clientId switch
        {
            "admin-client" => clientSecret == "admin-secret",
            "read-client" => clientSecret == "read-secret",
            "write-client" => clientSecret == "write-secret",
            _ => false
        };
    }

    private static string[] GetRolesForClient(string clientId)
    {
        return clientId switch
        {
            "admin-client" => new[] { "admin", "write", "read" },
            "write-client" => new[] { "write", "read" },
            "read-client" => new[] { "read" },
            _ => Array.Empty<string>()
        };
    }
}

public class TokenRequest
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
}