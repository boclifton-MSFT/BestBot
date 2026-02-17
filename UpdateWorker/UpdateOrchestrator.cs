using BestPracticesMcp.UpdateWorker.Models;
using BestPracticesMcp.UpdateWorker.Services;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DurableTask;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace BestPracticesMcp.UpdateWorker;

/// <summary>
/// Durable orchestrator that fans out across all language documents,
/// creating a parallel DurableAIAgent session for each language.
/// Each agent autonomously checks resource URLs, detects version changes,
/// and evaluates whether the best-practices document needs updating.
/// When updates are found, a PrCreationAgent uses GitHub MCP tools to open a PR.
/// </summary>
internal static class UpdateOrchestrator
{
    private const string AgentName = "LanguageUpdateAgent";
    private const string PrAgentName = "PrCreationAgent";

    [Function(nameof(OrchestrateLanguageUpdates))]
    public static async Task<List<LanguageUpdateResult>> OrchestrateLanguageUpdates(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var request = context.GetInput<LanguageUpdateOrchestrationInput>() ?? new LanguageUpdateOrchestrationInput();
        var inputs = request.Languages;
        var logger = context.CreateReplaySafeLogger(nameof(UpdateOrchestrator));
        var maxParallelRuns = Math.Max(1, request.MaxParallelAgentRuns);
        var charsPerToken = Math.Clamp(request.EstimatedCharsPerToken, 1, 12);

        UpdateWorkerLogging.OrchestrationStarted(logger, inputs.Count);
        UpdateWorkerLogging.OrchestrationConfig(logger, maxParallelRuns, charsPerToken);

        // Fan-out in bounded batches to reduce token-per-minute pressure.
        var agent = context.GetAgent(AgentName);
        var results = new List<LanguageUpdateResult>(inputs.Count);
        var totalPromptChars = 0;

        for (var batchStart = 0; batchStart < inputs.Count; batchStart += maxParallelRuns)
        {
            var batchSize = Math.Min(maxParallelRuns, inputs.Count - batchStart);
            var agentTasks = new List<Task<AgentResponse<LanguageUpdateResult>>>(batchSize);
            var batchInputs = new List<LanguageCheckInput>(batchSize);

            for (var i = 0; i < batchSize; i++)
            {
                var input = inputs[batchStart + i];
                UpdateWorkerLogging.AgentSessionStarted(logger, input.LanguageName);

                var session = await agent.CreateSessionAsync().ConfigureAwait(true);

                var prompt = BuildLanguagePrompt(input);
                var promptChars = prompt.Length;
                totalPromptChars += promptChars;

                UpdateWorkerLogging.AgentPromptEstimate(
                    logger,
                    input.LanguageName,
                    promptChars,
                    EstimateTokens(promptChars, charsPerToken));

                var task = agent.RunAsync<LanguageUpdateResult>(
                    message: prompt,
                    session: session);

                agentTasks.Add(task);
                batchInputs.Add(input);
            }

            await Task.WhenAll(agentTasks).ConfigureAwait(true);

            for (var i = 0; i < agentTasks.Count; i++)
            {
                var agentResponse = await agentTasks[i].ConfigureAwait(true);
                var result = agentResponse.Result;
                var input = batchInputs[i];

                // Ensure language name and file path are populated
                result.LanguageName = input.LanguageName;
                result.FilePath = input.FilePath;

                UpdateWorkerLogging.AgentSessionCompleted(logger, result.LanguageName, result.NeedsUpdate);
                results.Add(result);
            }
        }

        UpdateWorkerLogging.OrchestrationPromptEstimate(
            logger,
            totalPromptChars,
            EstimateTokens(totalPromptChars, charsPerToken),
            inputs.Count);

        // Filter to only languages that need updates
        var updatesNeeded = results.Where(r => r.NeedsUpdate).ToList();

        UpdateWorkerLogging.OrchestrationCompleted(logger, updatesNeeded.Count, results.Count);

        // If any updates are needed, use the PrCreationAgent to create a GitHub PR via MCP tools
        if (updatesNeeded.Count > 0)
        {
            var prAgent = context.GetAgent(PrAgentName);
            var prSession = await prAgent.CreateSessionAsync().ConfigureAwait(true);

            var prPrompt = BuildPrPrompt(updatesNeeded);
            UpdateWorkerLogging.PrPromptEstimate(logger, prPrompt.Length, EstimateTokens(prPrompt.Length, charsPerToken));

            var prResponse = await prAgent.RunAsync<PrCreationResult>(
                message: prPrompt,
                session: prSession).ConfigureAwait(true);

            UpdateWorkerLogging.PrCreatedAtUrl(logger, prResponse.Result.PrUrl);
        }

        return results;
    }

