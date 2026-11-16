using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using DotNetMastery.Core.Security;
using DotNetMastery.WebApi.Controllers;
using DotNetMastery.WebApi.Security;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DotNetMastery.Tests;

public class SecurityTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JwtTokenService _jwtService = new();

    public SecurityTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public void PasswordHasher_ShouldHashAndVerify_WithCorrectPassword()
    {
        string rawPassword = "P@ssword_2026_Enterprise!";
        string hashed = PasswordHasher.HashPassword(rawPassword);

        Assert.NotEmpty(hashed);
        Assert.True(PasswordHasher.VerifyPassword(rawPassword, hashed));
        Assert.False(PasswordHasher.VerifyPassword("WrongPassword123!", hashed));
    }

    [Fact]
    public void PasswordHasher_ShouldGenerateUniqueSalts_ForIdenticalPasswords()
    {
        string rawPassword = "CommonPassword99";
        string hash1 = PasswordHasher.HashPassword(rawPassword);
        string hash2 = PasswordHasher.HashPassword(rawPassword);

        Assert.NotEqual(hash1, hash2);
        Assert.True(PasswordHasher.VerifyPassword(rawPassword, hash1));
        Assert.True(PasswordHasher.VerifyPassword(rawPassword, hash2));
    }

    [Fact]
    public void Owasp_SsrfProtection_ShouldBlockInternalAndMetadataIps()
    {
        // Block non-https
        Assert.False(OwaspMitigations.IsSafeOutboundUrl("http://google.com"));

        // Block localhost / loopback
        Assert.False(OwaspMitigations.IsSafeOutboundUrl("https://localhost/api"));
        Assert.False(OwaspMitigations.IsSafeOutboundUrl("https://127.0.0.1:8080"));

        // Block Cloud Metadata (AWS/Azure/GCP 169.254.169.254)
        Assert.False(OwaspMitigations.IsSafeOutboundUrl("https://169.254.169.254/latest/meta-data/"));

        // Block RFC1918 Private ranges
        Assert.False(OwaspMitigations.IsSafeOutboundUrl("https://10.0.0.5/admin"));
        Assert.False(OwaspMitigations.IsSafeOutboundUrl("https://192.168.1.100/router"));
        Assert.False(OwaspMitigations.IsSafeOutboundUrl("https://172.20.0.1/status"));

        // Allow legitimate public HTTPS domains
        Assert.True(OwaspMitigations.IsSafeOutboundUrl("https://api.github.com/users"));
    }

    [Fact]
    public void Owasp_IdorProtection_ShouldEnforceStrictOwnershipOrAdmin()
    {
        var document = new OwaspMitigations.DocumentRecord(10, "user_alice", "Confidential Salary Report");

        // Alice accessing her own document -> allowed
        Assert.True(OwaspMitigations.CanAccessDocument(document, "user_alice", ["User"]));

        // Bob accessing Alice's document without admin role -> blocked (IDOR mitigated)
        Assert.False(OwaspMitigations.CanAccessDocument(document, "user_bob", ["User"]));

        // Admin accessing Alice's document -> allowed
        Assert.True(OwaspMitigations.CanAccessDocument(document, "user_bob", ["Admin", "User"]));
    }

    [Fact]
    public void JwtTokenService_ShouldGenerateAndValidateValidToken()
    {
        string token = _jwtService.GenerateToken("user_42", "dev@example.com", ["Developer", "User"]);
        var principal = _jwtService.ValidateToken(token);

        Assert.NotNull(principal);
        Assert.Equal("user_42", principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal("dev@example.com", principal.FindFirstValue(ClaimTypes.Email));
        Assert.True(principal.IsInRole("Developer"));
    }

    [Fact]
    public void JwtTokenService_ShouldRejectTamperedToken()
    {
        string validToken = _jwtService.GenerateToken("user_42", "dev@example.com", ["User"]);
        // Tamper with payload characters
        string tamperedToken = validToken[..^4] + "abcd";

        var principal = _jwtService.ValidateToken(tamperedToken);
        Assert.Null(principal);
    }

    [Fact]
    public void JwtTokenService_ShouldRejectExpiredToken()
    {
        // Create token with negative lifetime (already expired)
        string expiredToken = _jwtService.GenerateToken(
            "user_42",
            "dev@example.com",
            ["User"],
            lifetime: TimeSpan.FromSeconds(-10));

        var principal = _jwtService.ValidateToken(expiredToken);
        Assert.Null(principal);
    }

    [Fact]
    public async Task WebApi_SecurityHeadersMiddleware_ShouldAttachOwaspHeaders()
    {
        var response = await _client.GetAsync("/");

        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").First());

        Assert.True(response.Headers.Contains("X-Frame-Options"));
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").First());

        Assert.True(response.Headers.Contains("Referrer-Policy"));
        Assert.True(response.Headers.Contains("Content-Security-Policy"));
    }

    [Fact]
    public async Task WebApi_ProtectedEndpoint_ShouldReturn401_WhenTokenMissing()
    {
        var response = await _client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WebApi_LoginAndProtectedEndpoint_ShouldSucceedWithValidCredentials()
    {
        // 1. Login as Alice
        var loginRequest = new AuthController.LoginRequest("alice@example.com", "SecurePassword123!");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthController.LoginResponse>();
        Assert.NotNull(authResult);
        Assert.NotEmpty(authResult.AccessToken);

        // 2. Access /api/auth/me using Bearer token
        var meRequest = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        meRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

        var meResponse = await _client.SendAsync(meRequest);
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
    }

    [Fact]
    public async Task WebApi_AdminEndpoint_ShouldReturn403_WhenRoleInsufficient()
    {
        // Alice only has "User" role
        string userToken = _jwtService.GenerateToken("user_101", "alice@example.com", ["User"]);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/admin-only");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task WebApi_AdminEndpoint_ShouldReturn200_WhenUserIsAdmin()
    {
        // Admin has "Admin" role
        string adminToken = _jwtService.GenerateToken("user_999", "admin@example.com", ["Admin", "User"]);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/admin-only");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task WebApi_IdorProtection_ShouldBlockCrossUserDocumentAccess()
    {
        // Alice (user_101) tries to access Bob's document (id: 2, owner: user_102)
        string aliceToken = _jwtService.GenerateToken("user_101", "alice@example.com", ["User"]);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/document/2");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", aliceToken);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
