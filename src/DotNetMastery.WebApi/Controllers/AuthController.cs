using System.Security.Claims;
using DotNetMastery.Core.Security;
using DotNetMastery.WebApi.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotNetMastery.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtTokenService _jwtService;

    // Simulated user credentials database with securely hashed passwords
    private static readonly Dictionary<string, (string UserId, string PasswordHash, string[] Roles)> UsersDb = new(StringComparer.OrdinalIgnoreCase)
    {
        ["alice@example.com"] = ("user_101", PasswordHasher.HashPassword("SecurePassword123!"), ["User"]),
        ["admin@example.com"] = ("user_999", PasswordHasher.HashPassword("AdminSecretPassword2026!"), ["Admin", "User"])
    };

    // Simulated documents for IDOR verification
    private static readonly List<OwaspMitigations.DocumentRecord> DocumentsDb =
    [
        new(1, "user_101", "Confidential Medical Report for Alice"),
        new(2, "user_102", "Tax Information for Bob"),
        new(3, "user_999", "Company Financial Audit")
    ];

    public AuthController(JwtTokenService jwtService)
    {
        _jwtService = jwtService;
    }

    public record LoginRequest(string Email, string Password);
    public record LoginResponse(string AccessToken, string TokenType, int ExpiresInSeconds);

    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { error = "Email and password are required." });
        }

        if (!UsersDb.TryGetValue(request.Email, out var userData))
        {
            // Generic message to avoid username enumeration
            return Unauthorized(new { error = "Invalid credentials." });
        }

        bool isPasswordValid = PasswordHasher.VerifyPassword(request.Password, userData.PasswordHash);
        if (!isPasswordValid)
        {
            return Unauthorized(new { error = "Invalid credentials." });
        }

        string token = _jwtService.GenerateToken(userData.UserId, request.Email, userData.Roles, TimeSpan.FromHours(1));
        return Ok(new LoginResponse(token, "Bearer", 3600));
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUserProfile()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        string? email = User.FindFirstValue(ClaimTypes.Email);
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        return Ok(new
        {
            UserId = userId,
            Email = email,
            Roles = roles,
            Message = "Access granted to authenticated profile."
        });
    }

    [HttpGet("admin-only")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetAdminDashboard()
    {
        return Ok(new
        {
            Status = "Admin privileges confirmed.",
            AccessTimestampUtc = DateTime.UtcNow
        });
    }

    [HttpGet("document/{id:int}")]
    [Authorize]
    public IActionResult GetDocumentWithIdorProtection(int id)
    {
        string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized();
        }

        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var document = DocumentsDb.FirstOrDefault(d => d.Id == id);

        if (document == null)
        {
            return NotFound(new { error = $"Document {id} not found." });
        }

        // OWASP A01 BOLA/IDOR Defense: verify ownership or admin role
        if (!OwaspMitigations.CanAccessDocument(document, currentUserId, roles))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                error = "Access denied: you do not have permission to view this document (IDOR protected)."
            });
        }

        return Ok(document);
    }

    public record ValidateUrlRequest(string Url);

    [HttpPost("validate-url")]
    [Authorize]
    public IActionResult ValidateSsrfUrl([FromBody] ValidateUrlRequest request)
    {
        bool isSafe = OwaspMitigations.IsSafeOutboundUrl(request.Url);
        return Ok(new
        {
            Url = request.Url,
            IsSafeForOutbound = isSafe,
            Reason = isSafe ? "URL passed SSRF security checks." : "URL was blocked: internal IP, cloud metadata, or non-HTTPS scheme."
        });
    }
}
