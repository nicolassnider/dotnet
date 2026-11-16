namespace DotNetMastery.WebApi.Security;

/// <summary>
/// Middleware enforcing OWASP recommended Defense-in-Depth HTTP Security Headers:
/// - X-Content-Type-Options: Prevents MIME-type sniffing
/// - X-Frame-Options: Prevents Clickjacking attacks
/// - Referrer-Policy: Controls referrer leakage across origins
/// - Content-Security-Policy (CSP): Restricts executable scripts and resources
/// - Permissions-Policy: Restricts browser device APIs (camera, geolocation)
/// - Strict-Transport-Security (HSTS): Enforces HTTPS connections
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;

            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["Content-Security-Policy"] = "default-src 'self'; frame-ancestors 'none';";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

            // Remove server fingerprinting headers
            headers.Remove("Server");
            headers.Remove("X-Powered-By");

            return Task.CompletedTask;
        });

        await _next(context);
    }
}
