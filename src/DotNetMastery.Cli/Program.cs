using System.Diagnostics;
using DotNetMastery.Core.CleanCode;
using DotNetMastery.Core.Delegates;
using DotNetMastery.Core.Fundamentals;
using DotNetMastery.DataAndAsync.Async;
using DotNetMastery.DataAndAsync.Collections;
using DotNetMastery.Patterns.DependencyInjection;
using DotNetMastery.Patterns.FactoryMethod;
using DotNetMastery.Patterns.Repository;
using DotNetMastery.Patterns.Singleton;
using DotNetMastery.Patterns.Strategy;
using DotNetMastery.Performance.Memory;
using DotNetMastery.Performance.Span;
using Microsoft.EntityFrameworkCore;
using DotNetMastery.Cloud.Abstractions;
using DotNetMastery.Cloud.Factory;
using Microsoft.Extensions.Caching.Memory;

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("==================================================================");
Console.WriteLine("        .NET Mastery — Full Interactive Showcase Runner            ");
Console.WriteLine("==================================================================");
Console.ResetColor();

if (args.Length > 0 && args[0].Equals("--all", StringComparison.OrdinalIgnoreCase))
{
    await RunAllDemosAsync();
    return;
}

while (true)
{
    Console.WriteLine();
    Console.WriteLine("Select a topic to demonstrate:");
    Console.WriteLine("  1. Core Fundamentals (Stack vs Heap, Boxing, Keywords, Delegates)");
    Console.WriteLine("  2. Design Patterns (DI, Factory, Repository, Strategy, Singleton)");
    Console.WriteLine("  3. Data & Async (Collections, LINQ, 11 Async Rules, ValueTask)");
    Console.WriteLine("  4. Performance & Memory (Memory Leaks, ArrayPool, Span vs Substring)");
    Console.WriteLine("  5. Cloud Engineering (Azure, AWS, GCP basics & multi-cloud abstractions)");
    Console.WriteLine("  6. Run ALL Demonstrations");
    Console.WriteLine("  0. Exit");
    Console.Write("\nEnter choice [0-6]: ");

    var input = Console.ReadLine()?.Trim();
    Console.WriteLine();

    switch (input)
    {
        case "1":
            RunCoreFundamentalsDemo();
            break;
        case "2":
            await RunDesignPatternsDemoAsync();
            break;
        case "3":
            await RunDataAndAsyncDemoAsync();
            break;
        case "4":
            await RunPerformanceAndMemoryDemoAsync();
            break;
        case "5":
            await RunCloudEngineeringDemoAsync();
            break;
        case "6":
            await RunAllDemosAsync();
            break;
        case "0" or "exit" or "q":
            Console.WriteLine("Exiting. Keep mastering .NET!");
            return;
        default:
            Console.WriteLine("Invalid option, please choose between 0 and 6.");
            break;
    }
}

async Task RunAllDemosAsync()
{
    RunCoreFundamentalsDemo();
    await RunDesignPatternsDemoAsync();
    await RunDataAndAsyncDemoAsync();
    await RunPerformanceAndMemoryDemoAsync();
    await RunCloudEngineeringDemoAsync();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("\n[✔] All demonstrations completed successfully!");
    Console.ResetColor();
}

void PrintSectionHeader(string title)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"\n--- [ {title} ] ---");
    Console.ResetColor();
}

