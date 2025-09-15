# TASKS.md

Last updated: 2025-09-15

## Project Implementation Plan

1. **Initial Setup & Research**
   - [ ] **Review** existing documentation and requirements (README, TASKS/TASKLIST, source files under `Languages/`).
   - [ ] **Identify** key technologies and dependencies (`.NET 9`, Azure Functions v4, MCP extension).
   - [ ] **Setup** development environment and version control (local `dotnet` toolchain, `local.settings.json`).

2. **Architecture & Design**
   - [ ] **Draft** high-level architecture diagrams showing Functions, shared utilities, and resource files in `Languages/`.
   - [ ] **Define** responsibilities for shared utilities (`Utilities/FileCache.cs`, `Utilities/ToolLogging.cs`).
   - [ ] **Design** API contracts for MCP tools (per-language tool endpoints and payloads).

3. **Core Components Development**
   - [ ] **Implement** MCP tool classes for each language
     - [ ] Create `Functions/Csharp.cs`, `Functions/Python.cs`, `Functions/Vue3.cs`, `Functions/TypescriptTools.cs`, `Functions/Java.cs` as needed.
     - [ ] Ensure each tool uses `Utilities/FileCache.cs` for resource caching and `Utilities/ToolLogging<T>` for logging.
     - [ ] Add resource markdown files under `Languages/<Lang>/<lang>-best-practices.md`.
   - [ ] **Add** unit and integration tests for caching and endpoints
     - [ ] Validate TTL behavior, concurrency, and file-change detection.

4. **Integration & Configuration**
   - [ ] **Develop** integration between MCP tools and shared utilities; remove duplicated or deprecated logger classes.
   - [ ] **Configure** environment variables and local settings (`local.settings.json`) for local Functions host development.
   - [ ] **Setup** sample MCP client configuration in `.vscode/mcp.json` for local testing.

5. **Azure Service Integration**
   - [ ] **Implement** IaC under `infra/` (Bicep templates like `main.bicep` and parameter files).
   - [ ] **Configure** authentication and managed identity where needed for production endpoints.
   - [ ] **Test** deployments using `azd up` or equivalent `az`+deploy steps; ensure `azure.yaml` exists if using `azd`.

6. **Testing & Quality Assurance**
   - [ ] **Create** CI pipeline (GitHub Actions) to run: `dotnet restore`, `dotnet build`, `dotnet format --verify-no-changes`, and tests.
   - [ ] **Perform** end-to-end testing locally using Azure Functions Core Tools when available.
   - [ ] **Verify** system meets formatting and build requirements before merging PRs.

7. **Documentation & Handover**
   - [ ] **Update** project README with setup instructions and local run steps.
   - [ ] **Create** short guides for adding new language resources and MCP tools.
   - [ ] **Document** architecture decisions and locations of shared utilities.

8. **Deployment & Release Planning**
   - [ ] **Finalize** deployment strategy and post-deploy validation (Application Insights, logs).
   - [ ] **Create** CI/CD pipeline to automate builds and deploy to target Azure environment.
   - [ ] **Prepare** release notes and changelog entries.

## Advanced Features (Phase 2)

- [ ] **Expand** best-practices content to more languages (Go, Rust, Kotlin, etc.).
  - [ ] **Design** per-language resource and tool contract.
  - [ ] **Implement** tooling and tests for new language entries.
- [ ] **Add** sample MCP client apps and automated validation scripts.
- [ ] **Provide** templates for contributing guidelines, PR checks, and labeling automation.

## Discovered During Work

- [ ] Consolidate logging helpers: ensure `Utilities/ToolLogging.cs` is the single source of truth and remove old `*ToolsLogs` classes.
- [ ] Add unit tests for `Utilities/FileCache.cs` (TTL, concurrency, and file-watching behavior).
- [ ] Create GitHub Actions workflows for build/format/test and add required secrets/pipeline configuration.
- [ ] Add instructions for local development without Azure Functions Core Tools (how to validate business logic using `bin/Debug/net9.0/Resources/`).