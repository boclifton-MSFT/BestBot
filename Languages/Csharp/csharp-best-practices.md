---
language_version: "13.0"
last_checked: "2026-02-11"
resource_hash: ""
version_source_url: "https://dotnet.microsoft.com/download"
---

# C# Best Practices

A comprehensive guide to writing maintainable, reliable, and idiomatic C# based on Microsoft's official documentation, .NET design guidelines, and Roslyn analyzer conventions.

## Overview

C# is a statically typed, object-oriented programming language developed by Microsoft as part of the .NET platform. It combines the robustness of a strong type system with modern language features including pattern matching, records, nullable reference types, async/await, and source generators. C# excels in building enterprise applications, cloud services, desktop apps, and game development with Unity. Its tight integration with the .NET runtime and extensive standard library make it a productive choice for professional, large-scale software.

## When to use C# in projects

- **Web APIs and microservices**: ASP.NET Core minimal APIs, controllers, gRPC
- **Cloud-native services**: Azure Functions, Azure Container Apps, .NET Aspire
- **Desktop applications**: WPF, WinUI 3, MAUI for cross-platform
- **Game development**: Unity engine scripting
- **Enterprise line-of-business**: Complex domain models, background workers
- **CLI tools and automation**: System.CommandLine, Spectre.Console
- **Libraries and SDKs**: NuGet packages with strong typing and IntelliSense

## Tooling & ecosystem

### Core tools
- **SDK**: [.NET SDK](https://dotnet.microsoft.com/) (includes compiler, runtime, CLI)
- **IDE**: Visual Studio, VS Code with C# Dev Kit, JetBrains Rider
- **Package manager**: [NuGet](https://www.nuget.org/) via `dotnet add package`
- **Formatter**: `dotnet format` (built-in), EditorConfig
- **Analyzers**: [Roslyn Analyzers](https://github.com/dotnet/roslyn-analyzers), StyleCop.Analyzers, SonarAnalyzer

### Project setup

```bash
dotnet new webapi -n MyApi
cd MyApi
dotnet build
dotnet run
```

## Recommended formatting & linters

### dotnet format (built-in, recommended)

```bash
# Format code
dotnet format

# Verify formatting in CI
dotnet format --verify-no-changes
```

### EditorConfig

Place an `.editorconfig` at the repo root to enforce consistent rules across editors and CI:

```ini
[*.cs]
indent_style = space
indent_size = 4
dotnet_sort_system_directives_first = true
csharp_style_var_for_built_in_types = false:suggestion
```

### Code style essentials

- Types and public members: `PascalCase`; locals and parameters: `camelCase`
- Prefer descriptive, intent-revealing names; avoid abbreviations
- One top-level public type per file; file name matches the primary type
- Enable nullable reference types (`<Nullable>enable</Nullable>`) project-wide
- Use `using` / `await using` for deterministic disposal of resources

```csharp
public record UserProfile(string Name, string Email)
{
    public string DisplayName => Name.Trim();
}
```

## Testing & CI recommendations

### xUnit + NSubstitute

```bash
dotnet new xunit -n MyApi.Tests
dotnet add MyApi.Tests reference MyApi
```

Example test:

```csharp
using Xunit;

public class UserProfileTests
{
    [Fact]
    public void DisplayName_TrimsWhitespace()
    {
        var profile = new UserProfile("  Alice  ", "alice@example.com");
        Assert.Equal("Alice", profile.DisplayName);
    }

    [Theory]
    [InlineData("Bob", "Bob")]
    [InlineData(" Carol ", "Carol")]
    public void DisplayName_ReturnsExpected(string input, string expected)
    {
        var profile = new UserProfile(input, "test@test.com");
        Assert.Equal(expected, profile.DisplayName);
    }
}
```

### CI configuration (GitHub Actions)

```yaml
name: CI
on: [push, pull_request]
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 9.0.x
      - run: dotnet restore
      - run: dotnet build --no-restore
      - run: dotnet format --verify-no-changes
      - run: dotnet test --no-build --verbosity normal
```

## Packaging & release guidance

- For libraries, emit symbols and enable Source Link to aid consumer debugging
- Keep NuGet package metadata accurate (description, license, repository URL)
- Use semantic versioning and document breaking changes in release notes
- Produce both Debug and Release builds; only publish Release to NuGet
- Validate package contents to avoid shipping internal or unintended files

## Security & secrets best practices

- Never commit secrets to source control; use user secrets (`dotnet user-secrets`) locally and Azure Key Vault or Managed Identity in production
- Sanitize and validate all external inputs; use parameterized queries for SQL
- Keep dependencies up to date; run `dotnet list package --vulnerable` in CI
- Use `SecureString` sparingly; prefer short-lived credentials and token-based auth
- Enable Roslyn security analyzers (CA2xxx rules) to detect common vulnerabilities

## Recommended libraries

| Need | Library | Notes |
|------|---------|-------|
| Web framework | [ASP.NET Core](https://learn.microsoft.com/aspnet/core/) | Minimal APIs or MVC controllers |
| ORM | [Entity Framework Core](https://learn.microsoft.com/ef/core/) | Database access with LINQ |
| HTTP client | [HttpClient](https://learn.microsoft.com/dotnet/fundamentals/networking/http/httpclient) | Built-in; use IHttpClientFactory |
| Testing | [xUnit](https://xunit.net/) + [NSubstitute](https://nsubstitute.github.io/) | Unit testing and mocking |
| Serialization | [System.Text.Json](https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/) | High-performance JSON |
| Logging | [Microsoft.Extensions.Logging](https://learn.microsoft.com/dotnet/core/extensions/logging) | Structured logging with providers |

## Minimal example

```csharp
// Program.cs
Console.WriteLine("Hello, C#!");
```

```bash
dotnet new console -n HelloCsharp
cd HelloCsharp && dotnet run
# Output: Hello, World!
```

## Further reading

- [.NET Design Guidelines](https://learn.microsoft.com/dotnet/standard/design-guidelines/) — framework and API design conventions
- [Async guidance in depth](https://learn.microsoft.com/dotnet/standard/async-in-depth) — async/await patterns and pitfalls
- [C# language reference](https://learn.microsoft.com/dotnet/csharp/language-reference/) — complete language specification

## Resources

- Official C# documentation — https://learn.microsoft.com/dotnet/csharp/
- .NET design guidelines — https://learn.microsoft.com/dotnet/standard/design-guidelines/
- Roslyn analyzers — https://github.com/dotnet/roslyn-analyzers
- .NET diagnostics and profiling — https://learn.microsoft.com/dotnet/core/diagnostics/
- Async guidance — https://learn.microsoft.com/dotnet/standard/async-in-depth
- Testing in .NET — https://learn.microsoft.com/dotnet/core/testing/
- Security guidance for .NET — https://learn.microsoft.com/dotnet/standard/security/
- NuGet documentation — https://learn.microsoft.com/nuget/
