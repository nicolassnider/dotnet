using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DotNetMastery.WebApi.Security;

/// <summary>
/// Handles OAuth 2.0 / OpenID Connect JSON Web Token (JWT) generation and validation.
/// Implements standard claims (sub, email, role, jti, exp) and strict signature verification.
/// </summary>
public class JwtTokenService
{
    public const string DefaultIssuer = "DotNetMastery.IdP";
    public const string DefaultAudience = "DotNetMastery.Api";

    // 256-bit symmetric key for HMAC-SHA256 signing (in production, retrieved from Azure Key Vault or AWS Secrets Manager)
    private readonly SymmetricSecurityKey _signingKey;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtTokenService(
        string secretKey = "DotNetMasterySuperSecretKeyForJwtAuthentication2026!",
        string issuer = DefaultIssuer,
        string audience = DefaultAudience)
    {
        _issuer = issuer;
        _audience = audience;
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    }

    public SymmetricSecurityKey SigningKey => _signingKey;
    public string Issuer => _issuer;
    public string Audience => _audience;

    public string GenerateToken(
        string userId,
        string email,
        IEnumerable<string> roles,
        TimeSpan? lifetime = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(ClaimTypes.NameIdentifier, userId)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;
        var expires = now.Add(lifetime ?? TimeSpan.FromHours(1));
        var notBefore = expires < now ? expires.AddMinutes(-1) : now;

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = notBefore,
            Expires = expires,
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = credentials
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _signingKey,
            ValidateIssuer = true,
            ValidIssuer = _issuer,
            ValidateAudience = true,
            ValidAudience = _audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // Strict lifetime without 5-minute leeway
        };

        try
        {
            return handler.ValidateToken(token, validationParameters, out _);
        }
        catch
        {
            return null;
        }
    }
}
