# BestBot - Azure Functions MCP Server

**ALWAYS reference these instructions first and only fallback to search or additional commands when information here is incomplete or errors occur.**

BestBot (formerly BestPracticesMcp) is an Azure Functions application (C# .NET 9) that serves as an MCP (Model Context Protocol) server. It provides best practices guidance for C#, Python, and Vue 3 development through cached markdown content served via Azure Functions with MCP extension.

## Working Effectively

### Bootstrap and Build
**CRITICAL**: Use exact timeout values. NEVER CANCEL builds or long-running commands.

1. **Prerequisites**: .NET 9 SDK is required and available
2. **Dependencies**: 
   ```bash
   dotnet restore BestPractices.sln
   ```
   - Takes ~20 seconds. NEVER CANCEL. Set timeout to 60+ seconds.

3. **Build**:
   ```bash
   dotnet build BestPractices.sln
   ```
   - Takes ~12 seconds. NEVER CANCEL. Set timeout to 60+ seconds.

4. **Publish for deployment**:
   ```bash
   dotnet publish BestPractices.sln --configuration Release
   ```
   - Takes ~9 seconds. NEVER CANCEL. Set timeout to 60+ seconds.

### Development Workflow

1. **Clean build**:
   ```bash
   dotnet clean BestPractices.sln
   ```
   - Takes ~1 second. Safe to use default timeout.

2. **Format code** (REQUIRED before committing):
   ```bash
   dotnet format BestPractices.sln
   ```
   - Takes ~13 seconds. NEVER CANCEL. Set timeout to 60+ seconds.

3. **Verify formatting**:
   ```bash
   dotnet format BestPractices.sln --verify-no-changes
   ```
   - Takes ~13 seconds. Will exit with code 2 if formatting needed.

### Local Development

**Azure Functions Core Tools Required**: The application requires Azure Functions Core Tools to run locally. Installation may fail due to network restrictions.

**Alternative Testing**: You can validate the core business logic (file access and content serving) without the full Functions runtime:
1. Build the project: `dotnet build BestPractices.sln`
2. Resources are automatically copied to `bin/Debug/net9.0/Resources/`
3. Core functionality accesses files from `AppContext.BaseDirectory + "Resources/{file}.md"`

**Local Settings Required**: Create `local.settings.json` for local development:
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "DefaultEndpointsProtocol=https;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "FUNCTIONS_INPROC_NET9_ENABLED": "1"
  }
}
```

### Azure Deployment

**Azure Developer CLI (azd)**: Primary deployment tool. May not be available due to network restrictions.

**Deployment Commands**:
```bash
azd up
```
- Provisions infrastructure using Bicep templates and deploys the function app
- Takes 5-15 minutes depending on Azure resources. NEVER CANCEL. Set timeout to 30+ minutes.

**Infrastructure**: Uses Azure Bicep templates in `/infra/` folder:
- Flex Consumption plan (Linux)
- Application Insights for monitoring
- Storage account for function state
- Managed identity for authentication

**Alternative Deployment**: If `azd` fails, use Azure CLI:
1. Provision infrastructure: `az deployment group create --resource-group <rg> --template-file infra/main.bicep`
2. Deploy code: VS Code Azure Functions extension or Azure CLI

## Validation

### Build Validation
ALWAYS run these before committing changes:
1. `dotnet build BestPractices.sln` - Ensures code compiles
2. `dotnet format BestPractices.sln --verify-no-changes` - Ensures code formatting is correct

### Functional Validation
Since there are no unit tests, validate functionality by:
1. **Resource Access**: Verify the three best practices files are accessible:
   - `Resources/csharp-best-practices.md` (126 lines)
   - `Resources/python-best-practices.md` (129 lines) 
   - `Resources/vue3-best-practices.md` (184 lines)

2. **MCP Server Testing**: If Azure Functions Core Tools are available:
   - Local server runs on `http://localhost:7071/runtime/webhooks/mcp/sse`
   - Test with MCP client configuration in `.vscode/mcp.json`

### Deployment Validation
After successful deployment:
1. Verify function endpoints are accessible
2. Test MCP server endpoints with proper authentication
3. Check Application Insights for telemetry

## Common Tasks

