namespace DotNetMastery.Core.Delegates;

/// <summary>
/// Demonstrates C# Delegates: Action, Func, Predicate, Multicast delegates, and Delegates vs Events.
/// A delegate is a type-safe function pointer.
/// </summary>
public static class DelegatesAndLambdasDemo
{
    // ==========================================
    // 1. Action, Func, and Predicate
    // ==========================================

    /// <summary>
    /// Action: encapsulates a method that takes 0 to 16 parameters and returns void.
    /// </summary>
    public static void ExecuteAction(string message, Action<string> action)
    {
        action(message);
    }

    /// <summary>
    /// Func: encapsulates a method that takes 0 to 16 parameters and returns a value of type TResult.
    /// </summary>
    public static TResult ExecuteFunc<T1, T2, TResult>(T1 arg1, T2 arg2, Func<T1, T2, TResult> func)
    {
        return func(arg1, arg2);
    }

    /// <summary>
    /// Predicate: encapsulates a method that takes 1 parameter and returns a boolean condition result.
    /// Equivalent to Func&lt;T, bool&gt;.
    /// </summary>
    public static bool EvaluatePredicate<T>(T item, Predicate<T> predicate)
    {
        return predicate(item);
    }

    // ==========================================
    // 2. Multicast Delegates
    // ==========================================

    /// <summary>
    /// Multicast delegates hold references to multiple methods using the += and -= operators.
    /// When invoked, they execute the invocation list in order.
    /// </summary>
    public static List<string> RunMulticastDelegate(string payload)
    {
        var logs = new List<string>();

        Action<string>? logger = msg => logs.Add($"Handler 1: {msg}");
        logger += msg => logs.Add($"Handler 2: {msg}");

        logger?.Invoke(payload);

        // Demonstrating -= to detach a subscriber
        Action<string> handlerToRemove = msg => logs.Add("Handler 3: Temporary");
        logger += handlerToRemove;
        logger -= handlerToRemove;

        return logs;
    }

    // ==========================================
    // 3. Delegate vs Event Encapsulation
    // ==========================================

    public delegate void NotificationHandler(string message);

    /// <summary>
    /// Unsafe class: exposes raw delegate fields that can be completely overwritten or wiped from the outside.
    /// </summary>
    public class UnsafeNotifier
    {
        // ❌ Vulnerable: external code can do 'notifier.OnNotify = null;' and wipe all other listeners!
        public NotificationHandler? OnNotifyDelegate;

        public void Notify(string message) => OnNotifyDelegate?.Invoke(message);
    }

    /// <summary>
    /// Safe class: uses the 'event' keyword to encapsulate the delegate.
    /// External code can only subscribe (+=) or unsubscribe (-=). Cannot be reset to null from outside.
    /// </summary>
    public class SafeNotifier
    {
        // ✅ Encapsulated: external code can only use += or -=
        public event NotificationHandler? OnNotify;

        public void Notify(string message) => OnNotify?.Invoke(message);
    }
}
