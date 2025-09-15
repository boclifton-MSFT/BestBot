# BestPracticesMcp - Azure Functions MCP Server

**ALWAYS reference these instructions first and only fallback to search or additional commands when information here is incomplete or errors occur.**

BestPracticesMcp is an Azure Functions application (C# .NET 9) that serves as an MCP (Model Context Protocol) server. It provides best practices guidance for C#, Python, and Vue 3 development through cached markdown content served via Azure Functions with MCP extension.

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
├── BestPractices.sln              # Solution file
├── BestPracticesMcp.csproj        # Main project file (.NET 9, Azure Functions v4)
├── Program.cs                     # Application entry point with MCP configuration
├── Csharp.cs                      # C# best practices MCP tool
├── Python.cs                     # Python best practices MCP tool  
├── Vue3.cs                        # Vue 3 best practices MCP tool
├── Resources/                     # Best practices markdown files
├── host.json                      # Function host configuration
├── local.settings.json            # Local development settings (create this)
├── azure.yaml                     # Azure Developer CLI configuration
├── infra/                         # Azure infrastructure (Bicep templates)
├── .vscode/                       # VS Code configuration including MCP setup
└── .github/                       # GitHub configuration
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

Always build and validate your changes using the commands above before committing. The CI/CD pipeline expects properly formatted, buildable code.