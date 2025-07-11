using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace {{ PrefixName }}{{ SuffixName }}.Server.Services;

public class JwtAuthenticationOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}

public class JwtAuthenticationService : IAuthenticationService
{
    private readonly JwtAuthenticationOptions _options;
    private readonly ILogger<JwtAuthenticationService> _logger;
    private readonly SymmetricSecurityKey _signingKey;

    public JwtAuthenticationService(
        IConfiguration configuration, 
        ILogger<JwtAuthenticationService> logger)
    {
        _logger = logger;
        _options = new JwtAuthenticationOptions();
        configuration.GetSection("Authentication:Jwt").Bind(_options);
        
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
    }

    public Task<string> GenerateTokenAsync(string clientId, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, clientId),
            new(ClaimTypes.Name, clientId),
            new("client_id", clientId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, 
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                ClaimValueTypes.Integer64)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        _logger.LogDebug("Generated JWT token for client: {ClientId}", clientId);
        return Task.FromResult(tokenString);
    }

    public Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,
                ValidateIssuer = true,
                ValidIssuer = _options.Issuer,
                ValidateAudience = true,
                ValidAudience = _options.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            _logger.LogDebug("Successfully validated JWT token");
            return Task.FromResult<ClaimsPrincipal?>(principal);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to validate JWT token");
            return Task.FromResult<ClaimsPrincipal?>(null);
        }
    }

    public Task<bool> IsAuthorizedAsync(ClaimsPrincipal principal, string operation)
    {
        if (!principal.Identity?.IsAuthenticated == true)
        {
            return Task.FromResult(false);
        }

        // Simple role-based authorization
        var authorized = operation.ToLowerInvariant() switch
        {
            "create" => principal.IsInRole("admin") || principal.IsInRole("write"),
            "read" => principal.IsInRole("admin") || principal.IsInRole("write") || principal.IsInRole("read"),
            "update" => principal.IsInRole("admin") || principal.IsInRole("write"),
            "delete" => principal.IsInRole("admin"),
            _ => false
        };

        _logger.LogDebug("Authorization check for operation {Operation}: {Authorized}", operation, authorized);
        return Task.FromResult(authorized);
    }
}