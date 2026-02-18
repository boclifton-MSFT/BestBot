using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using BestPracticesMcp.UpdateWorker;
using BestPracticesMcp.UpdateWorker.FunctionTools;
using BestPracticesMcp.UpdateWorker.Models;
using BestPracticesMcp.UpdateWorker.Services;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.AzureFunctions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Register UpdateWorker configuration
builder.Services.Configure<UpdateWorkerOptions>(
    builder.Configuration.GetSection(UpdateWorkerOptions.SectionName));

builder.Services.AddSingleton<AzureOpenAIClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<UpdateWorkerOptions>>().Value;
    string endpoint = options.AzureOpenAIEndpoint;

    if (string.IsNullOrWhiteSpace(endpoint))
    {
        throw new InvalidOperationException("UpdateWorker:AzureOpenAIEndpoint configuration is not set.");
    }

    string apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY") ?? string.Empty;

    // if apiKey is empty, use DefaultAzureCredential, otherwise use AzureKeyCredential
    return string.IsNullOrEmpty(apiKey)
        ? new AzureOpenAIClient(
            new Uri(endpoint),
            new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ExcludeVisualStudioCredential = true,
            }))
        : new AzureOpenAIClient(
            new Uri(endpoint),
            new AzureKeyCredential(apiKey));
});

// Register the GitHub MCP client for PR creation via remote MCP server
builder.Services.AddSingleton<GithubMcpClient>();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Check if update worker is enabled and OpenAI is configured
var config = builder.Configuration.GetSection(UpdateWorkerOptions.SectionName).Get<UpdateWorkerOptions>();
bool updateWorkerEnabled = config?.Enabled ?? true; // Default to true if not configured
string? openAIEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");

// Only configure OpenAI and agents if the update worker is enabled
if (updateWorkerEnabled && !string.IsNullOrEmpty(openAIEndpoint))
{
    // Register AzureOpenAIClient
    builder.Services.AddSingleton<AzureOpenAIClient>(sp =>
    {
        string endpoint = openAIEndpoint!;
        string apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY") ?? string.Empty;

        // if apiKey is empty, use DefaultAzureCredential, otherwise use AzureKeyCredential
        return string.IsNullOrEmpty(apiKey)
            ? new AzureOpenAIClient(
                new Uri(endpoint),
                new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    ExcludeVisualStudioCredential = true,
                }))
            : new AzureOpenAIClient(
                new Uri(endpoint),
                new AzureKeyCredential(apiKey));
    });

    // Register the GitHub MCP client for PR creation via remote MCP server
    builder.Services.AddSingleton<GithubMcpClient>();

    // Build the AzureOpenAIClient to create the agent
    var serviceProvider = builder.Services.BuildServiceProvider();
    var openAIClient = serviceProvider.GetRequiredService<AzureOpenAIClient>();
    string deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? "gpt-4o";

    // Initialize the GitHub MCP client and filter to only the 3 tools the PrCreationAgent needs.
    // The repos + pull_requests toolsets expose 30+ tools; passing all of them bloats the
    // chat-completion request and can trigger 400 errors from Azure OpenAI.
    var githubMcpClient = serviceProvider.GetRequiredService<GithubMcpClient>();
    var requiredGithubTools = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "create_branch",        // repos toolset – create the auto-update branch
        "push_files",           // repos toolset – push updated files in a single commit
        "create_pull_request",  // pull_requests toolset – open the PR
    };
    var githubMcpTools = (await githubMcpClient.GetToolsAsync().ConfigureAwait(false))
        .Where(t => requiredGithubTools.Contains(t.Name))
        .Cast<AITool>().ToList();

    // Local function tools for the LanguageUpdateAgent (no GitHub MCP tools needed)
    var updateAgentTools = new List<AITool>
    {
        AIFunctionFactory.Create(ResourceCheckTool.CheckResourceUrls),
        AIFunctionFactory.Create(ResourceCheckTool.FetchUrlContent),
        AIFunctionFactory.Create(VersionCheckTool.CheckLatestVersion),
        AIFunctionFactory.Create(FrontmatterTool.ReadFrontmatter),
        AIFunctionFactory.Create(ContentHashTool.CompareContentHash),
    };

    // Read repo config for agent instructions
    string repoOwner = Environment.GetEnvironmentVariable("UpdateWorker__GitHubRepoOwner") ?? "boclifton-MSFT";
    string repoName = Environment.GetEnvironmentVariable("UpdateWorker__GitHubRepoName") ?? "BestBot";
    string defaultBranch = Environment.GetEnvironmentVariable("UpdateWorker__DefaultBranch") ?? "main";

    // Create the LanguageUpdateAgent with only its own function tools
    AIAgent updateAgent = AgentDefinitions.CreateUpdateAgent(openAIClient, new UpdateAgentConfig(deploymentName, updateAgentTools));

    // Create the PrCreationAgent which uses GitHub MCP tools to create PRs
    AIAgent prAgent = AgentDefinitions.CreateGithubAgent(
        openAIClient,
        new GithubAgentConfig(deploymentName, repoOwner, repoName, defaultBranch, githubMcpTools)
    );

    builder.ConfigureDurableAgents(options =>
    {
        options.AddAIAgent(updateAgent);
        options.AddAIAgent(prAgent);
    });
}
else
{
    // Log why OpenAI client and agents are not being configured
    var tempServiceProvider = builder.Services.BuildServiceProvider();
    var logger = tempServiceProvider.GetRequiredService<ILogger<Program>>();

    if (!updateWorkerEnabled)
    {
        UpdateWorkerLogging.UpdateWorkerDisabledNoOpenAI(logger);
    }
    else if (string.IsNullOrEmpty(openAIEndpoint))
    {
        UpdateWorkerLogging.OpenAIEndpointNotSet(logger);
    }
}

await builder.Build().RunAsync().ConfigureAwait(false);
