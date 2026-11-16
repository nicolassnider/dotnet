# Agent & LLM Repository Guide (`AGENTS.md`)

This repository is a production-grade educational and practical .NET reference solution that implements and demonstrates all concepts, design patterns, async rules, performance optimizations, and web architectural patterns described in `dotnet_tips.md`.

All code, class identifiers, method names, XML documentation comments, and project documentation are written in **English**.

---

## 1. Solution Architecture & Layout

The repository uses standard .NET enterprise solution conventions organized under `src/` and `tests/`:

```
DotNetMastery/
├── DotNetMastery.sln                  # Primary Visual Studio / .NET CLI solution
├── Directory.Build.props              # Centralized MSBuild configuration (net11.0, Nullable, etc.)
├── .editorconfig                      # Code formatting, file-scoped namespaces, style rules
├── .gitignore                         # .NET gitignore rules (ignoring bin/, obj/, .vs/)
├── AGENTS.md                          # Instructions and context for AI agents and LLMs
├── README.md                          # Project documentation and quickstart guide
├── dotnet_tips.md                     # Source technical reference and interview guide
│
├── src/
│   ├── DotNetMastery.Core/            # Fundamentals, keywords, delegates, and clean code
│   │   ├── Fundamentals/
│   │   │   ├── StackVsHeapDemo.cs     # Stack vs Heap, Value vs Reference types semantics
│   │   │   ├── BoxingUnboxingDemo.cs  # Boxing/unboxing mechanics, InvalidCastException
│   │   │   └── KeywordsDemo.cs        # ref/out, const/readonly, var/dynamic, yield, Equals/==, enum/nameof
│   │   ├── Delegates/
│   │   │   └── DelegatesAndLambdasDemo.cs # Action, Func, Predicate, Multicast, Delegates vs Events
│   │   ├── CleanCode/
│   │   │   └── CleanCodePrinciples.cs # KISS, Clever vs Maintainable, Vertical coding style
│   │   └── Security/
│   │       ├── PasswordHasher.cs      # PBKDF2 password hashing with CSPRNG salt & FixedTimeEquals
│   │       └── OwaspMitigations.cs    # IDOR, Parameterized queries, SSRF IP validation
│   │
│   ├── DotNetMastery.Patterns/        # Top 5 Design Patterns in .NET
│   │   ├── DependencyInjection/
│   │   │   └── DependencyInjectionDemo.cs # Tightly-coupled anti-pattern vs Injected abstractions
│   │   ├── FactoryMethod/
│   │   │   └── FactoryMethodDemo.cs   # Factory method with modern C# switch expressions
│   │   ├── Repository/
│   │   │   └── RepositoryPatternDemo.cs # Generic & specialized repository backed by EF Core InMemory
│   │   ├── Strategy/
│   │   │   └── StrategyPatternDemo.cs # Interchangeable algorithms at runtime (payment strategies)
│   │   └── Singleton/
│   │       └── SingletonPatternDemo.cs # Naive vs Thread-safe Lazy<T> vs DI-managed singleton
│   │
│   ├── DotNetMastery.DataAndAsync/    # Collections, LINQ, and Async programming
│   │   ├── Collections/
│   │   │   └── CollectionsAndLinqDemo.cs # IEnumerable vs IQueryable vs List, OrderBy/ThenBy, CountBy (.NET 9+)
│   │   └── Async/
│   │       ├── AsyncRulesDemo.cs      # Implementation and verification of the 11 Async/Await Rules
│   │       └── TaskVsValueTaskDemo.cs # Task<T> heap allocations vs ValueTask<T> zero-alloc fast path
│   │
│   ├── DotNetMastery.Performance/     # High performance, zero-allocations, memory leak prevention
│   │   ├── Memory/
│   │   │   └── MemoryLeaksDemo.cs     # Event leaks/IDisposable, Bounded cache, IMemoryCache, ArrayPool, Closures
│   │   └── Span/
│   │       └── SpanParsingDemo.cs     # Substring heap allocations vs ReadOnlySpan<char> zero allocations
│   │
│   ├── DotNetMastery.Cloud/           # Multi-cloud architecture (Azure, AWS, GCP basics)
│   │   ├── Abstractions/              # IBlobStorageService, ISecretManagerService, IMessageQueueService, ICloudIdentityService
│   │   ├── Azure/                     # Azure Blob Storage, Key Vault, Service Bus, DefaultAzureCredential
│   │   ├── Aws/                       # AWS S3, Secrets Manager, SQS, IAM / IRSA
│   │   ├── Gcp/                       # GCP Cloud Storage, Secret Manager, Cloud Pub/Sub, Workload Identity
│   │   ├── Factory/                   # CloudServiceFactory & provider resolution
│   │   └── Health/                    # CloudStorageHealthCheck for Kubernetes probes
│   │
│   ├── DotNetMastery.WebApi/          # ASP.NET Core Web API architecture & best practices
│   │   ├── Middleware/
│   │   │   └── CorrelationIdMiddleware.cs # X-Correlation-ID extraction, propagation, and logger scope
│   │   ├── Security/
│   │   │   ├── JwtTokenService.cs     # OAuth 2.0 / JWT issuance & cryptographic validation
│   │   │   └── SecurityHeadersMiddleware.cs # OWASP security headers (CSP, HSTS, X-Content-Type)
│   │   ├── Services/
│   │   │   └── DiLifecycleServices.cs # Transient, Scoped, and Singleton service interfaces and implementation
│   │   ├── Models/
│   │   │   └── CreateUserRequest.cs   # DTOs with DataAnnotations model validation
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs      # Login, JWT protected endpoints, IDOR & SSRF defenses
│   │   │   ├── UsersController.cs     # RESTful CRUD operations with validation
│   │   │   └── DiLifecycleController.cs # Endpoint exposing DI lifecycle behavior and instance IDs
│   │   └── Program.cs                 # Pipeline configuration (JWT, CORS, Rate Limiting, Correlation ID, Routing)
│   │
│   └── DotNetMastery.Cli/             # Interactive console showcase application
│       └── Program.cs                 # Interactive runner with micro-benchmarks and topic demonstrations
│
└── tests/
    └── DotNetMastery.Tests/           # Automated test suite (xUnit)
        ├── CoreTests.cs               # Tests for fundamentals, keywords, delegates, and clean code
        ├── PatternsTests.cs           # Tests for DI, Factory, Repository, Strategy, and Singleton
        ├── DataAndAsyncTests.cs       # Tests for LINQ rules, 11 Async rules, and ValueTask
        ├── PerformanceTests.cs        # Tests for memory leak fixes, bounded cache, and Span parsing
        ├── CloudTests.cs              # Multi-cloud tests for Azure, AWS, and GCP implementations
        ├── SecurityTests.cs           # Tests for JWT, PBKDF2 hashing, IDOR, SSRF, and OWASP headers
        └── WebApiTests.cs             # Integration tests (WebApplicationFactory) for Web API endpoints
```