void RunCoreFundamentalsDemo()
{
    PrintSectionHeader("Core Fundamentals & Keywords");

    // Stack vs Heap
    var (valOrig, valCopy) = StackVsHeapDemo.DemonstrateValueTypeCopy();
    Console.WriteLine($"[Value Type (Stack)] Original X={valOrig.X}, Copy X={valCopy.X} (Copy did NOT affect original)");

    var (refOrig, refShared) = StackVsHeapDemo.DemonstrateReferenceTypeCopy();
    Console.WriteLine($"[Reference Type (Heap)] Original X={refOrig.X}, Shared X={refShared.X} (Both reference same object)");

    // Boxing & Unboxing
    int num = 42;
    object boxed = BoxingUnboxingDemo.BoxInteger(num);
    int unboxed = BoxingUnboxingDemo.UnboxInteger(boxed);
    bool failedCast = BoxingUnboxingDemo.TryInvalidUnboxing(boxed);
    Console.WriteLine($"[Boxing/Unboxing] Boxed {num} -> Heap object -> Unboxed back: {unboxed}. Incompatible cast threw: {failedCast}");

    // Keywords: ref & out
    int val = 10;
    KeywordsDemo.Increment(ref val);
    KeywordsDemo.TryParseInteger("123", out int parsed);
    KeywordsDemo.GetUserInfo(1, out string userName, out int userAge);
    Console.WriteLine($"[ref/out] ref increment: {val}, out parsed: {parsed}, out user info: {userName}, {userAge}");

    // yield lazy evaluation
    var eagerList = KeywordsDemo.GenerateNumbersEager(5);
    var lazyNumbers = KeywordsDemo.GenerateNumbersLazy(5).Take(3).ToList();
    Console.WriteLine($"[yield] Eager count: {eagerList.Count}, Lazy requested 3: [{string.Join(", ", lazyNumbers)}]");

    // Equals vs ==
    var (opEq, eqMethod, recEq) = KeywordsDemo.CompareEqualities();
    Console.WriteLine($"[Equality] Class ==: {opEq}, Class .Equals(): {eqMethod}, Record ==: {recEq}");

    // enum & nameof
    var status = KeywordsDemo.UserStatus.Active;
    Console.WriteLine($"[enum/nameof] Fast zero-alloc status name: {KeywordsDemo.GetStatusNameFast(status)}");

    // Delegates & Events
    var logs = DelegatesAndLambdasDemo.RunMulticastDelegate("ServerStarted");
    Console.WriteLine($"[Delegates] Multicast handlers triggered:\n  - {string.Join("\n  - ", logs)}");

    // Clean Code
    var errors = CleanCodePrinciples.ParseErrorsMaintainable("ERR:ConnectionTimeout;INFO:Heartbeat;ERR:DatabaseDown;IGNORED:Skipped");
    Console.WriteLine($"[Clean Code] Clean log parsed errors: [{string.Join(", ", errors)}]");
}

async Task RunDesignPatternsDemoAsync()
{
    PrintSectionHeader("Top 5 Design Patterns");

    // 1. Dependency Injection
    var db = new DependencyInjectionDemo.SqlDatabaseConnection();
    var userService = new DependencyInjectionDemo.DecoupledUserService(db);
    var user = userService.GetUser(1);
    Console.WriteLine($"[1. DI] Decoupled service queried user: {user?.Name} ({user?.Email})");

    // 2. Factory Method
    var fileLogger = FactoryMethodDemo.LoggerFactory.CreateLogger("file");
    var dbLogger = FactoryMethodDemo.LoggerFactory.CreateLogger("db");
    Console.WriteLine($"[2. Factory] Created: {fileLogger.Log("Writing to disk")}");
    Console.WriteLine($"[2. Factory] Created: {dbLogger.Log("Writing to database")}");

    // 3. Repository Pattern
    var options = new DbContextOptionsBuilder<RepositoryPatternDemo.AppDbContext>()
        .UseInMemoryDatabase("PatternsDemoDb")
        .Options;
    using var context = new RepositoryPatternDemo.AppDbContext(options);
    var repo = new RepositoryPatternDemo.UserRepository(context);
    var repoService = new RepositoryPatternDemo.UserService(repo);
    await repoService.RegisterUserAsync(new RepositoryPatternDemo.UserEntity { Name = "Charlie", Email = "charlie@demo.com" });
    var found = await repoService.FetchUserAsync(1);
    Console.WriteLine($"[3. Repository] Repository pattern retrieved user: {found?.Name}");

    // 4. Strategy Pattern
    var processor = new StrategyPatternDemo.PaymentProcessor(new StrategyPatternDemo.CreditCardPaymentStrategy());
    Console.WriteLine($"[4. Strategy] {processor.Process(150.00m)}");
    processor.SetStrategy(new StrategyPatternDemo.BitcoinPaymentStrategy());
    Console.WriteLine($"[4. Strategy] Swapped to {processor.CurrentStrategyName}: {processor.Process(0.0035m)}");

    // 5. Singleton Pattern
    var lazySingleton1 = SingletonPatternDemo.LazyAppSettings.Instance;
    var lazySingleton2 = SingletonPatternDemo.LazyAppSettings.Instance;
    Console.WriteLine($"[5. Singleton] Lazy instances identical: {ReferenceEquals(lazySingleton1, lazySingleton2)}, Version={lazySingleton1.ApiVersion}");
}

