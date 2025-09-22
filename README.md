# BestBot

BestBot (formerly BestPracticesMcp) is a small Azure Functions-based MCP (Model Context Protocol) server that serves curated, authoritative "best practices" guidance for programming languages and frameworks. The project is intentionally simple: it reads markdown resources on disk and exposes them via MCP tool triggers.

**Renaming note:** The project is being rebranded to **BestBot** for public/branding purposes. For compatibility, the repository, solution, and project filenames (for example `BestPracticesMcp.sln` and `BestPracticesMcp.csproj`) remain unchanged — keep using those filenames in build commands and CI unless you intentionally update workflows and references.

This repository is aimed at developers who want to contribute canonical best-practice guidance and publish it via an MCP server.

## Key features

- Per-language best-practices stored as markdown files in `Languages/` alongside Azure Function code.
- Lightweight process-wide file caching to minimize disk reads and serve cached content when the underlying file is unchanged.
- Centralized, source-generated logging surface using `ToolLogging<T>` for consistent event messages across tools.
- Simple, extensible pattern for adding more language tools.

## Included languages

The repository currently contains these language resources and corresponding MCP tool patterns:

- C#
  - Resource: `Languages/Csharp/csharp-best-practices.md`
  - MCP tool pattern: `Functions/<Language>Tools.cs` (e.g. `Functions/Csharp.cs`)

- .NET Aspire
  - Resource: `Languages/NetAspire/netaspire-best-practices.md`
  - MCP tool pattern: `Functions/<Language>Tools.cs` (e.g. `Functions/NetAspire.cs`)

- JavaScript
  - Resource: `Languages/Javascript/javascript-best-practices.md`
  - MCP tool pattern: `Functions/<Language>Tools.cs` (e.g. `Functions/Javascript.cs`)

- Python
  - Resource: `Languages/Python/python-best-practices.md`
  - MCP tool pattern: `Functions/<Language>Tools.cs` (e.g. `Functions/Python.cs`)

- Vue 3
  - Resource: `Languages/Vue3/vue3-best-practices.md`
  - MCP tool pattern: `Functions/<Language>Tools.cs` (e.g. `Functions/Vue3.cs`)

- TypeScript
  - Resource: `Languages/Typescript/typescript-best-practices.md`
  - MCP tool pattern: `Functions/<Language>Tools.cs` (add `Functions/TypescriptTools.cs` when implementing)

- Java
  - Resource: `Languages/Java/java-best-practices.md`
  - MCP tool pattern: `Functions/<Language>Tools.cs` (e.g. `Functions/Java.cs`)

- PHP
  - Resource: `Languages/Php/php-best-practices.md`
  - MCP tool pattern: `Functions/<Language>Tools.cs` (e.g. `Functions/Php.cs`)

- R
  - Resource: `Languages/R/r-best-practices.md`
  - MCP tool pattern: `Functions/<Language>Tools.cs` (e.g. `Functions/R.cs`)

- React
  - Resource: `Languages/React/react-best-practices.md`
  - MCP tool pattern: `Functions/<Language>Tools.cs` (e.g. `Functions/React.cs`)

- Ruby
  - Resource: `Languages/Ruby/ruby-best-practices.md`
  - MCP tool pattern: `Functions/<Language>Tools.cs` (e.g. `Functions/Ruby.cs`)

- Elixir
  - Resource: `Languages/Elixir/elixir-best-practices.md`
  - MCP tool pattern: `Functions/<Language>Tools.cs` (e.g. `Functions/Elixir.cs`)

- Swift
  - Resource: `Languages/Swift/swift-best-practices.md`
  - MCP tool pattern: `Functions/<Language>Tools.cs` (e.g. `Functions/Swift.cs`)

- Kotlin
  - Resource: `Languages/Kotlin/kotlin-best-practices.md`
  - MCP tool pattern: `Functions/<Language>Tools.cs` (e.g. `Functions/Kotlin.cs`)

- Go
  - Resource: `Languages/Go/go-best-practices.md`
  - MCP tool pattern: `Functions/<Language>Tools.cs` (e.g. `Functions/Go.cs`)

- Flutter
  - Resource: `Languages/Flutter/flutter-best-practices.md`
  - MCP tool pattern: `Functions/<Language>Tools.cs` (e.g. `Functions/Flutter.cs`)

- Rust
  - Resource: `Languages/Rust/rust-best-practices.md`
  - MCP tool pattern: `Functions/<Language>Tools.cs` (e.g. `Functions/Rust.cs`)

- C++
  - Resource: `Languages/Cpp/cpp-best-practices.md`
  - MCP tool pattern: `Functions/<Language>Tools.cs` (e.g. `Functions/Cpp.cs`)

## Prerequisites

- .NET 9 SDK (required)
- (Optional) Azure Functions Core Tools for local function host testing
- (Optional) Azure Developer CLI (azd) — supported for provisioning and deploying this project. The repository includes an `azure.yaml` and Bicep templates under `infra/` so you can use `azd up` to create the required Azure resources and deploy the function app. (See "Quick start (developer)" for a sample `azd` workflow.)

## Quick start (developer)

1. Restore and build:
   - `dotnet restore`
   - `dotnet build BestPractices.sln`

