namespace DotNetMastery.Core.Fundamentals;

/// <summary>
/// Demonstrates Stack vs Heap allocation and Value vs Reference types semantics.
/// Stack: Fast, LIFO (Last-In-First-Out) memory for local variables and value types. Automatically reclaimed when leaving scope.
/// Heap: Shared managed memory where reference types and boxed objects live, managed by the Garbage Collector (GC).
/// </summary>
public static class StackVsHeapDemo
{
    public struct PointValue
    {
        public int X { get; set; }
        public int Y { get; set; }

        public PointValue(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class PointReference
    {
        public int X { get; set; }
        public int Y { get; set; }

        public PointReference(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    /// <summary>
    /// Demonstrates that assigning a value type copies the entire state by value.
    /// Modifying the copy leaves the original unchanged.
    /// </summary>
    public static (PointValue original, PointValue modifiedCopy) DemonstrateValueTypeCopy()
    {
        var original = new PointValue(10, 20);
        var copy = original; // Full value copy on the stack
        copy.X = 99;

        return (original, copy);
    }

    /// <summary>
    /// Demonstrates that assigning a reference type copies only the memory reference (pointer).
    /// Both variables point to the same object instance on the managed heap.
    /// </summary>
    public static (PointReference original, PointReference sharedReference) DemonstrateReferenceTypeCopy()
    {
        var original = new PointReference(10, 20);
        var shared = original; // Copies only the reference pointer
        shared.X = 99; // Mutates the underlying object on the heap

        return (original, shared);
    }
}
