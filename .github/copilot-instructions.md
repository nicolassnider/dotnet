# GitHub Copilot Instructions for .NET Mastery

This repository is an educational and architectural reference solution implementing the concepts from `dotnet_tips.md`.

## Key Guidelines

1. **Language & Naming**:
   - Write all code, method names, comments, and documentation in **English**.
   - Use idiomatic C# naming conventions (PascalCase for types and public members, camelCase for parameters and local variables, `_camelCase` for private fields).

2. **Language Version & Style**:
   - Target framework: `net11.0` (C# 13+).
   - Use **file-scoped namespaces** (`namespace DotNetMastery...;`).
   - Nullable reference types are enabled: avoid suppressing warnings with `!` unless strictly guaranteed safe.
   - Adhere to the **Vertical Coding Style** when chaining LINQ or fluent builders.

3. **Architecture**:
   - `DotNetMastery.Core`: Language fundamentals, keywords, delegates, and clean code principles.
   - `DotNetMastery.Patterns`: The top 5 design patterns in .NET (DI, Factory Method, Repository, Strategy, Singleton).
   - `DotNetMastery.DataAndAsync`: Collections, LINQ optimizations (CountBy, OrderBy/ThenBy), the 11 Async Rules, and ValueTask hot-path caching.
   - `DotNetMastery.Performance`: Memory leak prevention, ArrayPool for LOH, Span<T> zero-allocation parsing.
   - `DotNetMastery.Cloud`: Multi-cloud abstractions and client implementations for Azure, AWS, and GCP (Storage, Secrets, Messaging, Identity, K8s Health Checks).
   - `DotNetMastery.WebApi`: ASP.NET Core request lifecycle, OAuth 2.0 / JWT authentication, OWASP security headers & mitigations (BOLA/IDOR, SSRF), custom Correlation ID middleware, secure CORS, native rate limiting, and DI lifecycles.
   - `DotNetMastery.Cli`: Interactive console runner with micro-benchmarks.
   - `DotNetMastery.Tests`: xUnit tests for all features and WebApplicationFactory integration tests.

4. **Testing**:
   - Always run `dotnet test` to verify that all tests pass without errors or regressions.