async Task RunDataAndAsyncDemoAsync()
{
    PrintSectionHeader("Data & Async Programming");

    // LINQ: OrderBy vs ThenBy
    var sampleProducts = new List<CollectionsAndLinqDemo.Product>
    {
        new(1, "Electronics", "Laptop", 1200m, 5),
        new(2, "Electronics", "Mouse", 25m, 50),
        new(3, "Books", "C# in Depth", 45m, 20),
        new(4, "Books", "Clean Code", 40m, 15),
    };

    var correctlyOrdered = CollectionsAndLinqDemo.OrderByCorrect(sampleProducts);
    Console.WriteLine("[LINQ OrderBy + ThenBy] Correct composite sort:");
    foreach (var p in correctlyOrdered)
    {
        Console.WriteLine($"   {p.Category} - {p.Price:C} - {p.Name}");
    }

    // LINQ: CountBy (.NET 9+)
    var categoryCounts = CollectionsAndLinqDemo.CountByCategoryModern(sampleProducts);
    Console.WriteLine("[LINQ CountBy (.NET 9+)] Counts: " + string.Join(", ", categoryCounts.Select(c => $"{c.Key}: {c.Value}")));

    // Async 11 Rules
    string data = await AsyncRulesDemo.Rule1_FetchDataNonBlockingAsync();
    var (first, second) = await AsyncRulesDemo.Rule3_RunInParallelAsync();
    bool handledEx = await AsyncRulesDemo.Rule7_HandleTaskExceptionAsync();
    Console.WriteLine($"[Async Rules] Non-blocking: '{data}', Parallel results: ({first}, '{second}'), Handled exception: {handledEx}");

    // Task.WhenEach (.NET 9+)
    var streamTasks = new[]
    {
        Task.Run(async () => { await Task.Delay(20); return "Task C (Fast)"; }),
        Task.Run(async () => { await Task.Delay(40); return "Task B (Medium)"; }),
        Task.Run(async () => { await Task.Delay(60); return "Task A (Slow)"; })
    };
    var streamed = await AsyncRulesDemo.Rule11_StreamWithWhenEachAsync(streamTasks);
    Console.WriteLine($"[Task.WhenEach (.NET 9+)] Streamed completion order: [{string.Join(" -> ", streamed)}]");

    // Task vs ValueTask
    var valService = new TaskVsValueTaskDemo.UserService();
    var hit1 = await valService.GetUserValueTaskAsync(1); // Cache hit (sync)
    var hit2 = await valService.GetUserValueTaskAsync(1); // Cache hit (sync)
    Console.WriteLine($"[ValueTask] Cached retrieval zero-allocation hit: {hit1?.Username}. Network calls: {valService.NetworkCallCount}");
}

