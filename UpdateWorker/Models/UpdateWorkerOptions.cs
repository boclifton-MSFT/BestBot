namespace BestPracticesMcp.UpdateWorker.Models;

/// <summary>
/// Configuration options for the update worker, bound from the "UpdateWorker" configuration section.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by configuration binding")]
internal sealed class UpdateWorkerOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "UpdateWorker";

    /// <summary>
    /// Azure OpenAI resource endpoint (e.g. "https://myresource.openai.azure.com/").
    /// </summary>
    public string AzureOpenAIEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Azure OpenAI deployment/model name (e.g. "gpt-4o").
    /// </summary>
    public string AzureOpenAIDeployment { get; set; } = "gpt-4o";

    /// <summary>
    /// GitHub Personal Access Token (or Key Vault reference) for PR creation.
    /// </summary>
    public string GitHubToken { get; set; } = string.Empty;

    /// <summary>
    /// GitHub repository owner (e.g. "boclifton-MSFT").
    /// </summary>
    public string GitHubRepoOwner { get; set; } = string.Empty;

    /// <summary>
    /// GitHub repository name (e.g. "BestBot").
    /// </summary>
    public string GitHubRepoName { get; set; } = string.Empty;

    /// <summary>
    /// Whether the update worker is enabled. Set to false to disable scheduled runs.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// The default branch to create PRs against (e.g. "main").
    /// </summary>
    public string DefaultBranch { get; set; } = "main";

    /// <summary>
    /// Maximum number of language agent sessions to run concurrently per orchestration.
    /// Lower values reduce token-per-minute pressure on the model deployment.
    /// </summary>
    public int MaxParallelAgentRuns { get; set; } = 2;

    /// <summary>
    /// Approximate number of characters per token used for lightweight token-budget estimation.
    /// </summary>
    public int EstimatedCharsPerToken { get; set; } = 4;
}
