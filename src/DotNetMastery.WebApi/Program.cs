using System.Threading.RateLimiting;
using DotNetMastery.WebApi.Middleware;
using DotNetMastery.WebApi.Services;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Service Registrations (DI Container)
// ==========================================

// Add Controllers & OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register DI Lifecycles Demonstration
builder.Services.AddTransient<ITransientOperation, OperationService>();
builder.Services.AddScoped<IScopedOperation, OperationService>();
builder.Services.AddSingleton<ISingletonOperation, OperationService>();

// CORS Configuration: Insecure vs Secure Policies
builder.Services.AddCors(options =>
{
    // ❌ Insecure policy: allows any origin, method, header
    options.AddPolicy("InsecurePolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });

    // ✅ Secure policy: explicitly restricted origins, methods, and headers
    options.AddPolicy("SecurePolicy", policy =>
    {
        policy.WithOrigins("https://myapp.com", "https://admin.myapp.com", "http://localhost:3000")
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .WithHeaders("Content-Type", "Authorization", CorrelationIdMiddleware.CorrelationIdHeader)
              .AllowCredentials();
    });
});

// Native Rate Limiting (.NET 7+)
builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Fixed Window Limiter: permits 10 requests per 10-second window
    rateLimiterOptions.AddFixedWindowLimiter("fixed", options =>
    {
        options.PermitLimit = 10;
        options.Window = TimeSpan.FromSeconds(10);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    });

    // Sliding Window Limiter: 10 requests across 10 seconds divided into 2 segments
    rateLimiterOptions.AddSlidingWindowLimiter("sliding", options =>
    {
        options.PermitLimit = 10;
        options.Window = TimeSpan.FromSeconds(10);
        options.SegmentsPerWindow = 2;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    });
});

var app = builder.Build();

// ==========================================
// 2. HTTP Request Pipeline (Middleware Ordering)
// ==========================================

// 1. Exception handling & Developer tooling
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 2. HTTPS Redirection
app.UseHttpsRedirection();

// 3. Routing
app.UseRouting();

// 4. CORS
app.UseCors("SecurePolicy");

// 5. Rate Limiting
app.UseRateLimiter();

// 6. Custom Correlation ID Middleware
app.UseMiddleware<CorrelationIdMiddleware>();

// 7. Map Endpoints
app.MapControllers();

// Rate-limited demo endpoints
app.MapGet("/api/rate-limited/fixed", () => Results.Ok(new { message = "Fixed window rate-limited endpoint reached successfully." }))
   .RequireRateLimiting("fixed");

app.MapGet("/api/rate-limited/sliding", () => Results.Ok(new { message = "Sliding window rate-limited endpoint reached successfully." }))
   .RequireRateLimiting("sliding");

app.MapGet("/", () => Results.Ok(new
{
    title = ".NET Mastery API",
    status = "Healthy",
    features = new[] { "CorrelationId", "RateLimiting", "DILifecycles", "ModelValidation", "SecureCORS" }
}));

app.Run();

// Visible for WebApplicationFactory in integration tests
public partial class Program { }
