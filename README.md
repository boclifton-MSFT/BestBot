# BestBot

BestBot (formerly BestPracticesMcp) is a small Azure Functions-based MCP (Model Context Protocol) server that serves curated, authoritative "best practices" guidance for programming languages and frameworks. The project is intentionally simple: it reads markdown resources on disk and exposes them via MCP tool triggers.

**Renaming note:** The project is being rebranded to **BestBot** for public/branding purposes. For compatibility, the repository, solution, and project filenames (for example `BestPracticesMcp.sln` and `BestPracticesMcp.csproj`) remain unchanged — keep using those filenames in build commands and CI unless you intentionally update workflows and references.

This repository is aimed at developers who want to contribute canonical best-practice guidance and publish it via an MCP server.

## Key features

- Per-language best-practices stored as markdown files in `Languages/` alongside Azure Function code.
- Lightweight process-wide file caching to minimize disk reads and serve cached content when the underlying file is unchanged.
- Centralized, source-generated logging surface using `ToolLogging<T>` for consistent event messages across tools.
- Simple, extensible pattern for adding more language tools.
- **Automated update worker** — a weekly Durable Agent orchestration that checks each language's reference links, detects version changes (excluding beta/preview/prerelease), and opens a GitHub PR with updated content when needed.

## Included languages

The repository currently contains these language resources and corresponding MCP tool files:

- C#
  - Resource: `Languages/Csharp/csharp-best-practices.md`
  - MCP tool file: `Languages/Csharp/Csharp.cs`
- COBOL
  - Resource: `Languages/Cobol/cobol-best-practices.md`
  - MCP tool file: `Languages/Cobol/Cobol.cs`
- C++
  - Resource: `Languages/Cpp/cpp-best-practices.md`
  - MCP tool file: `Languages/Cpp/Cpp.cs`
- Elixir
  - Resource: `Languages/Elixir/elixir-best-practices.md`
  - MCP tool file: `Languages/Elixir/Elixir.cs`
- Flutter
  - Resource: `Languages/Flutter/flutter-best-practices.md`
  - MCP tool file: `Languages/Flutter/Flutter.cs`
- Go
  - Resource: `Languages/Go/go-best-practices.md`
  - MCP tool file: `Languages/Go/Go.cs`
- Java
  - Resource: `Languages/Java/java-best-practices.md`
  - MCP tool file: `Languages/Java/Java.cs`
- JavaScript
  - Resource: `Languages/Javascript/javascript-best-practices.md`
  - MCP tool file: `Languages/Javascript/Javascript.cs`
- Kotlin
  - Resource: `Languages/Kotlin/kotlin-best-practices.md`
  - MCP tool file: `Languages/Kotlin/Kotlin.cs`
- Lua
  - Resource: `Languages/Lua/lua-best-practices.md`
  - MCP tool file: `Languages/Lua/Lua.cs`
- .NET Aspire
  - Resource: `Languages/NetAspire/netaspire-best-practices.md`
  - MCP tool file: `Languages/NetAspire/NetAspire.cs`
- PHP
  - Resource: `Languages/Php/php-best-practices.md`
  - MCP tool file: `Languages/Php/Php.cs`
- Python
  - Resource: `Languages/Python/python-best-practices.md`
  - MCP tool file: `Languages/Python/Python.cs`
- R
  - Resource: `Languages/R/r-best-practices.md`
  - MCP tool file: `Languages/R/R.cs`
- React
  - Resource: `Languages/React/react-best-practices.md`
  - MCP tool file: `Languages/React/React.cs`
- Ruby
  - Resource: `Languages/Ruby/ruby-best-practices.md`
  - MCP tool file: `Languages/Ruby/Ruby.cs`
- Rust
  - Resource: `Languages/Rust/rust-best-practices.md`
  - MCP tool file: `Languages/Rust/Rust.cs`
- Swift
  - Resource: `Languages/Swift/swift-best-practices.md`
  - MCP tool file: `Languages/Swift/Swift.cs`
- TypeScript
  - Resource: `Languages/Typescript/typescript-best-practices.md`
  - MCP tool file: `Languages/Typescript/Typescript.cs`
- Vue 3
  - Resource: `Languages/Vue3/vue3-best-practices.md`
  - MCP tool file: `Languages/Vue3/Vue3.cs`

