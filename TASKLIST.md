# TASKLIST

This file records completed work, immediate validation steps, and a backlog of identified tasks for the BestPracticesMcp project.

## Completed

- Added centralized logging helpers
  - `Functions/ToolLogging.cs` — single source-generated LoggerMessage partial class plus a generic wrapper `ToolLogging<T>` for per-tool usage.
- Added shared file caching implementation
  - `Functions/FileCache.cs` — process-wide, per-path cache with per-entry semaphore locking and TTL support.
- Refactored existing tools to use shared helpers
  - `Functions/Python.cs` — replaced per-class cache and logging helpers with `FileCache` and `ToolLogging<PythonTools>`.
  - `Functions/Csharp.cs` — replaced per-class cache and logging helpers with `FileCache` and `ToolLogging<McpTools>`.
  - `Functions/Vue3.cs` — replaced per-class cache and logging helpers with `FileCache` and `ToolLogging<Vue3Tools>`.
- Removed duplicated logger partial classes
  - Removed the `*ToolsLogs` extension classes; logging is now centralized in `ToolLogging.cs`.

## Immediate validation (recommended)

- Build the solution to ensure source-generated loggers are produced and code compiles:
  - `dotnet restore` (if needed)
  - `dotnet build BestPractices.sln`
- Run formatter check before committing:
  - `dotnet format BestPractices.sln --verify-no-changes` (fix formatting with `dotnet format` if needed)

## Backlog / Future tasks

Priority items:
- Add best-practices content for additional languages:
  - `TypeScript` — Official sources:
    - https://www.typescriptlang.org/docs/
    - https://www.typescriptlang.org/handbook/
  - `Java` — Official sources:
    - https://docs.oracle.com/en/java/
    - https://openjdk.org/
    - https://google.github.io/styleguide/javaguide.html
  - `Go` — Official sources:
    - https://go.dev/doc/
    - https://go.dev/doc/effective_go
  - `Rust` — Official sources:
    - https://www.rust-lang.org/learn
    - https://doc.rust-lang.org/book/
    - https://rust-lang.github.io/api-guidelines/
  - `Kotlin` — Official sources:
    - https://kotlinlang.org/docs/home.html
    - https://kotlinlang.org/docs/coding-conventions.html
- Add MCP tool implementations for new languages (create `Functions/<Lang>Tools.cs` following the existing pattern).
- Add CI pipeline steps to:
  - Build the solution
  - Run `dotnet format --verify-no-changes`
  - Run unit tests (when present)
- Add unit and integration tests for the caching and file-serving behavior.

Enhancements and polish:
- Provide example MCP client configuration and validation scripts in `.vscode/`.
- Improve documentation: more detailed `Resources/*` editing guidelines and release notes.
- Add PR templates and contributing workflow automation (branch naming, changelogs, labels).
- Add Azure deployment smoke tests and post-deploy verification steps.

## Notes

If you want, I can:
- Add initial `Resources/*.md` stubs for the recommended languages.
- Create MCP tool classes for them using the same pattern.
- Run the solution build and start fixing any compile or formatting issues.