    private static int EstimateTokens(int chars, int charsPerToken)
    {
        if (chars <= 0)
        {
            return 0;
        }

        return (int)Math.Ceiling(chars / (double)Math.Max(1, charsPerToken));
    }

    /// <summary>
    /// Builds a detailed prompt for the agent to evaluate a specific language's best-practices document.
    /// </summary>
    private static string BuildLanguagePrompt(LanguageCheckInput input)
    {
        return $"""
            Evaluate the following best-practices document for the "{input.LanguageName}" language/framework.

            ## Instructions

            1. First, use the ReadFrontmatter tool to parse the document's YAML frontmatter and extract:
               - The current language_version
               - The last_checked date
               - The resource_hash
               - The version_source_url
               - All resource URLs from the ## Resources section

            2. Use CheckResourceUrls to verify all resource URLs are still accessible. Note any that return errors.

            3. Use CheckLatestVersion with the version_source_url and current language_version to detect
               if a new stable version has been released. Only consider GA/stable releases.
               IGNORE: alpha, beta, RC, preview, dev, canary, nightly, pre-release versions.

            4. If resource URLs have changed content, use FetchUrlContent to get the full page and
               CompareContentHash to detect content drift from the stored resource_hash.

            5. Based on your findings, determine if the document needs updating:
               - If all URLs are accessible, no version bump, and content hashes match → no update needed.
               - If URLs are broken, content has changed significantly, or a major/minor version bump occurred →
                 produce an updated document.

            6. If an update IS needed, produce a COMPLETE updated markdown document that:
               - Preserves the existing structure, sections, and formatting conventions
               - Updates the YAML frontmatter with new language_version, today's date as last_checked, new resource_hash
               - Fixes any broken URLs
               - Incorporates any significant new best practices from the version update
               - Does NOT add information about beta, preview, or prerelease features
               - Maintains the same approximate document length (100-250 lines for body)

            7. Return your response as a JSON object with these fields:
               - "languageName": the language name
               - "needsUpdate": true/false
               - "updatedContent": the complete updated markdown (empty string if no update)
               - "changeSummary": explanation of what changed and why (or "No changes needed")
               - "filePath": "{input.FilePath}"

            ## Current Document Content

            ```markdown
            {input.CurrentContent}
            ```
            """;
    }

    /// <summary>
    /// Builds a prompt for the PrCreationAgent with all update details.
    /// </summary>
    private static string BuildPrPrompt(List<LanguageUpdateResult> updates)
    {
        var dateStamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        var sb = new System.Text.StringBuilder();

        sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"Create a pull request for the following {updates.Count} language updates dated {dateStamp}.");
        sb.AppendLine();
        sb.AppendLine("## Files to update");
        sb.AppendLine();

        foreach (var update in updates.Where(u => u.NeedsUpdate && !string.IsNullOrEmpty(u.UpdatedContent)))
        {
            // Convert absolute path to repository-relative path
            var relativePath = GetRelativePath(update.FilePath);

            sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"### {update.LanguageName}");
            sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"- File path: `{relativePath}`");
            sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"- Change summary: {update.ChangeSummary}");
            sb.AppendLine("- Updated content:");
            sb.AppendLine("```markdown");
            sb.AppendLine(update.UpdatedContent);
            sb.AppendLine("```");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts an absolute file path to a repository-relative path.
    /// </summary>
    private static string GetRelativePath(string filePath)
    {
        var index = filePath.IndexOf("Languages", StringComparison.OrdinalIgnoreCase);
        if (index >= 0)
        {
            return filePath[index..].Replace('\\', '/');
        }

        return filePath.Replace('\\', '/');
    }
}