## Prerequisites

- .NET 10 SDK (required)
- (Optional) Azure Functions Core Tools for local function host testing
- (Optional) Azure Developer CLI (azd) — supported for provisioning and deploying this project. The repository includes an `azure.yaml` and Bicep templates under `infra/` so you can use `azd up` to create the required Azure resources and deploy the function app. (See "Quick start (developer)" for a sample `azd` workflow.)

## Automated update worker

BestBot includes a weekly automated update worker built on the Microsoft Agent Framework (Durable Agents). It checks every language's best-practices document for staleness and opens a GitHub PR when updates are needed.

### How it works

1. **Timer trigger** — `UpdateTimerTrigger` fires every Monday at 2 AM UTC (`0 0 2 * * 1`). It discovers all `Languages/*/<lang>-best-practices.md` files and starts a Durable orchestration.
2. **Fan-out orchestration** — `UpdateOrchestrator` creates one Durable AI Agent session per language, running them in parallel. Each agent receives the language's current markdown content and a detailed prompt.
3. **Agent-driven analysis** — Each `LanguageUpdateAgent` (backed by Azure OpenAI) uses function tools to:
   - **ReadFrontmatter** — Parse YAML frontmatter (version, last-checked date, resource hash, version source URL).
   - **CheckLatestVersion** — Fetch the version source URL and detect the latest stable release, skipping any beta/preview/prerelease versions.
   - **CheckResourceUrls** — Verify all URLs in the `## Resources` section are reachable and unchanged.
   - **FetchUrlContent** — Retrieve full page content from reference URLs for deeper analysis.
   - **CompareContentHash** — SHA-256 hash comparison to detect meaningful content drift.
4. **PR creation** — If any language needs an update, the `PrCreationAgent` uses GitHub MCP tools to create a branch (`auto-update/<date>`), commit the updated markdown files, and open a pull request with a summary of changes.

### YAML frontmatter

Each best-practices markdown file includes a YAML frontmatter block for version tracking:

```yaml
---
language: "C#"
language_version: "13.0"
last_checked: "2026-02-11"
resource_hash: ""
version_source_url: "https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-13"
---
```

### Configuration

Add the following settings to `local.settings.json` (or Azure App Settings):

| Setting | Description | Default |
|---------|-------------|---------|
| `AZURE_OPENAI_ENDPOINT` | Azure OpenAI service endpoint | — |
| `AZURE_OPENAI_DEPLOYMENT` | Chat model deployment name | `gpt-4o` |
| `AZURE_OPENAI_KEY` | (Optional) API key; omit to use managed identity | — |
| `UpdateWorker__Enabled` | Enable/disable the weekly worker | `true` |
| `UpdateWorker__GitHubToken` | GitHub PAT with `repo` scope | — |
| `UpdateWorker__GitHubRepoOwner` | Repository owner | — |
| `UpdateWorker__GitHubRepoName` | Repository name | — |
| `UpdateWorker__DefaultBranch` | Base branch for PRs | `main` |

### Architecture

```
UpdateTimerTrigger (weekly cron)
  └─► UpdateOrchestrator (Durable orchestration)
        ├─► LanguageUpdateAgent (C#)      ─► function tools ─► result
        ├─► LanguageUpdateAgent (Python)   ─► function tools ─► result
        ├─► LanguageUpdateAgent (Go)       ─► function tools ─► result
        │   ... (parallel, one per language)
  └─► PrCreationAgent + GithubMcpClient (creates PR if updates are found)
```

## Quick start (developer)

1. Restore and build:
  - `dotnet restore BestPracticesMcp.sln`
  - `dotnet build BestPracticesMcp.sln`

> Note: The public/branding name shown in this README is **BestBot**; however the actual solution and project filenames still use the original names (e.g. `BestPracticesMcp.sln`). When running local builds, CI pipelines, or other tooling, continue to use the filenames shown in the code blocks above to avoid breaking tooling.

2. Format code (required before committing):
  - `dotnet format BestPracticesMcp.sln`
  - Verify formatting: `dotnet format BestPracticesMcp.sln --verify-no-changes`
3. Run locally (if you have Azure Functions Core Tools installed):
  - Create `local.settings.json` (see the "Configuration" section above for update worker settings and required host settings).
   - Start the Functions host (workspace task or `func host start`).
