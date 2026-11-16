using System.Net;
using System.Net.Http.Json;
using DotNetMastery.WebApi.Middleware;
using DotNetMastery.WebApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DotNetMastery.Tests;

public class WebApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public WebApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RootEndpoint_ShouldReturnHealthyStatus()
    {
        var response = await _client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains(".NET Mastery API", content);
    }

    [Fact]
    public async Task CorrelationIdMiddleware_ShouldGenerateNewId_WhenHeaderMissing()
    {
        var response = await _client.GetAsync("/");

        Assert.True(response.Headers.Contains(CorrelationIdMiddleware.CorrelationIdHeader));
        string? correlationId = response.Headers.GetValues(CorrelationIdMiddleware.CorrelationIdHeader).FirstOrDefault();
        Assert.False(string.IsNullOrWhiteSpace(correlationId));
    }

    [Fact]
    public async Task CorrelationIdMiddleware_ShouldEchoProvidedHeader()
    {
        string customId = "trace-request-custom-98765";
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add(CorrelationIdMiddleware.CorrelationIdHeader, customId);

        var response = await _client.SendAsync(request);

        Assert.True(response.Headers.Contains(CorrelationIdMiddleware.CorrelationIdHeader));
        string? echoedId = response.Headers.GetValues(CorrelationIdMiddleware.CorrelationIdHeader).FirstOrDefault();
        Assert.Equal(customId, echoedId);
    }

    [Fact]
    public async Task DiLifecyclesEndpoint_ShouldReflectTransientScopedSingletonSemantics()
    {
        var response = await _client.GetAsync("/api/dilifecycle");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<DiLifecycleResponse>();
        Assert.NotNull(json);

        // Transient instances must be distinct
        Assert.False(json.Transient.AreIdentical);
        Assert.NotEqual(json.Transient.FirstInstanceId, json.Transient.SecondInstanceId);

        // Scoped instances must be identical within the same HTTP request
        Assert.True(json.Scoped.AreIdentical);
        Assert.Equal(json.Scoped.FirstInstanceId, json.Scoped.SecondInstanceId);

        // Singleton instances must be identical
        Assert.True(json.Singleton.AreIdentical);
        Assert.Equal(json.Singleton.FirstInstanceId, json.Singleton.SecondInstanceId);
    }

    [Fact]
    public async Task UsersController_Create_WithInvalidData_ShouldReturn400BadRequest()
    {
        var invalidRequest = new CreateUserRequest
        {
            Name = "A", // Min length is 2
            Email = "not-an-email",
            Age = 12 // Min age is 18
        };

        var response = await _client.PostAsJsonAsync("/api/users", invalidRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UsersController_Create_WithValidData_ShouldReturn201Created()
    {
        var validRequest = new CreateUserRequest
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            Age = 30
        };

        var response = await _client.PostAsJsonAsync("/api/users", validRequest);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(created);
        Assert.Equal("John Doe", created.Name);
        Assert.True(created.Id > 0);
    }

    [Fact]
    public async Task RateLimiting_FixedWindowEndpoint_ShouldAllowNormalRequests()
    {
        var response = await _client.GetAsync("/api/rate-limited/fixed");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Helper DTO for deserialization in tests
    private record DiLifecycleItem(Guid FirstInstanceId, Guid SecondInstanceId, bool AreIdentical);
    private record DiLifecycleResponse(DiLifecycleItem Transient, DiLifecycleItem Scoped, DiLifecycleItem Singleton);
}
