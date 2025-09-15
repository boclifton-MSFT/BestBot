# TASKLIST

Last updated: 2025-09-15

This document records completed work, immediate validation steps, and a prioritized backlog for the BestPracticesMcp project. Tasks are organized into phases so the team can quickly see what is done, what to do next, and what we might do later.

## Next Up (priority)

These are the highest-priority, near-term tasks. Aim to complete these in the next 1–2 sprints.

- 2025-09-15 — Validate build and formatting (Immediate)
  - Run: `dotnet restore` then `dotnet build BestPracticesMcp.sln` and `dotnet format BestPracticesMcp.sln --verify-no-changes`.
- 2025-09-18 — Add MCP tool class for TypeScript (planned)
  - Implement `Functions/TypescriptTools.cs` following existing tool patterns so the TypeScript content is served via MCP endpoints.
- 2025-09-22 — CI pipeline: build + format + (future) tests
  - Add GitHub Actions workflow to run `dotnet build`, `dotnet format --verify-no-changes`, and test steps when tests exist.
- 2025-09-29 — Add unit and integration tests for caching and file serving
  - Create tests that validate `FileCache` behavior (TTL, concurrency) and tool endpoints that serve resources.

## Future (Not Planned)

Ideas and lower-priority items to pursue when time and resources permit.

- Add best-practices content for additional languages (one item per language):
  - Java — resources: Oracle docs, OpenJDK, Google Java Style Guide
  - Go — resources: go.dev docs, Effective Go
  - Rust — resources: Rust book, API guidelines
  - Kotlin — resources: kotlinlang docs and coding conventions
- Add MCP tool implementations for the above languages (create `Functions/<Lang>Tools.cs`).
- Provide example MCP client configurations and validation scripts in `.vscode/`.
- Add PR templates and contributing workflow automation (branch naming, changelogs, labels).
- Add Azure deployment smoke tests and post-deploy verification steps.

## Notes and conventions

- Dates are YYYY-MM-DD and indicate the date the task was completed or planned.
- Update this file whenever you complete a task or reprioritize work. Keep completed items concise and link to files changed.
- For immediate help, open an issue or ping the repository owner.
- 
- Note: `FileCache.cs` and `ToolLogging.cs` were moved from `Functions/` to `Utilities/` on 2025-09-15. Update any references if you relied on the old paths.

## Completed

- 2025-09-15 — Added centralized logging helpers
  - `Utilities/ToolLogging.cs` — single source-generated LoggerMessage partial and `ToolLogging<T>` wrapper for per-tool loggers.
- 2025-09-15 — Added shared file caching implementation
  - `Utilities/FileCache.cs` — process-wide, per-path cache with per-entry semaphore locking and TTL support.
- 2025-09-15 — Refactored existing tools to use shared helpers
  - `Functions/Python.cs` — now uses `FileCache` and `ToolLogging<PythonTools>`.
  - `Functions/Csharp.cs` — now uses `FileCache` and `ToolLogging<McpTools>`.
  - `Functions/Vue3.cs` — now uses `FileCache` and `ToolLogging<Vue3Tools>`.
- 2025-09-15 — Removed duplicated logger partial classes
  - Removed the `*ToolsLogs` extension classes; logging is now centralized in `ToolLogging.cs`.
- 2025-09-15 — Added TypeScript best-practices resource
  - `Languages/Typescript/typescript-best-practices.md` — initial best-practices checklist (tsconfig, linting, tooling, conventions).