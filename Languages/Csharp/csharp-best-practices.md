# C# Best Practices

A practical, opinionated reference for writing maintainable, reliable, and secure C# code. This document focuses on language and ecosystem best practices; combine it with framework-specific guidance (ASP.NET, Xamarin, MAUI, etc.) for application-level decisions.

## Quick checklist

- Prefer clarity over cleverness; name things for intent.
- Keep methods small and focused; aim for single responsibility.
- Favor immutable models for state where appropriate (records, readonly structs).
- Embrace nullable reference types and annotate public APIs.
- Use async/await consistently; accept CancellationToken on public async APIs.
- Validate inputs early and fail fast with helpful messages.
- Catch only what you can handle; preserve/propagate original exceptions when rethrowing.
- Dispose of unmanaged and disposable resources deterministically (prefer `using` / `await using`).
- Add logging for observable failures — avoid noisy logs in hot paths.
- Run analyzers and formatters in CI to enforce standards.

---

## Naming, style and source layout

- Types and public members: PascalCase. Locals and parameters: camelCase.
- Prefer descriptive, intent-revealing names; avoid abbreviations and single-letter names except in small scopes (e.g. LINQ lambdas).
- Keep one top-level public type per file when practical. File name should match the primary public type.
- Use folders that map to namespaces and keep namespaces aligned with project structure.
- Public API signatures should be stable and well-documented — prefer XML docs for libraries.

## Formatting and tooling

- Use `dotnet format` and enforce formatting in CI: "dotnet format --verify-no-changes".
- Enable and configure Roslyn analyzers and code style rules; treat important analyzers as build-breakers in CI.
- Use EditorConfig to set consistent rules across editors (.editorconfig in repo root).
- Integrate StyleCop.Analyzers or other rule sets if your team follows stricter style rules.

## Nullability and types

- Enable nullable reference types ("<Nullable>enable</Nullable>") at the project level and address warnings.
- Prefer `string?` for parameters that accept null and `string` for non-nullable contracts; document semantics.
- Use `ReadOnlySpan<T>` / `Span<T>` in performance-sensitive code to avoid allocations.
- Prefer `enum` for closed sets of values. Use `Flags` sparingly and document bit-field semantics.

## Async/Concurrency

- Prefer async all the way: use `async`/`await` and avoid blocking (Task.Result/Wait) in async code paths.
- Accept CancellationToken on public APIs that may be long-running; do not swallow cancellation requests.
- Avoid capturing the SynchronizationContext unnecessarily; use `ConfigureAwait(false)` in library code where appropriate.
- For parallel CPU-bound work, prefer `Parallel.ForEach` or `Task.Run` guarded by sensible degree-of-parallelism; prefer Channels or Dataflow for producer/consumer pipelines.

## Error handling and exceptions

- Use exceptions for exceptional conditions. Do not use exceptions for control flow.
- Catch only those exceptions you can handle; otherwise let them propagate.
- Wrap exceptions when adding context, using `throw new MyException("context", ex)` and preserve the original exception as InnerException.
- Use custom exception types sparingly and only for cases where consumers must catch specific error classes.
- Ensure library methods do not unexpectedly throw exceptions for normal operation; document thrown exceptions in XML docs.

## Resource management

- Prefer `IDisposable` and `IAsyncDisposable` patterns; prefer `using` / `await using` for deterministic disposal.
- For large object graphs, consider object pools (e.g., ArrayPool<T>) to reduce allocations.
- Close and dispose unmanaged handles promptly; prefer SafeHandle-based wrappers for interop.

## Immutability and data modeling

- Use `record` types for immutable data carriers. For mutable domain models, use clear intent and encapsulation.
- Prefer `readonly struct` for small value types that benefit from embedded semantics; avoid large structs.
- When exposing collections from APIs, return `IReadOnlyList<T>` or `IEnumerable<T>` to prevent callers from mutating internal state.

## API design guidance

- Design clear, minimal public surfaces. Prefer overloads that improve discoverability and maintain consistent parameter ordering.
- Use argument validation helpers (Guard clauses) at API boundaries and throw `ArgumentException`, `ArgumentNullException`, or `ArgumentOutOfRangeException` as appropriate.
- Consider semantic versioning and binding redirects for libraries. Avoid breaking changes in minor releases.
- Use Task-returning methods for asynchronous APIs; avoid returning `void` for asynchronous operations except for event handlers.

## Testing, CI and code quality

- Write unit tests for behavior and integration tests for contracts with external systems. Use test doubles to isolate dependencies.
- Run static analysis, tests, format checks, and security scanning in CI. Fail builds on analyzer or test regressions.
- Use coverage tools to ensure critical paths are exercised; aim for meaningful coverage without gaming metrics.
- Run dependency vulnerability scans (e.g., dotnet list package --vulnerable) in CI.

## Performance and profiling

- Measure before optimizing. Use BenchmarkDotNet for microbenchmarks and dotnet-trace/dotnet-counters for runtime profiling.
- Avoid premature allocations in hot paths; prefer spans and pooling where measured improvements exist.
- Use async streaming (IAsyncEnumerable<T>) to process large sequences efficiently with backpressure.

## Security and secrets

- Never commit secrets or credentials to source control. Use user secrets or environment variables for local development and secure stores (Key Vault, Azure Managed Identity) in production.
- Sanitize and validate all external inputs. Use parameterized queries and avoid string concatenation for SQL.
- Keep dependencies up to date and monitor for CVEs.

## Packaging and releases

- For libraries, produce symbols and source link to aid debugging. Ensure NuGet package metadata is accurate (description, license, repository URL).
- Validate package contents to avoid shipping internal or unintended files.
- Use semantic versioning and document breaking changes in release notes.

## Logging and observability

- Use structured logging (Microsoft.Extensions.Logging) and avoid string concatenation in log messages; prefer message templates.
- Include correlation IDs and contextual properties for distributed tracing.
- Avoid logging sensitive information; redact or omit secrets.

---

## Resources

- Official C# docs: https://learn.microsoft.com/dotnet/csharp/
- .NET guide and design guidelines: https://learn.microsoft.com/dotnet/standard/design-guidelines/
- Roslyn analyzers: https://github.com/dotnet/roslyn-analyzers
- .NET diagnostics and profiling: https://learn.microsoft.com/dotnet/core/diagnostics/
- Async guidance: https://learn.microsoft.com/dotnet/standard/async-in-depth
- Testing in .NET: https://learn.microsoft.com/dotnet/core/testing/
- Security guidance for .NET: https://learn.microsoft.com/dotnet/standard/security/

---

This document is intentionally pragmatic—adapt conventions to your project's needs and enforce them with tooling (analyzers, formatters, CI) so that style and correctness are automated and consistent across the team.