> Note: The public/branding name shown in this README is **BestBot**; however the actual solution and project filenames still use the original names (e.g. `BestPracticesMcp.sln`). When running local builds, CI pipelines, or other tooling, continue to use the filenames shown in the code blocks above to avoid breaking tooling.

2. Format code (required before committing):
   - `dotnet format BestPractices.sln`
   - Verify formatting: `dotnet format BestPractices.sln --verify-no-changes`
3. Run locally (if you have Azure Functions Core Tools installed):
   - Create `local.settings.json` (see `Resources` or project docs for a minimal example).
   - Start the Functions host (workspace task or `func host start`).
4. (Optional) Provision and deploy to Azure using the Azure Developer CLI (`azd`):
   - Ensure you have `azd` installed and authenticated to the target subscription.
   - From the repository root run `azd up` — this uses `azure.yaml` and `infra/main.bicep` to provision the required resources and deploy the code. Note: provisioning can take several minutes.

## Project layout

- `./Functions/` — MCP tool implementations. Each language has a `*Tools` class that exposes a method triggered by the MCP extension.
- `./Languages/` — Each language is stored in a folder named for the language (e.g. `/Elixir`). This folder contains two files: 
  - A source markdown file containing best-practice guidance. 
  - A `<language-name>.cs` file containing the Azure Function code written in C#.
- `./Utilities/` — shared helpers (logging, caching) such as `FileCache.cs` and `ToolLogging.cs` live here.
- `./infra/` — Bicep templates for Azure deployment (used by `azd` when deploying).

## Adding a new language using Github

- Create an Issue in the Github repository using the "Request new language" issue template. 
- Fill in the language name in the two required places (where you see "\<INSERT LANGUAGE NAME HERE>")
- If you have any __authoritative__ documentation sources from the internet, add them where indicated.  
<small>*Examples of "authoritative sources" include official language or framework documentation that is kept current by the language "owner" or core maintainer, or well-known public-facing websites focused mostly on the language. In most cases, you should not include individual articles or opinion pieces as these community-driven approaches may not be supported by the official language owner and so may present suggestions that are more volatile and prone to change.*</small>
- Submit the issue.  That's it!  The team will take it from here.

## Adding a new language in code

To add another language's best-practices follow the repository's current layout and helper conventions:

1. Add the markdown resource
   - Create a new folder for the language under `Languages/` and add `<language>-best-practices.md` there, e.g. `Languages/Go/go-best-practices.md`.  Ensure the markdown file name uses `kebab-case`.
   - These language resources are included in the project output and are available at runtime from the build output `Resources/` folder (e.g. `AppContext.BaseDirectory + "Resources/go-best-practices.md"`).

2. Add an MCP tool implementation
   - Add a new C# tool class in `Languages/<language_name>/<language_name>.cs` named `<Language>Tools`.
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

We welcome contributions! Please follow these guidelines when opening a PR:

- Formatting: Run `dotnet format BestPractices.sln` before pushing; CI will verify formatting.
- Build: Ensure `dotnet build BestPracticesMcp.sln` passes locally.
- Tests: Add unit tests for new logic where appropriate. There are no unit tests currently but adding tests is encouraged.
- Small PRs: Prefer small, focused PRs (one language/tool per PR when adding new content).
- Documentation: When adding or changing content in `Resources/`, include a short rationale in the PR description and link authoritative sources.

## Contribution Guidelines

We welcome contributions of any size. The following guidelines help us keep changes small, reviewable, and easy to validate.

- Follow the repository formatting rules:
  - Run `dotnet format BestPractices.sln` before committing and ensure `dotnet format BestPractices.sln --verify-no-changes` passes.
- Build and validate your changes locally:
  - Ensure `dotnet build BestPracticesMcp.sln` succeeds.
  - If you add or change language resources, verify the files are copied to the build output `bin/Debug/net*/Resources/` and can be read by the tools.
- Adding a new language/tool:
  - Add a markdown resource under `Languages/<Language>/<language>-best-practices.md`.
  - Add the corresponding MCP tool implementation `Functions/<Language>Tools.cs` (expose `Get<Language>BestPractices` and use `Utilities/FileCache` and `Utilities/ToolLogging<T>`).
  - Documentation sync requirement (COPILOT): When a new language resource or MCP tool is added, update this `README.md` to include the new language in the "Included languages" list and add the resource path (e.g. `Languages/<Language>/<language>-best-practices.md`) and the expected MCP tool path (e.g. `Functions/<Language>Tools.cs`). This README update must be included in the same PR that introduces the language.
- PR size and scope:
  - Prefer small, focused PRs (one language/tool per PR when adding new content).
  - Include a short rationale and any authoritative sources for new or changed guidance content.
- Tests and validation:
  - Add unit tests for new logic where appropriate. There are no tests currently for the content files, but project-specific tests are encouraged.

PR checklist (include in PR description):

- [ ] Ran `dotnet format` locally
- [ ] Project builds (`dotnet build`)
- [ ] New content includes references to authoritative sources
- [ ] README updated (if adding a language or changing resource paths)
- [ ] Changes documented in the PR description

## Contact / ownership

Project maintained by repository owners. Open an issue or PR for questions, corrections, or suggested best-practice sources.
