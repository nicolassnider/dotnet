using System.Net;

namespace DotNetMastery.Core.Security;

/// <summary>
/// Practical demonstrations and utility guards mitigating OWASP Top 10 vulnerabilities in .NET applications.
/// </summary>
public static class OwaspMitigations
{
    // ==========================================
    // A01: Broken Access Control (BOLA / IDOR)
    // ==========================================

    public record DocumentRecord(int Id, string OwnerUserId, string Content);

    /// <summary>
    /// Mitigates Insecure Direct Object References (IDOR).
    /// Always verifies that the authenticated user either owns the resource or has an elevated admin role.
    /// </summary>
    public static bool CanAccessDocument(DocumentRecord document, string currentUserId, IEnumerable<string> currentUserRoles)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(currentUserId);

        // Admins have broad oversight
        if (currentUserRoles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        // Standard users can ONLY access their own records
        return string.Equals(document.OwnerUserId, currentUserId, StringComparison.Ordinal);
    }

    // ==========================================
    // A03: Injection (SQL / Command)
    // ==========================================

    /// <summary>
    /// Demonstrates safe parameterization.
    /// Raw string concatenation in SQL queries invites SQL Injection (e.g., "' OR '1'='1").
    /// Parameterized queries separate query structure from untrusted user input.
    /// </summary>
    public static (string safeCommandText, Dictionary<string, object> parameters) BuildSafeSearchQuery(string userInput)
    {
        // Safe parameterized SQL pattern (used by EF Core FormattableString and Dapper)
        const string query = "SELECT Id, Username, Email FROM Users WHERE Username = @username AND IsActive = 1";
        var parameters = new Dictionary<string, object>
        {
            ["@username"] = userInput ?? string.Empty
        };

        return (query, parameters);
    }

    // ==========================================
    // A10: Server-Side Request Forgery (SSRF)
    // ==========================================

    /// <summary>
    /// Validates whether a target URL requested by an untrusted user is safe for outbound HttpClient requests.
    /// Blocks loopback (127.0.0.1, localhost), link-local cloud metadata endpoints (169.254.169.254),
    /// and private local networks (10.0.0.0/8, 172.16.0.0/12, 192.168.0.0/16).
    /// </summary>
    public static bool IsSafeOutboundUrl(string rawUrl)
    {
        if (string.IsNullOrWhiteSpace(rawUrl))
        {
            return false;
        }

        if (!Uri.TryCreate(rawUrl, UriKind.Absolute, out Uri? uri))
        {
            return false;
        }

        // Only allow HTTPS
        if (uri.Scheme != Uri.UriSchemeHttps)
        {
            return false;
        }

        string host = uri.DnsSafeHost.ToLowerInvariant();

        // Block localhost and standard loopback hostnames
        if (host is "localhost" or "127.0.0.1" or "::1")
        {
            return false;
        }

        // If IP address, check for private/link-local/metadata ranges
        if (IPAddress.TryParse(host, out IPAddress? ip))
        {
            return !IsPrivateOrReservedIp(ip);
        }

        return true;
    }

    private static bool IsPrivateOrReservedIp(IPAddress ip)
    {
        if (IPAddress.IsLoopback(ip))
        {
            return true;
        }

        byte[] bytes = ip.GetAddressBytes();

        // IPv4 checks
        if (bytes.Length == 4)
        {
            // 10.0.0.0/8
            if (bytes[0] == 10) return true;

            // 172.16.0.0/12
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return true;

            // 192.168.0.0/16
            if (bytes[0] == 192 && bytes[1] == 168) return true;

            // 169.254.0.0/16 (Link-Local & Cloud Metadata e.g. AWS/Azure 169.254.169.254)
            if (bytes[0] == 169 && bytes[1] == 254) return true;
        }

        return false;
    }
}