4. (Optional) Provision and deploy to Azure using the Azure Developer CLI (`azd`):
   - Ensure you have `azd` installed and authenticated to the target subscription.
   - From the repository root run `azd up` — this uses `azure.yaml` and `infra/main.bicep` to provision the required resources and deploy the code. Note: provisioning can take several minutes.

## Secure azd deployments in private networks

Some subscriptions enforce policies that automatically disable public network access on storage accounts. The infrastructure template provisions everything needed to keep deployments compliant:

- A virtual network with delegated subnets for the Function App, private endpoints, and the deployment runner.
- Private endpoints and DNS zones for the Function App storage account and an Azure Key Vault that holds deployment secrets.
- A user-assigned managed identity scoped with least-privilege access (resource group Contributor, Storage Data Contributor, Key Vault Secrets User).
- An optional Azure Container Instance (ACI) “azd runner” that executes provisioning/deployment entirely inside the private network.

### Manage secrets with the deployment Key Vault

The template outputs the Key Vault name (`AZURE_KEY_VAULT_NAME`) and exposes it to the runner as the `AZD_KEY_VAULT_NAME` environment variable. Store connection strings, repo credentials, or PATs in this vault:

```powershell
$vault = azd env get-value AZURE_KEY_VAULT_NAME
az keyvault secret set --vault-name $vault --name 'GitAccessToken' --value '<token>'
```

Inside the container use managed identity to retrieve secrets:

```bash
az keyvault secret show --vault-name "$AZD_KEY_VAULT_NAME" --name GitAccessToken --query value -o tsv
```

### Build and publish the runner image

Build the container definition in `container-runner/Dockerfile` and push it to your Azure Container Registry (ACR):

```powershell
$tag = "<your-acr>.azurecr.io/azd-runner:latest"
docker build --file container-runner/Dockerfile --tag $tag .
docker push $tag
```

