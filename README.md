# .NET Mastery — Enterprise Reference Solution & Showcase

[![.NET Version](https://img.shields.io/badge/.NET-11.0%20%7C%20C%23%2013-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Tests Status](https://img.shields.io/badge/Tests-62%20Passed%20(100%25)-brightgreen)](https://github.com/dotnet/core)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Multi-Cloud](https://img.shields.io/badge/Cloud-Azure%20%7C%20AWS%20%7C%20GCP-orange)](#5-dotnetmasterycloud)
[![Security](https://img.shields.io/badge/Security-OAuth%202.0%20%7C%20OWASP%20Top%2010-red)](#6-dotnetmasterywebapi)

A modular, enterprise-grade practical .NET reference solution that turns the theory and interview concepts from [dotnet_tips.md](file:///c:/repos/dotnet/dotnet_tips.md) into clean, runnable, benchmarked, and fully tested production C# code.

All source code, identifiers, XML comments, and technical documentation are written in **English**.

---

## 🎯 General Idea & Project Goals

Most technical guides and interview cheat sheets explain concepts in isolation through hypothetical snippets. **DotNetMastery** bridges the gap between theory and real-world implementation:

1. **Concrete Implementations over Theory**: Every topic in `dotnet_tips.md` (from Stack vs Heap and Span slicing to OAuth 2.0 PKCE and multi-cloud abstractions) has real, runnable C# code.
2. **Empirical Benchmarks & Proofs**: High-performance claims (e.g. `Span<T>` vs `Substring()` or `ValueTask<T>` hot-path caching) are proven through live micro-benchmarks and memory measurement.
3. **Multi-Cloud Portability**: Demonstrates unified abstractions for Azure, AWS, and GCP (Storage, Secrets, Queues, and Passwordless Identity) allowing zero-vendor-lockin architectures.
4. **Defense-in-Depth Security**: Production implementations of PBKDF2 password hashing, strict JWT token validation, OWASP security headers, and defenses against BOLA/IDOR and SSRF.
5. **AI Agent / LLM Friendly**: Standardized with [AGENTS.md](file:///c:/repos/dotnet/AGENTS.md) and [.github/copilot-instructions.md](file:///c:/repos/dotnet/.github/copilot-instructions.md) for automated workflows.

---

## 🏗 Solution Architecture

The solution is partitioned into modular, highly cohesive projects under `src/` and `tests/`:

```
DotNetMastery.sln
│
├── src/
│   ├── DotNetMastery.Core/           # Fundamentals, keywords, delegates, and clean code principles
│   │   ├── Fundamentals/             # Stack vs Heap, Boxing/Unboxing, ref/out, const/readonly, yield, Equals
│   │   ├── Delegates/                # Action, Func, Predicate, Multicast, Event encapsulation
│   │   ├── CleanCode/                # KISS, Clever vs Maintainable, Vertical coding style
│   │   └── Security/                 # PBKDF2 password hasher, OWASP IDOR & SSRF mitigations
│   │
│   ├── DotNetMastery.Patterns/       # Top 5 Design Patterns in enterprise .NET
│   │   ├── DependencyInjection/      # Decoupled abstractions vs tight coupling
│   │   ├── FactoryMethod/            # Switch-expression logger factory
│   │   ├── Repository/               # Generic & specialized repository backed by EF Core InMemory
│   │   ├── Strategy/                 # Interchangeable payment algorithms at runtime
│   │   └── Singleton/                # Thread-safe Lazy<T> vs DI AddSingleton
│   │
│   ├── DotNetMastery.DataAndAsync/   # Collections, LINQ optimizations, and Async rules
│   │   ├── Collections/              # IEnumerable vs IQueryable vs List, OrderBy/ThenBy, CountBy (.NET 9+)
│   │   └── Async/                    # Verification of the 11 Async/Await Rules, ValueTask hot-path caching
│   │
│   ├── DotNetMastery.Performance/    # Zero-allocation parsing and memory leak prevention
│   │   ├── Memory/                   # Event leaks & IDisposable, Bounded cache, IMemoryCache, ArrayPool (LOH)
│   │   └── Span/                     # Substring allocations vs ReadOnlySpan<char> zero-allocation slicing
│   │
│   ├── DotNetMastery.Cloud/          # Multi-cloud architecture (Azure, AWS, GCP)
│   │   ├── Abstractions/             # IBlobStorageService, ISecretManagerService, IMessageQueueService, ICloudIdentityService
│   │   ├── Azure/                    # Azure Blob Storage, Key Vault, Service Bus, DefaultAzureCredential
│   │   ├── Aws/                      # AWS S3, Secrets Manager, SQS, IAM Roles / IRSA
│   │   ├── Gcp/                      # GCP Cloud Storage, Secret Manager, Cloud Pub/Sub, Workload Identity
│   │   ├── Factory/                  # CloudServiceFactory for dynamic multi-cloud resolution
│   │   └── Health/                   # CloudStorageHealthCheck for Kubernetes probes
│   │
│   ├── DotNetMastery.WebApi/         # ASP.NET Core Web API best practices
│   │   ├── Middleware/               # CorrelationIdMiddleware (X-Correlation-ID & logger scope)
│   │   ├── Security/                 # JwtTokenService (OAuth 2.0 / JWT) & SecurityHeadersMiddleware
│   │   ├── Services/                 # DI Lifecycles: Transient, Scoped, Singleton
│   │   ├── Models/                   # DTOs with DataAnnotations model validation
│   │   ├── Controllers/              # AuthController (OAuth/OWASP), UsersController, DiLifecycleController
│   │   └── Program.cs                # Ordered pipeline: CORS, Rate Limiting, JWT Auth, Correlation ID
│   │
│   └── DotNetMastery.Cli/            # Interactive console runner showcasing all demos & benchmarks
│       └── Program.cs                # Multi-topic menu runner with live micro-benchmarks
│
└── tests/
    └── DotNetMastery.Tests/          # 62 automated xUnit tests & WebApplicationFactory integration tests
        ├── CoreTests.cs              # Language mechanics, copy semantics, boxing, keywords, delegates
        ├── PatternsTests.cs          # DI, Factory, Repository, Strategy, Singleton
        ├── DataAndAsyncTests.cs      # LINQ sorting/grouping, 11 Async rules, ValueTask
        ├── PerformanceTests.cs       # Memory leak prevention, Bounded cache eviction, Span parsing
        ├── CloudTests.cs             # Multi-cloud parity tests across Azure, AWS, and GCP
        ├── SecurityTests.cs          # PBKDF2 hashing, JWT signing/validation, IDOR, SSRF, OWASP headers
        └── WebApiTests.cs            # End-to-end Web API integration tests
```

---

## 🚀 Quickstart & Setup ("Cómo Ponerlo en Marcha")

### 1. Prerequisites
- [.NET 11 SDK](https://dotnet.microsoft.com/) installed (SDK `11.0.100` or `10.0.300+`).
- Verify installation:
  ```shell
  dotnet --version
  ```

---

### 2. Build the Entire Solution
Compile all libraries, applications, and test projects:
```shell
dotnet build DotNetMastery.sln
```
*Expected output: `Build succeeded. 0 Warning(s), 0 Error(s)`.*

---

### 3. Run Automated Tests (62 Passing Tests)
Execute the comprehensive test suite validating all rules and behaviors:
```shell
dotnet test DotNetMastery.sln
```
*Expected output: `Passed: 62, Failed: 0, Skipped: 0`.*

---

### 4. Run the Interactive Showcase CLI
The CLI provides an interactive terminal runner where you can explore each topic individually or run all demonstrations with live performance timings:

```shell
# Option A: Run with interactive menu:
dotnet run --project src/DotNetMastery.Cli

# Option B: Run all topics non-interactively:
dotnet run --project src/DotNetMastery.Cli -- --all
```

#### What the CLI demonstrates:
- **Core Fundamentals**: Stack vs Heap copy mutation differences, boxing/unboxing checks, `ref`/`out`, `yield` lazy evaluation, `enum`/`nameof` performance.
- **Design Patterns**: Decoupled DI, Factory Method logger instantiation, Repository CRUD with in-memory DB, runtime Strategy swapping, Singleton identity check.
- **Data & Async**: `OrderBy`/`ThenBy` composite sorting, .NET 9 `CountBy`, 11 Async rules, `Task.WhenEach` streaming, `ValueTask` zero-allocation cache hit.
- **Performance & Memory**: Event subscriber cleanup via `IDisposable`, `BoundedSmartCache` FIFO eviction, `ArrayPool<byte>` for Large Object Heap (>85KB), and a live **100,000-iteration micro-benchmark** proving `AsSpan()` is **~2.2x faster with zero heap allocations**.
- **Cloud Engineering**: Unified storage upload, secret retrieval, and message queuing across **Azure, AWS, and GCP**.
- **Security & OWASP**: PBKDF2 password hashing verification, signed JWT issuance, BOLA/IDOR ownership validation, and SSRF internal IP blocking.

---

### 5. Run and Test the ASP.NET Core Web API
Start the Web API service locally:
```shell
dotnet run --project src/DotNetMastery.WebApi
```
The API listens on `http://localhost:5000` (or the configured launch port). Open a second terminal to test the endpoints:

#### A. Health & Features Endpoint
```shell
curl -i http://localhost:5000/
```
*Notice the response includes the OWASP Defense-in-Depth headers: `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Content-Security-Policy`, and `X-Correlation-ID`.*

#### B. OAuth 2.0 / JWT Login
Authenticate with simulated user credentials:
```shell
# In PowerShell:
$loginBody = @{ email = "alice@example.com"; password = "SecurePassword123!" } | ConvertTo-Json
$response = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
$token = $response.accessToken
Write-Host "Token received: $token"

# In Bash / curl:
TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"alice@example.com","password":"SecurePassword123!"}' | jq -r .accessToken)
```

#### C. Access Protected Endpoint with Bearer Token
```shell
# In PowerShell:
Invoke-RestMethod -Uri "http://localhost:5000/api/auth/me" -Headers @{ Authorization = "Bearer $token" }

# In Bash / curl:
curl -i http://localhost:5000/api/auth/me -H "Authorization: Bearer $TOKEN"
```

#### D. Test OWASP BOLA / IDOR Defense
Alice (`user_101`) tries to access Bob's private document (`id = 2`, owned by `user_102`):
```shell
# Returns HTTP 403 Forbidden with IDOR protection explanation:
curl -i http://localhost:5000/api/auth/document/2 -H "Authorization: Bearer $TOKEN"
```

#### E. Test OWASP SSRF Defense
Validate destination URLs:
```shell
# Safe public URL -> returns isSafeForOutbound: true
curl -s -X POST http://localhost:5000/api/auth/validate-url \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"url":"https://api.github.com/events"}'

# Dangerous Cloud Metadata URL -> returns isSafeForOutbound: false (BLOCKED)
curl -s -X POST http://localhost:5000/api/auth/validate-url \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"url":"https://169.254.169.254/latest/meta-data/"}'
```

#### F. Test Native Rate Limiting (.NET 7+)
Send multiple rapid requests to the rate-limited endpoint:
```shell
curl -i http://localhost:5000/api/rate-limited/fixed
```

#### G. Inspect DI Lifecycles
Observe how Transient IDs differ while Scoped IDs stay identical within the same HTTP request:
```shell
curl -s http://localhost:5000/api/dilifecycle | jq .
```

---

## 📚 Topic Mapping to Documentation & Code

| Concept in [dotnet_tips.md](file:///c:/repos/dotnet/dotnet_tips.md) | Source Implementation | Test Verification |
|---|---|---|
| **Stack vs Heap & Types** | `src/DotNetMastery.Core/Fundamentals/StackVsHeapDemo.cs` | `CoreTests.cs` |
| **Boxing & Unboxing** | `src/DotNetMastery.Core/Fundamentals/BoxingUnboxingDemo.cs` | `CoreTests.cs` |
| **Keywords (`ref`, `out`, `yield`, etc.)** | `src/DotNetMastery.Core/Fundamentals/KeywordsDemo.cs` | `CoreTests.cs` |
| **Delegates, Multicast & Events** | `src/DotNetMastery.Core/Delegates/DelegatesAndLambdasDemo.cs` | `CoreTests.cs` |
| **Clean Code & KISS** | `src/DotNetMastery.Core/CleanCode/CleanCodePrinciples.cs` | `CoreTests.cs` |
| **Top 5 Design Patterns** | `src/DotNetMastery.Patterns/` | `PatternsTests.cs` |
| **IEnumerable vs IQueryable** | `src/DotNetMastery.DataAndAsync/Collections/CollectionsAndLinqDemo.cs` | `DataAndAsyncTests.cs` |
| **LINQ (`OrderBy`, `CountBy`)** | `src/DotNetMastery.DataAndAsync/Collections/CollectionsAndLinqDemo.cs` | `DataAndAsyncTests.cs` |
| **11 Async / Await Rules** | `src/DotNetMastery.DataAndAsync/Async/AsyncRulesDemo.cs` | `DataAndAsyncTests.cs` |
| **Task vs ValueTask** | `src/DotNetMastery.DataAndAsync/Async/TaskVsValueTaskDemo.cs` | `DataAndAsyncTests.cs` |
| **Memory Leaks & Prevention** | `src/DotNetMastery.Performance/Memory/MemoryLeaksDemo.cs` | `PerformanceTests.cs` |
| **Span\<T\> & AsSpan()** | `src/DotNetMastery.Performance/Span/SpanParsingDemo.cs` | `PerformanceTests.cs` |
| **Multi-Cloud (Azure, AWS, GCP)** | `src/DotNetMastery.Cloud/` | `CloudTests.cs` |
| **OAuth 2.0 / JWT Authentication** | `src/DotNetMastery.WebApi/Security/JwtTokenService.cs` | `SecurityTests.cs` |
| **OWASP Top 10 Mitigations** | `src/DotNetMastery.Core/Security/OwaspMitigations.cs` | `SecurityTests.cs` |
| **Password Hashing (PBKDF2)** | `src/DotNetMastery.Core/Security/PasswordHasher.cs` | `SecurityTests.cs` |
| **Web API Pipeline & Correlation IDs** | `src/DotNetMastery.WebApi/` | `WebApiTests.cs` |

---

## 🤖 AI Agents & Developer Conventions

When working with LLMs (Copilot, Gemini, Claude, Cursor) or extending this codebase:
- Review [AGENTS.md](file:///c:/repos/dotnet/AGENTS.md) for full architecture context and agent conventions.
- Review [.github/copilot-instructions.md](file:///c:/repos/dotnet/.github/copilot-instructions.md) for style rules.
- Enforce file-scoped namespaces, nullable reference types, and vertical LINQ style.
- Ensure all 62 tests pass prior to submitting pull requests: `dotnet test DotNetMastery.sln`.