async Task RunPerformanceAndMemoryDemoAsync()
{
    PrintSectionHeader("Performance & Memory");

    // Event Subscription Leak Fix
    var broadcaster = new MemoryLeaksDemo.EventBroadcaster();
    using (var cleanSub = new MemoryLeaksDemo.CleanSubscriber(broadcaster))
    {
        broadcaster.Publish("Hello");
        Console.WriteLine($"[Memory Leak Fix] Clean subscriber received: {cleanSub.ReceivedCount}");
    }
    broadcaster.Publish("SecondMessage"); // Will not leak or be handled by disposed subscriber
    Console.WriteLine("[Memory Leak Fix] Subscriber successfully detached after Dispose()");

    // Bounded Smart Cache
    var boundedCache = new MemoryLeaksDemo.BoundedSmartCache<string, string>(maxCapacity: 2);
    boundedCache.Set("k1", "v1");
    boundedCache.Set("k2", "v2");
    boundedCache.Set("k3", "v3"); // Evicts k1
    bool hasK1 = boundedCache.TryGet("k1", out _);
    bool hasK3 = boundedCache.TryGet("k3", out var v3);
    Console.WriteLine($"[Bounded Cache] Capacity limit=2. Oldest 'k1' evicted: {!hasK1}, 'k3' exists: {hasK3} (val: {v3})");

    // ArrayPool for LOH (>85KB)
    int bufferSize = MemoryLeaksDemo.ProcessLargeBufferWithPool(100_000);
    Console.WriteLine($"[ArrayPool (LOH)] Successfully rented & returned {bufferSize} bytes from shared pool.");

    // Span vs Substring
    string sampleLog = "LOG_2026-05-27_ACTIVE";
    var parsedSub = SpanParsingDemo.ParseWithSubstring(sampleLog);
    var parsedSpan = SpanParsingDemo.ParseWithSpan(sampleLog);
    bool isActive = SpanParsingDemo.IsActiveStatusWithSpan(sampleLog);
    Console.WriteLine($"[Span<T>] Substring Result: {parsedSub.Year}-{parsedSub.Month:D2}-{parsedSub.Day:D2} [{parsedSub.Status}]");
    Console.WriteLine($"[Span<T>] AsSpan Result:    {parsedSpan.Year}-{parsedSpan.Month:D2}-{parsedSpan.Day:D2} [{parsedSpan.Status}]");
    Console.WriteLine($"[Span<T>] Zero-alloc SequenceEqual status check: isActive = {isActive}");

    // Micro-benchmark timing
    const int iterations = 100_000;
    var sw = Stopwatch.StartNew();
    for (int i = 0; i < iterations; i++)
    {
        _ = SpanParsingDemo.ParseWithSubstring(sampleLog);
    }
    sw.Stop();
    long substringMs = sw.ElapsedMilliseconds;

    sw.Restart();
    for (int i = 0; i < iterations; i++)
    {
        _ = SpanParsingDemo.ParseWithSpan(sampleLog);
    }
    sw.Stop();
    long spanMs = sw.ElapsedMilliseconds;

    Console.WriteLine($"[Benchmark - {iterations:N0} ops] Substring: {substringMs} ms | AsSpan: {spanMs} ms (~{(double)substringMs / Math.Max(1, spanMs):F1}x faster, zero allocation)");
}

async Task RunCloudEngineeringDemoAsync()
{
    PrintSectionHeader("Cloud Engineering (Azure, AWS, GCP Basics)");

    var factory = new CloudServiceFactory();
    var providers = new[] { CloudProvider.Azure, CloudProvider.Aws, CloudProvider.Gcp };

    // 1. Blob / Object Storage across Providers
    Console.WriteLine("[1. Blob / Object Storage]");
    foreach (var provider in providers)
    {
        var blobStorage = factory.GetBlobStorage(provider);
        byte[] payload = System.Text.Encoding.UTF8.GetBytes($"Payload for {provider}");
        using var stream = new MemoryStream(payload);
        string uri = await blobStorage.UploadBlobAsync("mastery-container", "invoice-001.pdf", stream, "application/pdf");
        Console.WriteLine($"   [{provider}] Uploaded to URI: {uri}");
    }

    // 2. Secret Management across Providers
    Console.WriteLine("\n[2. Secret Management (Passwordless / Managed Keys)]");
    foreach (var provider in providers)
    {
        var secretService = factory.GetSecretManager(provider);
        await secretService.SetSecretAsync("DbConnection", $"Server={provider.ToString().ToLowerInvariant()}.db;Database=Prod;");
        string? secret = await secretService.GetSecretAsync("DbConnection");
        Console.WriteLine($"   [{provider}] Retrieved secret: {secret}");
    }

    // 3. Message Queues & Event-Driven Pipelines
    Console.WriteLine("\n[3. Message Queuing & Event Streaming]");
    foreach (var provider in providers)
    {
        var queue = factory.GetMessageQueue(provider);
        await queue.PublishMessageAsync("order-events", new { OrderId = 501, Provider = provider.ToString() });
        var messages = await queue.ReceiveMessagesAsync<object>("order-events", maxMessages: 1);
        Console.WriteLine($"   [{provider}] Message published & consumed: MessageId={messages[0].MessageId}");
    }

    // 4. Cloud Identity (Passwordless / Managed Identities)
    Console.WriteLine("\n[4. Cloud Identity & Token Acquisition]");
    foreach (var provider in providers)
    {
        var identity = factory.GetIdentityService(provider);
        var token = await identity.AcquireTokenAsync("https://cloud.api/.default");
        Console.WriteLine($"   [{provider}] IdP: {token.IdentityProvider} | Principal: {token.PrincipalId}");
    }
}