### Repository Structure
```
.
├── BestPracticesMcp.sln           # Solution file
├── BestPracticesMcp.csproj        # Main project file (.NET 9, Azure Functions v4)
├── Program.cs                     # Application entry point with MCP + tool registrations
├── azure.yaml                     # Azure Developer CLI configuration
├── host.json                      # Function host configuration
├── local.settings.json            # Local dev settings (not for production source control)
├── README.md                      # Project overview & included languages list
├── TASKLIST.md                    # Work tracking / backlog (if present)
├── Languages/                     # ALL language best-practice tool pairs live here
│   ├── Csharp/                    #   csharp-best-practices.md + Csharp.cs
│   ├── Python/                    #   python-best-practices.md + Python.cs
│   ├── Vue3/                      #   vue3-best-practices.md + Vue3.cs
│   ├── Javascript/                #   javascript-best-practices.md + Javascript.cs
│   ├── Typescript/                #   typescript-best-practices.md + Typescript.cs
│   ├── Go/ Java/ Rust/ ...        #   Additional languages (see directory for full list)
│   └── (Cpp, Elixir, Flutter, Kotlin, Php, R, React, Ruby, Swift, NetAspire, etc.)
├── Utilities/
│   ├── FileCache.cs               # File caching & change detection
│   └── ToolLogging.cs             # Shared logging utilities
├── infra/                         # Azure Bicep templates & modules
│   ├── main.bicep
│   └── modules/                   #   apim, flexible plans, etc.
├── docs/                          # Additional project documentation / analyses
│   └── auto-update-approaches-analysis.md
├── Properties/
│   └── launchSettings.json        # Debug/launch configuration
├── .github/                       # GitHub & Copilot instruction files
├── .vscode/                       # VS Code tasks, MCP client configuration
├── bin/                           # Build outputs (ignored in structure explanation)
├── obj/                           # Intermediate build artifacts
└── LICENSE                        # License file
```

### Key Project Details
- **Target Framework**: .NET 9
- **Azure Functions Version**: v4
- **Runtime**: dotnet-isolated
- **MCP Extension**: Microsoft.Azure.Functions.Worker.Extensions.Mcp v1.0.0-preview.6
- **Key Features**: Caching, managed identity support, Application Insights integration

### VS Code Integration
- **Extensions**: Azure Functions, C# 
- **Tasks**: Build, clean, publish defined in `.vscode/tasks.json`
- **MCP Configuration**: `.vscode/mcp.json` contains local and Azure server endpoints
- **Launch Settings**: Debug configuration for attaching to Functions process

### Important Notes
1. **NO Unit Tests**: Project currently has no test suite
2. **Resource Files**: Automatically copied to build output via project configuration
3. **Caching**: Functions implement in-memory caching with file modification checking
4. **Authentication**: Uses managed identity for Azure resources, function keys for endpoints
5. **Monitoring**: Application Insights configured for telemetry and performance monitoring
6. **Documentation sync requirement (COPILOT)**: When a new language resource or MCP tool is added to the repository, Copilot (the automated contributor agent) MUST update `README.md` to include the new language in the "Included languages" section and add the resource path (e.g. `Languages/<Language>/<language>-best-practices.md`) and the expected MCP tool path (e.g. `Functions/<Language>Tools.cs`). This update should be included in the same PR that introduces the language to the codebase.

### Best-practices markdown format standard

All language best-practices markdown files under `Languages/` **MUST** follow this standard format. This ensures consistency across languages and compatibility with the automated update worker.

**YAML frontmatter (REQUIRED at top of file):**

```yaml
---
language: "<human-readable display name>"
language_version: "<current stable version>"
last_checked: "<YYYY-MM-DD>"
resource_hash: ""
version_source_url: "<URL to check for latest version>"
---
```

**Required sections in order (use exact heading names):**

1. `# <Language> Best Practices` — Title
2. `## Overview` — 1–2 paragraphs on design philosophy and key characteristics
3. `## When to use <Language> in projects` — Bullet list of ideal use cases
4. `## Tooling & ecosystem` — Subsections: `### Core tools`, `### Project setup`
5. `## Recommended formatting & linters` — Tools, config snippets, code style essentials
6. `## Testing & CI recommendations` — Test framework, commands, CI config example
7. `## Packaging & release guidance` — Build, distribute, version
8. `## Security & secrets best practices` — Language-specific security patterns
9. `## Recommended libraries` — Table or grouped list (1–2 per need)
10. `## Minimal example` — Hello world + build/run commands
11. `## Further reading` — Key links with descriptions
12. `## Resources` — Bullet list of canonical URLs (`- Title — https://...`)

**Section naming rules:**
- Use `&` (not "and") in section names: `Tooling & ecosystem`, not `Tooling and ecosystem`
- Use exact heading names as listed above for consistency and automation compatibility
- The `## Resources` section MUST have at minimum 5 canonical HTTPS URLs

Always build and validate your changes using the commands above before committing. The CI/CD pipeline expects properly formatted, buildable code.