using DotNetMastery.Patterns.DependencyInjection;
using DotNetMastery.Patterns.FactoryMethod;
using DotNetMastery.Patterns.Repository;
using DotNetMastery.Patterns.Singleton;
using DotNetMastery.Patterns.Strategy;
using Microsoft.EntityFrameworkCore;

namespace DotNetMastery.Tests;

public class PatternsTests
{
    [Fact]
    public void DependencyInjection_DecoupledService_ShouldQueryUserSuccessfully()
    {
        var mockDb = new DependencyInjectionDemo.SqlDatabaseConnection();
        var service = new DependencyInjectionDemo.DecoupledUserService(mockDb);

        var user = service.GetUser(1);

        Assert.NotNull(user);
        Assert.Equal("Alice", user.Name);
    }

    [Fact]
    public void FactoryMethod_ShouldCreateRequestedLoggerType()
    {
        var fileLogger = FactoryMethodDemo.LoggerFactory.CreateLogger("file");
        var dbLogger = FactoryMethodDemo.LoggerFactory.CreateLogger("db");
        var consoleLogger = FactoryMethodDemo.LoggerFactory.CreateLogger("unknown");

        Assert.Equal("File", fileLogger.Destination);
        Assert.Equal("Database", dbLogger.Destination);
        Assert.Equal("Console", consoleLogger.Destination);
    }

    [Fact]
    public async Task RepositoryPattern_CrudOperations_ShouldPersistAndRetrieve()
    {
        var options = new DbContextOptionsBuilder<RepositoryPatternDemo.AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new RepositoryPatternDemo.AppDbContext(options);
        var repository = new RepositoryPatternDemo.UserRepository(context);
        var service = new RepositoryPatternDemo.UserService(repository);

        var newUser = new RepositoryPatternDemo.UserEntity { Name = "Eve", Email = "eve@example.com" };
        await service.RegisterUserAsync(newUser);

        var retrieved = await service.FetchUserAsync(newUser.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Eve", retrieved.Name);
    }

    [Fact]
    public void StrategyPattern_ShouldAllowDynamicAlgorithmSwapping()
    {
        var processor = new StrategyPatternDemo.PaymentProcessor(new StrategyPatternDemo.CreditCardPaymentStrategy());
        string cardResult = processor.Process(50m);
        Assert.Contains("via Credit Card", cardResult);

        processor.SetStrategy(new StrategyPatternDemo.PayPalPaymentStrategy());
        string paypalResult = processor.Process(50m);
        Assert.Contains("via PayPal", paypalResult);
    }

    [Fact]
    public void SingletonPattern_LazyInstance_ShouldBeIdenticalAcrossAccesses()
    {
        var first = SingletonPatternDemo.LazyAppSettings.Instance;
        var second = SingletonPatternDemo.LazyAppSettings.Instance;

        Assert.Same(first, second);
        Assert.Equal("2.0", first.ApiVersion);
    }
}