---

## 2. Command Reference

### Build the entire solution
```shell
dotnet build DotNetMastery.sln
```

### Run all automated tests
```shell
dotnet test DotNetMastery.sln
```

### Run the interactive CLI demo
```shell
# Run with interactive menu:
dotnet run --project src/DotNetMastery.Cli

# Or run all demonstrations non-interactively:
dotnet run --project src/DotNetMastery.Cli -- --all
```

### Run the ASP.NET Core Web API
```shell
dotnet run --project src/DotNetMastery.WebApi
```

---

## 3. Topic Mapping to Source Code & Tests

| Topic from `dotnet_tips.md` | Implementation File | Test File |
|---|---|---|
| **Stack vs Heap & Value vs Reference Types** | `src/DotNetMastery.Core/Fundamentals/StackVsHeapDemo.cs` | `CoreTests.cs` |
| **Boxing & Unboxing** | `src/DotNetMastery.Core/Fundamentals/BoxingUnboxingDemo.cs` | `CoreTests.cs` |
| **Keywords (`ref`, `out`, `const`, `readonly`, `var`, `dynamic`, `yield`, `Equals`, `enum`)** | `src/DotNetMastery.Core/Fundamentals/KeywordsDemo.cs` | `CoreTests.cs` |
| **Delegates, Multicast & Events** | `src/DotNetMastery.Core/Delegates/DelegatesAndLambdasDemo.cs` | `CoreTests.cs` |
| **Clean Code & KISS & Vertical Style** | `src/DotNetMastery.Core/CleanCode/CleanCodePrinciples.cs` | `CoreTests.cs` |
| **Top 5 Design Patterns (DI, Factory, Repo, Strategy, Singleton)** | `src/DotNetMastery.Patterns/` | `PatternsTests.cs` |
| **IEnumerable vs IQueryable vs List** | `src/DotNetMastery.DataAndAsync/Collections/CollectionsAndLinqDemo.cs` | `DataAndAsyncTests.cs` |
| **LINQ (`OrderBy`/`ThenBy`, `CountBy`)** | `src/DotNetMastery.DataAndAsync/Collections/CollectionsAndLinqDemo.cs` | `DataAndAsyncTests.cs` |
| **11 Async / Await Rules** | `src/DotNetMastery.DataAndAsync/Async/AsyncRulesDemo.cs` | `DataAndAsyncTests.cs` |
| **Task vs ValueTask (Hot-Path Caching)** | `src/DotNetMastery.DataAndAsync/Async/TaskVsValueTaskDemo.cs` | `DataAndAsyncTests.cs` |
| **Memory Leaks & Prevention (LOH, ArrayPool, Bounded Cache)** | `src/DotNetMastery.Performance/Memory/MemoryLeaksDemo.cs` | `PerformanceTests.cs` |
| **Span\<T\> & AsSpan() vs Substring()** | `src/DotNetMastery.Performance/Span/SpanParsingDemo.cs` | `PerformanceTests.cs` |
| **ASP.NET Core Middleware & Correlation IDs** | `src/DotNetMastery.WebApi/Middleware/CorrelationIdMiddleware.cs` | `WebApiTests.cs` |
| **Secure CORS & Native Rate Limiting** | `src/DotNetMastery.WebApi/Program.cs` | `WebApiTests.cs` |
| **DI Lifecycles (Transient, Scoped, Singleton)** | `src/DotNetMastery.WebApi/Services/DiLifecycleServices.cs` | `WebApiTests.cs` |
| **Model Binding & Validation** | `src/DotNetMastery.WebApi/Controllers/UsersController.cs` | `WebApiTests.cs` |
| **Cloud Engineering (Azure, AWS, GCP Storage)** | `src/DotNetMastery.Cloud/` | `CloudTests.cs` |
| **Cloud Secrets & Passwordless Identity** | `src/DotNetMastery.Cloud/` | `CloudTests.cs` |
| **Cloud Messaging & K8s Health Checks** | `src/DotNetMastery.Cloud/` | `CloudTests.cs` |
| **OAuth 2.0, OIDC & JWT Bearer Auth** | `src/DotNetMastery.WebApi/Security/JwtTokenService.cs` | `SecurityTests.cs` |
| **OWASP Top 10 (IDOR, Injection, SSRF, Headers)** | `src/DotNetMastery.Core/Security/OwaspMitigations.cs` | `SecurityTests.cs` |
| **Password Hashing (PBKDF2 & Constant-Time)** | `src/DotNetMastery.Core/Security/PasswordHasher.cs` | `SecurityTests.cs` |

---

## 4. Coding & Agent Conventions

When modifying or expanding code in this repository:
1. **Target Framework**: .NET 11 (`net11.0`) with C# 13+ language features.
2. **Nullable Reference Types**: `<Nullable>enable</Nullable>` is enforced across all projects. Do not introduce unhandled nullable warnings.
3. **Namespaces**: Always use file-scoped namespaces (`namespace DotNetMastery.Core.Fundamentals;`).
4. **Documentation**: Add standard XML doc comments (`/// <summary>`) on public classes and methods explaining why a pattern or technique is applied.
5. **Formatting**: Follow vertical coding style for LINQ queries and chained methods.
6. **Tests**: Every new feature, pattern, or demo must have a corresponding xUnit test in `tests/DotNetMastery.Tests/`. Run `dotnet test` to verify passing status before submitting changes.
