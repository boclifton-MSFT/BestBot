# BestPracticesMcp

BestPracticesMcp is a small Azure Functions-based MCP (Model Context Protocol) server that serves curated, authoritative "best practices" guidance for programming languages and frameworks. The project is intentionally simple: it reads markdown resources on disk and exposes them via MCP tool triggers.

This repository is aimed at developers who want to contribute canonical best-practice guidance and publish it via an MCP server.

## Key features

- Per-language best-practices stored as markdown files in `Resources/`.
- Lightweight process-wide file caching to minimize disk reads and serve cached content when the underlying file is unchanged.
- Centralized, source-generated logging surface using `ToolLogging<T>` for consistent event messages across tools.
- Simple, extensible pattern for adding more language tools.

## Prerequisites

- .NET 8 SDK (required)
- (Optional) Azure Functions Core Tools for local function host testing

## Quick start (developer)

1. Restore and build:
   - `dotnet restore`
   - `dotnet build BestPractices.sln`
2. Format code (required before committing):
   - `dotnet format BestPractices.sln`
   - Verify formatting: `dotnet format BestPractices.sln --verify-no-changes`
3. Run locally (if you have Azure Functions Core Tools installed):
   - Create `local.settings.json` (see `Resources` or project docs for a minimal example).
   - Start the Functions host (workspace task or `func host start`).

## Project layout

- `Functions/` — MCP tool implementations. Each language has a `*Tools` class that exposes a method triggered by the MCP extension.
- `Resources/` — Markdown files containing best-practice guidance (one file per language/framework).
- `Tools/` — shared helpers (logging, caching) live under `Functions/` for now.
- `infra/` — Bicep templates for Azure deployment (used by `azd` when deploying).

## Adding a new language

To add another language's best-practices:

1. Add a new markdown resource under `Resources/` named `<language>-best-practices.md`.
2. Add a new MCP tool implementation in `Functions/<Language>Tools.cs` following the existing pattern. Key points:
   - Name the class `<Language>Tools` and expose a `Get<Language>BestPractices` method.
   - Use `ToolLogging<<Language>Tools>` for logging.
   - Use `FileCache.TryGetValid` and `FileCache.GetOrLoadAsync` to read and cache the resource.
   - Provide a small fallback array of bullet points in case the resource can't be read.
3. Build and verify the project.

## Logging and instrumentation

- Logging is provided via source-generated methods defined in `Functions/ToolLogging.cs` and consumed through `ToolLogging<T>`.
- Keep message shapes stable when adding new log calls to preserve consistency.

## Contributing

We welcome contributions. Please follow these guidelines when opening a PR:

- Formatting: Run `dotnet format BestPractices.sln` before pushing; CI will verify formatting.
- Build: Ensure `dotnet build BestPractices.sln` passes locally.
- Tests: Add unit tests for new logic where appropriate. There are no unit tests currently but adding tests is encouraged.
- Small PRs: Prefer small, focused PRs (one language/tool per PR when adding new content).
- Documentation: When adding or changing content in `Resources/`, include a short rationale in the PR description and link authoritative sources.

PR checklist:

- [ ] Ran `dotnet format` locally
- [ ] Project builds (`dotnet build`)
- [ ] New content includes references to authoritative sources
- [ ] Changes documented in the PR description

## Roadmap / ideas

- Add more languages (TypeScript, Java, Go, Rust, Kotlin are good candidates).
- Add CI to run builds and formatting checks.
- Add taxonomy or tagging for resource files so clients can query by topic.

## Contact / ownership

Project maintained by repository owners. Open an issue or PR for questions, corrections, or suggested best-practice sources.
