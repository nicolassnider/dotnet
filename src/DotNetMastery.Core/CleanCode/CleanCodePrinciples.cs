namespace DotNetMastery.Core.CleanCode;

/// <summary>
/// Demonstrates Clean Code principles from the guide:
/// 1. KISS (Keep It Simple, Stupid)
/// 2. Clever vs Maintainable Code
/// 3. Vertical Coding Style
/// </summary>
public static class CleanCodePrinciples
{
    public class UserProfile
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }

    // ==========================================
    // 1. KISS (Keep It Simple, Stupid)
    // ==========================================

    /// <summary>
    /// Over-engineered and overly verbose check.
    /// </summary>
    public static bool CanAccessSystemOverEngineered(UserProfile? user)
    {
        // ❌ Verbose check with redundant comparisons
        if (user != null && user.IsActive == true && user.IsDeleted == false)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Simple, idiomatic, and readable check adhering to KISS.
    /// </summary>
    public static bool CanAccessSystemKiss(UserProfile? user)
    {
        // ✅ Idiomatic null-conditional check
        return user?.IsActive is true && !user.IsDeleted;
    }

    // ==========================================
    // 2. Clever vs Maintainable Code
    // ==========================================

    /// <summary>
    /// "Clever" one-liner style that is hard to read, debug, and maintain.
    /// </summary>
    public static List<string> ParseErrorsClever(string rawLogData)
    {
        // ❌ Cryptic variable names, raw string splitting, poor readability
        var res = new List<string>();
        var s = rawLogData.Split(';', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i].Contains("ERR:") && !s[i].Contains("IGNORED"))
            {
                res.Add(s[i].Replace("ERR:", "").Trim());
            }
        }
        return res;
    }

    /// <summary>
    /// Maintainable, self-documenting implementation with dedicated domain methods.
    /// </summary>
    public static List<string> ParseErrorsMaintainable(string rawLogData)
    {
        // ✅ Expressive domain methods, clear intention
        var entries = ExtractLogEntries(rawLogData);
        var errors = new List<string>();

        foreach (var entry in entries)
        {
            if (IsSignificantError(entry))
            {
                errors.Add(ExtractErrorMessage(entry));
            }
        }

        return errors;
    }

    private static string[] ExtractLogEntries(string rawLogData) =>
        rawLogData.Split(';', StringSplitOptions.RemoveEmptyEntries);

    private static bool IsSignificantError(string entry) =>
        entry.StartsWith("ERR:", StringComparison.OrdinalIgnoreCase) &&
        !entry.Contains("IGNORED", StringComparison.OrdinalIgnoreCase);

    private static string ExtractErrorMessage(string entry) =>
        entry["ERR:".Length..].Trim();

    // ==========================================
    // 3. Vertical Coding Style
    // ==========================================

    /// <summary>
    /// Demonstrates vertical coding style for fluent LINQ chains, improving readability in diffs and reviews.
    /// </summary>
    public static List<string> GetActiveAdultEmailsVertical(IEnumerable<UserProfile> users)
    {
        // ✅ Vertical style: each transformation on its own line
        return users
            .Where(u => u.IsActive && !u.IsDeleted)
            .Where(u => u.Age >= 18)
            .OrderBy(u => u.Username)
            .Select(u => u.Email)
            .ToList();
    }
}
