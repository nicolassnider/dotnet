namespace DotNetMastery.Core.Fundamentals;

/// <summary>
/// Demonstrates Boxing (value type -> object on heap) and Unboxing (object -> value type).
/// Highlights heap allocation overhead and type mismatch risks (InvalidCastException).
/// </summary>
public static class BoxingUnboxingDemo
{
    /// <summary>
    /// Performs boxing: converts a value type (int on stack) into an object reference on the heap.
    /// </summary>
    public static object BoxInteger(int value)
    {
        // Allocates an object on the managed heap and copies 'value' into it.
        object boxed = value;
        return boxed;
    }

    /// <summary>
    /// Performs unboxing: extracts the value type out of the boxed heap object.
    /// Requires exact type match.
    /// </summary>
    public static int UnboxInteger(object boxed)
    {
        // Unboxes and copies the value back to the stack.
        int unboxed = (int)boxed;
        return unboxed;
    }

    /// <summary>
    /// Demonstrates that unboxing to an incompatible type throws InvalidCastException at runtime.
    /// Even though an int can implicitly convert to double, unboxing requires an exact type match first.
    /// </summary>
    public static bool TryInvalidUnboxing(object boxedInt)
    {
        try
        {
            // Throws InvalidCastException because boxedInt contains an int, not a double
            _ = (double)boxedInt;
            return false;
        }
        catch (InvalidCastException)
        {
            return true;
        }
    }

    /// <summary>
    /// Demonstrates generic collections (zero boxing) vs non-generic ArrayList (boxes value types).
    /// </summary>
    public static int SumWithGenerics(ReadOnlySpan<int> numbers)
    {
        int sum = 0;
        foreach (int n in numbers)
        {
            sum += n; // No heap allocation, no boxing
        }
        return sum;
    }
}
