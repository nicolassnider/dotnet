namespace DotNetMastery.Patterns.Strategy;

/// <summary>
/// Demonstrates the Strategy Pattern.
/// Defines a family of interchangeable algorithms, encapsulating each one,
/// and allowing them to be swapped dynamically at runtime without altering the host class.
/// </summary>
public static class StrategyPatternDemo
{
    public interface IPaymentStrategy
    {
        string PaymentMethodName { get; }
        string Pay(decimal amount);
    }

    public class CreditCardPaymentStrategy : IPaymentStrategy
    {
        public string PaymentMethodName => "CreditCard";

        public string Pay(decimal amount) =>
            $"Paid ${amount:F2} via Credit Card.";
    }

    public class PayPalPaymentStrategy : IPaymentStrategy
    {
        public string PaymentMethodName => "PayPal";

        public string Pay(decimal amount) =>
            $"Paid ${amount:F2} via PayPal.";
    }

    public class BitcoinPaymentStrategy : IPaymentStrategy
    {
        public string PaymentMethodName => "Bitcoin";

        public string Pay(decimal amount) =>
            $"Paid {amount:F4} BTC via Blockchain.";
    }

    public class PaymentProcessor
    {
        private IPaymentStrategy _strategy;

        public PaymentProcessor(IPaymentStrategy initialStrategy)
        {
            _strategy = initialStrategy ?? throw new ArgumentNullException(nameof(initialStrategy));
        }

        public void SetStrategy(IPaymentStrategy strategy)
        {
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        public string CurrentStrategyName => _strategy.PaymentMethodName;

        public string Process(decimal amount)
        {
            return _strategy.Pay(amount);
        }
    }
}
