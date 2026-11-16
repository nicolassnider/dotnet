# .NET Mastery Solution

A production-grade, modular .NET reference solution providing runnable implementations, micro-benchmarks, and comprehensive automated test suites for the core concepts, design patterns, async rules, performance optimizations, and web architectures outlined in [dotnet_tips.md](file:///c:/repos/dotnet/dotnet_tips.md).

All code, class identifiers, method names, XML comments, and documentation are in **English**.

---

## Solution Structure

```
DotNetMastery.sln
├── src/
│   ├── DotNetMastery.Core/           # Fundamentals, keywords, delegates, and clean code principles
│   ├── DotNetMastery.Patterns/       # Top 5 Design Patterns (DI, Factory, Repository, Strategy, Singleton)
│   ├── DotNetMastery.DataAndAsync/   # Collections (IEnumerable vs IQueryable), LINQ, 11 Async Rules, ValueTask
│   ├── DotNetMastery.Performance/    # Memory leak prevention, ArrayPool for LOH, Span<T> zero-alloc parsing
│   ├── DotNetMastery.Cloud/          # Multi-cloud architecture: Azure, AWS, GCP basics & abstractions
│   ├── DotNetMastery.WebApi/         # ASP.NET Core: Pipeline, Correlation ID, Secure CORS, Rate Limiting, DI lifecycles
│   └── DotNetMastery.Cli/            # Interactive console runner showcasing all demos with live benchmark
└── tests/
    └── DotNetMastery.Tests/          # 49+ automated xUnit tests & WebApplicationFactory integration tests
```

---

## Projects Overview

### 1. `DotNetMastery.Core`
- **Memory Model**: Value types (Stack) vs Reference types (Heap), value copy semantics vs reference pointer copy semantics.
- **Type Conversions**: Boxing value types to heap objects and unboxing mechanics, including `InvalidCastException` behavior.
- **Language Keywords**:
  - `ref` vs `out`: passing by reference vs returning multiple mandatory outputs.
  - `const` vs `readonly`: compile-time constant inlining vs runtime-initialized immutability.
  - `var` vs `dynamic`: compile-time type inference vs runtime dynamic dispatch.
  - `yield return`: lazy evaluation vs eager in-memory list allocation.
  - `Equals()` vs `==`: semantic equality vs reference equality vs record value equality.
  - `enum` and `nameof`: fast, zero-allocation identifier formatting vs reflection-based `.ToString()`.
- **Delegates & Events**:
  - `Action`, `Func`, and `Predicate` delegate usage.
  - Multicast delegate chains (`+=`, `-=`).
  - Delegates vs Events: why event encapsulation protects against accidental external overwriting (`event = null`).
- **Clean Code Principles**:
  - KISS: concise null-propagation vs over-engineered conditionals.
  - Clever vs Maintainable code: cryptic loops vs self-documenting domain methods.
  - Vertical coding style for fluent LINQ chains.

### 2. `DotNetMastery.Patterns`
- **Dependency Injection**: Tightly-coupled instantiations vs constructor-injected abstractions for testability.
- **Factory Method**: Dynamic logger creation (`FileLogger`, `DatabaseLogger`, `ConsoleLogger`) with modern C# switch expressions.
- **Repository Pattern**: Collection-like data access abstraction (`IUserRepository`, `UserRepository`) backed by an in-memory Entity Framework Core database context.
- **Strategy Pattern**: Interchangeable payment algorithms (`CreditCard`, `PayPal`, `Bitcoin`) swappable dynamically at runtime.
- **Singleton Pattern**: Naive unsafe singleton vs thread-safe `Lazy<T>` singleton vs DI-managed singleton (`AddSingleton`).

### 3. `DotNetMastery.DataAndAsync`
- **Collections & LINQ**:
  - `IEnumerable` (in-memory filtering after full fetch) vs `IQueryable` (expression trees translated into data source queries) vs `List`.
  - `OrderBy` vs `ThenBy`: why multiple `OrderBy` calls overwrite previous sorting, whereas `ThenBy` creates composite keys.
  - `GroupBy` vs modern `CountBy` (.NET 9+): single-pass frequency count without materializing intermediate group lists.
- **11 Essential Async/Await Rules**:
  1. Never block async code (`await` instead of `.Result` or `.Wait()`).
  2. Never return `async void` for tasks (use `Task` or `Task<T>`; keep `async void` strictly for event handlers).
  3. Run independent tasks concurrently using `Task.WhenAll`.
  4. Do not wrap asynchronous I/O in `Task.Run`.
  5. Use `ConfigureAwait(false)` in reusable library code to prevent context capture.
  6. Keep async all the way up the call stack.
  7. Always observe and handle task exceptions (`try/catch` around `await`).
  8. Avoid marking methods `async` if they don't perform asynchronous awaits (use `Task.FromResult`).
  9. Pass `CancellationToken` to long-running operations.
  10. Avoid sequential `await` inside loops when tasks can execute concurrently.
  11. Use `Task.WhenEach` (.NET 9+) to stream completed tasks as they finish.
- **Task vs ValueTask**:
  - Heap-allocated `Task<T>` on every call vs zero-allocation `ValueTask<T>` on synchronous cache hits.

### 4. `DotNetMastery.Performance`
- **Memory Leaks in .NET**:
  - Event subscription leaks and resolution via the `IDisposable` pattern.
  - Unbounded static caches vs `BoundedSmartCache<TKey, TValue>` with strict capacity limits and FIFO/LRU eviction.
  - `IMemoryCache` with absolute and sliding expiration policies.
  - Large Object Heap (LOH) pressure (>85KB) and zero-fragmentation buffer reuse via `ArrayPool<byte>.Shared`.
  - Task closure retention: capturing small scalars instead of full buffers in background tasks.
- **High-Performance Parsing**:
  - `Substring()` allocation overhead vs `ReadOnlySpan<char>` and `AsSpan()` zero-allocation slicing.
  - Zero-allocation string comparison via `SequenceEqual`.
  - Live micro-benchmark demonstrating `AsSpan()` running ~2.1x faster with zero heap allocations.

### 5. `DotNetMastery.Cloud`
- **Multi-Cloud Abstractions**:
  - `IBlobStorageService`: unified upload, download, exists, and delete for unstructured files.
  - `ISecretManagerService`: centralized secret and connection string retrieval.
  - `IMessageQueueService`: unified producer/consumer pattern for asynchronous message queues and event buses.
  - `ICloudIdentityService`: token acquisition abstraction for passwordless cloud authentication.
- **Provider Implementations**:
  - **Azure**: Azure Blob Storage (`Azure.Storage.Blobs`), Key Vault Secrets (`Azure.Security.KeyVault.Secrets`), Service Bus (`Azure.Messaging.ServiceBus`), and Entra ID Managed Identity (`DefaultAzureCredential`).
  - **AWS**: Amazon S3 (`AWSSDK.S3`), Secrets Manager (`AWSSDK.SecretsManager`), SQS (`AWSSDK.SQS`), and IAM Roles for Service Accounts (IRSA).
  - **GCP**: Google Cloud Storage (`Google.Cloud.Storage.V1`), Secret Manager (`Google.Cloud.SecretManager.V1`), Cloud Pub/Sub (`Google.Cloud.PubSub.V1`), and Workload Identity (Application Default Credentials).
- **Cloud Factory & Health Checks**:
  - `CloudServiceFactory`: dynamic runtime provider resolution.
  - `CloudStorageHealthCheck`: Kubernetes readiness/liveness probe implementation.

### 6. `DotNetMastery.WebApi`
- **Request Pipeline & Middleware Ordering**:
  - Ordered pipeline: Exception handling → HTTPS Redirection → Routing → Secure CORS → Rate Limiting → Correlation ID → Endpoints.
- **Correlation ID Middleware**:
  - Extracts or generates `X-Correlation-ID`.
  - Propagates ID into response headers and `HttpContext.Items`.
  - Structured logging with `_logger.BeginScope("CorrelationId", id)`.
- **CORS Configuration**:
  - Insecure wildcard policy vs production-ready `SecurePolicy` with explicit origins, methods, headers, and credentials.
- **Native Rate Limiting**:
  - Fixed Window Rate Limiter.
  - Sliding Window Rate Limiter.
- **Dependency Injection Lifecycles**:
  - `ITransientOperation`, `IScopedOperation`, `ISingletonOperation` exposed via `/api/dilifecycle` to demonstrate lifecycle scope behavior.
- **RESTful Endpoints & Validation**:
  - `UsersController` with `CreateUserRequest` validating required fields, email format, and age range with `ModelState` validation.

### 7. `DotNetMastery.Cli`
- Interactive console runner that provides a topic-by-topic showcase and micro-benchmark comparisons.

### 8. `DotNetMastery.Tests`
- Comprehensive xUnit test suite containing **49 passing tests** across core, patterns, data/async, performance, multi-cloud, and WebApplicationFactory integration test suites.

---

## Getting Started

### Prerequisites
- [.NET 11 SDK](https://dotnet.microsoft.com/) (or .NET 10 SDK)

### Build Solution
```shell
dotnet build DotNetMastery.sln
```

### Run Automated Tests
```shell
dotnet test DotNetMastery.sln
```

### Run Interactive Showcase CLI
```shell
# Interactive menu:
dotnet run --project src/DotNetMastery.Cli

# Run all demos non-interactively:
dotnet run --project src/DotNetMastery.Cli -- --all
```

### Run Web API
```shell
dotnet run --project src/DotNetMastery.WebApi
```
Once started, test endpoints such as:
- Root info: `GET http://localhost:5000/`
- Correlation ID test: `GET http://localhost:5000/` (check `X-Correlation-ID` response header)
- DI Lifecycles: `GET http://localhost:5000/api/dilifecycle`
- Users CRUD: `GET http://localhost:5000/api/users`
- Rate-limited endpoint: `GET http://localhost:5000/api/rate-limited/fixed`

---

## AI Agents & LLM Guidelines
See [AGENTS.md](file:///c:/repos/dotnet/AGENTS.md) and [.github/copilot-instructions.md](file:///c:/repos/dotnet/.github/copilot-instructions.md) for architectural guidelines, style standards, and topic mappings designed for AI coding assistants.
