namespace DotNetMastery.Performance.Span;

/// <summary>
/// Demonstrates high-performance zero-allocation parsing with Span&lt;T&gt; and AsSpan()
/// compared to Substring().
/// Substring creates new string objects on the managed heap on every call.
/// AsSpan provides a window over existing memory without copying or allocating.
/// </summary>
public static class SpanParsingDemo
{
    public record ParsedLogEntry(int Year, int Month, int Day, string Status);

    /// <summary>
    /// ❌ Legacy Substring approach: allocates 4 new heap strings per parsed line.
    /// In high-throughput scenarios, this causes significant GC pressure.
    /// Format: "LOG_YYYY-MM-DD_STATUS" (e.g., "LOG_2026-05-27_ACTIVE")
    /// </summary>
    public static ParsedLogEntry ParseWithSubstring(string rawLog)
    {
        // ❌ Creates multiple heap strings
        string yearStr = rawLog.Substring(4, 4);   // Allocates "2026"
        string monthStr = rawLog.Substring(9, 2);  // Allocates "05"
        string dayStr = rawLog.Substring(12, 2);   // Allocates "27"
        string statusStr = rawLog.Substring(15);   // Allocates "ACTIVE"

        int year = int.Parse(yearStr);
        int month = int.Parse(monthStr);
        int day = int.Parse(dayStr);

        return new ParsedLogEntry(year, month, day, statusStr);
    }

    /// <summary>
    /// ✅ Modern AsSpan approach: zero string allocations for numeric parsing.
    /// Slices memory views with ReadOnlySpan&lt;char&gt;.
    /// </summary>
    public static ParsedLogEntry ParseWithSpan(string rawLog)
    {
        // ✅ ReadOnlySpan<char> creates a view over existing string characters on the stack
        ReadOnlySpan<char> span = rawLog.AsSpan();

        ReadOnlySpan<char> yearSpan = span.Slice(4, 4);
        ReadOnlySpan<char> monthSpan = span.Slice(9, 2);
        ReadOnlySpan<char> daySpan = span.Slice(12, 2);
        ReadOnlySpan<char> statusSpan = span.Slice(15);

        // int.Parse directly accepts ReadOnlySpan<char> without intermediate string allocation
        int year = int.Parse(yearSpan);
        int month = int.Parse(monthSpan);
        int day = int.Parse(daySpan);

        // Materialize string only when long-term retention in entity is required
        string status = statusSpan.ToString();

        return new ParsedLogEntry(year, month, day, status);
    }

    /// <summary>
    /// Validates status without ANY string allocation using SequenceEqual on ReadOnlySpan.
    /// </summary>
    public static bool IsActiveStatusWithSpan(string rawLog)
    {
        ReadOnlySpan<char> span = rawLog.AsSpan();
        if (span.Length < 15) return false;

        ReadOnlySpan<char> statusSpan = span.Slice(15);
        return statusSpan.SequenceEqual("ACTIVE"); // Zero heap allocations
    }
}