If Docker Desktop is unavailable locally, build the image in CI (GitHub Actions/Azure DevOps) or via an [ACR task](https://learn.microsoft.com/azure/container-registry/container-registry-tasks-overview).

### Configure azd environment settings

After running `azd provision` at least once, configure the environment to enable the runner parameters:

```powershell
pwsh ./scripts/Configure-AciDeploymentRunner.ps1 `
  -EnvironmentName azd-env-name `
  -Image <your-acr>.azurecr.io/azd-runner:latest `
  -Command "git clone https://github.com/boclifton-MSFT/BestPracticesMcp.git repo && cd repo && azd provision --environment azd-env-name && azd deploy" `
  -EnvironmentVariables @{ AZD_ENV_NAME = "azd-env-name"; AZURE_SUBSCRIPTION_ID = "<subscription-id>" }

azd provision
```

The script enables the runner, sets the image/command parameters, and optionally injects additional environment variables. The managed identity (`DEPLOYMENT_RUNNER_IDENTITY_ID`) already has the necessary permissions to access the resource group, Key Vault, and storage account.

### Run deployments on demand

Use `scripts/Invoke-AciDeploymentRunner.ps1` to spin up an ACI job that executes the configured command:

```powershell
pwsh ./scripts/Invoke-AciDeploymentRunner.ps1 `
  -EnvironmentName azd-env-name `
  -Command "azd provision --environment azd-env-name && azd deploy" `
  -FollowLogs
```

The script reads runtime values from the azd environment, creates a uniquely named container group in the secure subnet, and streams logs until completion. Use `-NoWait` to return immediately and `-Name` to control the container group name.

### Automate deployments

- **CI/CD pipeline**: Run the trigger script from a GitHub Actions or Azure DevOps job that has az CLI/azd installed. The pipeline only needs control plane access; the ACI container performs the private-network deployment work.
- **Scheduled runs**: Combine the script with Azure Automation or Logic Apps to kick off container jobs on a schedule (nightly sync, weekly refresh, etc.).
- **Manual executions**: Operators can invoke the script locally or from a Bastion-connected VM.

### Monitor and troubleshoot

- Tail logs in real time:
  ```powershell
  az container logs --name <container-name> --resource-group <rg> --follow
  ```
- Check job status:
  ```powershell
  az container show --name <container-name> --resource-group <rg> --query "instanceView.state"
  ```
- Attach for interactive output if needed: `az container attach --name <container-name> --resource-group <rg>`

### Clean up after runs

- Set `deployDeploymentRunner` back to `false` (or rerun the configure script with an empty command) and execute `azd provision` to remove the container group until the next deployment window.
- Delete completed container groups when you no longer need their logs:
  ```powershell
  az container delete --name <container-name> --resource-group <rg> --yes
  ```

### Security checklist

- Keep the managed identity scoped to the resource group plus Storage Data Contributor and Key Vault Secrets User.
- Restrict ACR access (private endpoints, firewall rules, or AAD-only auth) so only trusted identities can pull the runner image.
- Prefer Key Vault secrets and environment variables—never bake secrets into the container image.
- Audit container runs via Azure Activity Log and consider Application Insights or Log Analytics alerts for failed deployments.

## Azure API Management Integration

BestBot supports optional Azure API Management (APIM) integration to provide a secure, manageable, and versioned API surface for MCP endpoints. When enabled, APIM fronts the Function App and requires subscription keys for access.

### Deploying with APIM

To deploy with APIM integration, set the following parameters when running `azd up`:

```bash
# Set APIM deployment parameters
azd env set deployApim true
azd env set apimPublisherEmail "your-email@example.com"
azd env set apimSku "Developer"  # Optional: Developer, Standard, Premium, Consumption

# Deploy with APIM
azd up
```

Alternatively, you can create a custom `infra/main.parameters.json` file:

```json
{
  "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#",
  "contentVersion": "1.0.0.0",
  "parameters": {
    "location": {
      "value": "southcentralus"
    },
    "environmentName": {
      "value": "${AZURE_ENV_NAME}"
    },
    "deployApim": {
      "value": true
    },
    "apimPublisherEmail": {
      "value": "your-email@example.com"
    },
    "apimSku": {
      "value": "Developer"
    }
  }
}
```

### APIM Configuration

When APIM is deployed:

- **Security**: Function App is restricted to only accept traffic from APIM using IP restrictions
- **Authentication**: APIM uses managed identity to authenticate with the Function App
- **Subscription Keys**: All API calls require valid APIM subscription keys
- **Endpoints**: MCP endpoints are available at `https://{apim-gateway}/mcp/runtime/webhooks/mcp/*`

### Testing APIM Integration

After deployment with APIM enabled, test the integration:

1. **Get the APIM endpoint** from deployment outputs:
   ```bash
   azd env get-values | grep mcpEndpoint
   ```

2. **Create a subscription** in the APIM Developer Portal or Azure Portal

3. **Test the MCP endpoint** with a subscription key:
   ```bash
   # Test MCP runtime endpoint (requires subscription key)
   curl -H "Ocp-Apim-Subscription-Key: YOUR_SUBSCRIPTION_KEY" \
        "https://your-apim-gateway.azure-api.net/mcp/runtime/webhooks/mcp/sse"
   ```

4. **Verify direct Function App access is blocked**:
   ```bash
   # This should return 403 Forbidden when APIM is enabled
   curl "https://your-function-app.azurewebsites.net/runtime/webhooks/mcp/sse"
   ```

### APIM Management

- **Developer Portal**: Access at the `apimDeveloperPortalUrl` output value
- **Management API**: Access at the `apimManagementUrl` output value  
- **Subscription Management**: Use Azure Portal or APIM REST APIs to manage API subscriptions

### Local Development vs Azure Deployment

- **Local Development**: Run Function App directly without APIM (set `deployApim: false` or omit parameter)
- **Azure Deployment**: Can choose to deploy with or without APIM based on requirements
- **Testing**: Local development endpoints work directly; Azure endpoints require APIM subscription keys when APIM is enabled

### Cost Considerations

- **Developer SKU**: Recommended for dev/test scenarios (~$50/month)
- **Consumption SKU**: Pay-per-call pricing (if available in your region)
- **Standard/Premium**: Production scenarios with higher availability requirements

### Disabling APIM

To disable APIM and allow direct Function App access:

```bash
azd env set deployApim false
azd up
```

This will redeploy the infrastructure without APIM and remove IP restrictions from the Function App.

## Project layout

- `./Languages/` — MCP tool implementations and best-practices content. Each language is stored in its own folder (for example `Languages/Elixir/`) and contains:
  - A source markdown file containing best-practice guidance. 
  - A `<Language>.cs` file containing the Azure Function MCP tool code (for example `Languages/Elixir/Elixir.cs` with class `ElixirTools`).
- `./UpdateWorker/` — Automated weekly update worker using Microsoft Agent Framework Durable Agents.
  - `Models/` — DTOs for configuration, inputs, and results.
  - `Services/` — YAML frontmatter parser and logging.
  - `FunctionTools/` — AI agent function tools (version check, resource check, frontmatter, content hash).
  - GitHub PR creation is handled by `PrCreationAgent` and `GithubMcpClient` within the Durable Agent orchestration (no separate `Activities/` folder).
  - `UpdateTimerTrigger.cs` — Weekly cron trigger.
  - `UpdateOrchestrator.cs` — Fan-out orchestration across all languages.
- `./Utilities/` — shared helpers (logging, caching) such as `FileCache.cs` and `ToolLogging.cs` live here.
- `./infra/` — Bicep templates for Azure deployment (used by `azd` when deploying).
  - `main.bicep` — Main infrastructure template with conditional APIM deployment
  - `modules/apim.bicep` — Azure API Management module for fronting the Function App

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
   - The file **MUST** begin with the standard YAML frontmatter block (see [YAML frontmatter](#yaml-frontmatter) above) and follow the required section order documented in [`.github/copilot-instructions.md`](.github/copilot-instructions.md) under "Best-practices markdown format standard".
  - These language resources are included in the project output and are available at runtime from the build output `Languages/` folder (e.g. `AppContext.BaseDirectory + "Languages/Go/go-best-practices.md"`).

2. Add an MCP tool implementation
  - Add a new C# tool class in `Languages/<Language>/<Language>.cs` named `<Language>Tools`.
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
  - Optionally run the Functions host locally (if you have Azure Functions Core Tools) and test the MCP endpoint, or validate the file-access logic by inspecting the build output under `bin/Debug/net*/Languages/`.

## Logging and instrumentation

- Logging is provided via source-generated methods defined in `Utilities/ToolLogging.cs` and consumed through `ToolLogging<T>`.
- Keep message shapes stable when adding new log calls to preserve consistency.

## Contributing

We welcome contributions! Please follow these guidelines when opening a PR:

- Formatting: Run `dotnet format BestPracticesMcp.sln` before pushing; CI will verify formatting.
- Build: Ensure `dotnet build BestPracticesMcp.sln` passes locally.
- Tests: Add unit tests for new logic where appropriate. There are no unit tests currently but adding tests is encouraged.
- Small PRs: Prefer small, focused PRs (one language/tool per PR when adding new content).
- Documentation: When adding or changing language content under `Languages/`, include a short rationale in the PR description and link authoritative sources.

## Contribution Guidelines

We welcome contributions of any size. The following guidelines help us keep changes small, reviewable, and easy to validate.

- Follow the repository formatting rules:
  - Run `dotnet format BestPracticesMcp.sln` before committing and ensure `dotnet format BestPracticesMcp.sln --verify-no-changes` passes.
- Build and validate your changes locally:
  - Ensure `dotnet build BestPracticesMcp.sln` succeeds.
  - If you add or change language resources, verify the files are copied to the build output `bin/Debug/net*/Languages/` and can be read by the tools.
- Adding a new language/tool:
  - Add a markdown resource under `Languages/<Language>/<language>-best-practices.md`. The file must include YAML frontmatter and follow the standard section order (see [`.github/copilot-instructions.md`](.github/copilot-instructions.md) for the full format specification).
  - Add the corresponding MCP tool implementation `Languages/<Language>/<Language>.cs` (expose `Get<Language>BestPractices` and use `Utilities/FileCache` and `Utilities/ToolLogging<T>`).
  - Documentation sync requirement (COPILOT): When a new language resource or MCP tool is added, update this `README.md` to include the new language in the "Included languages" list and add the resource path (e.g. `Languages/<Language>/<language>-best-practices.md`) and tool code path (e.g. `Languages/<Language>/<Language>.cs`). This README update must be included in the same PR that introduces the language.
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
