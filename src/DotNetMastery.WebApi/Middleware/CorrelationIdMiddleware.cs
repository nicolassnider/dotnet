namespace DotNetMastery.WebApi.Middleware;

/// <summary>
/// Middleware that inspects incoming requests for an X-Correlation-ID header.
/// If present, reuses it; if not, generates a new GUID.
/// Propagates the ID to the response headers, HttpContext.Items, and sets up
/// a structured logging scope so every log line emitted during the request lifecycle includes it.
/// </summary>
public class CorrelationIdMiddleware
{
    public const string CorrelationIdHeader = "X-Correlation-ID";
    public const string CorrelationIdItemKey = "CorrelationId";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Extract existing correlation ID or generate new
        string correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
                               ?? Guid.NewGuid().ToString("N");

        // 2. Store in HttpContext.Items for downstream code access
        context.Items[CorrelationIdItemKey] = correlationId;

        // 3. Attach to response headers
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
            {
                context.Response.Headers.Append(CorrelationIdHeader, correlationId);
            }
            return Task.CompletedTask;
        });

        // 4. Wrap pipeline execution in structured logger scope
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            [CorrelationIdItemKey] = correlationId
        }))
        {
            _logger.LogInformation("Request processing started with Correlation ID: {CorrelationId}", correlationId);
            await _next(context);
            _logger.LogInformation("Request processing completed for Correlation ID: {CorrelationId}", correlationId);
        }
    }
}
