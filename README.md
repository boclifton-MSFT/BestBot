# BestPracticesMcp

BestPracticesMcp is a small Azure Functions-based MCP (Model Context Protocol) server that serves curated, authoritative "best practices" guidance for programming languages and frameworks. The project is intentionally simple: it reads markdown resources on disk and exposes them via MCP tool triggers.

This repository is aimed at developers who want to contribute canonical best-practice guidance and publish it via an MCP server.

## Key features

- Per-language best-practices stored as markdown files in `Resources/`.
- Lightweight process-wide file caching to minimize disk reads and serve cached content when the underlying file is unchanged.
- Centralized, source-generated logging surface using `ToolLogging<T>` for consistent event messages across tools.
- Simple, extensible pattern for adding more language tools.

## Prerequisites

- .NET 9 SDK (required)
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

To add another language's best-practices follow the repository's current layout and helper conventions:

1. Add the markdown resource
   - Create a new folder for the language under `Languages/` and add `<language>-best-practices.md` there, e.g. `Languages/Go/go-best-practices.md`.
   - These language resources are included in the project output and are available at runtime from the build output `Resources/` folder (e.g. `AppContext.BaseDirectory + "Resources/go-best-practices.md"`).

2. Add an MCP tool implementation
   - Add a new C# tool class in `Functions/` named `<Language>Tools` (file `Functions/<Language>Tools.cs`).
   - Expose a public method `Get<Language>BestPractices` that the MCP extension will call.
   - Use the centralized helpers in `Utilities/`:
     - `Utilities/ToolLogging<<Language>Tools>` for consistent, source-generated logging.
     - `Utilities/FileCache` to read and cache the resource. Prefer `FileCache.TryGetValid(...)` to check cache validity and `FileCache.GetOrLoadAsync(...)` to load when necessary.
   - Provide a small fallback array of bullet-point strings the method can return if the resource cannot be read at runtime.
   - Example pattern (pseudocode):
     - Determine the resource file name (e.g. `"go-best-practices.md"`).
     - If `FileCache.TryGetValid(file, out var content)` return cached content; otherwise call `await FileCache.GetOrLoadAsync(file, async () => /* read file content */)`.
     - Log events through `ToolLogging<<Language>Tools>.SomeEvent(...)` and return the content or fallback array.

3. Build and verify
   - Run `dotnet restore` and `dotnet build BestPracticesMcp.sln`.
   - Optionally run the Functions host locally (if you have Azure Functions Core Tools) and test the MCP endpoint, or validate the file-access logic by inspecting the build output under `bin/Debug/net*/Resources/`.

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
