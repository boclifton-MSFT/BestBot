using BestPracticesMcp.UpdateWorker.Models;
using BestPracticesMcp.UpdateWorker.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BestPracticesMcp.UpdateWorker;

/// <summary>
/// Timer-triggered function that runs weekly (Mondays at 2 AM UTC) to check
/// all best-practices documents for staleness and version changes.
/// Starts a Durable orchestration that fans out across all languages.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Functions host via reflection")]
internal sealed class UpdateTimerTrigger(
    IOptions<UpdateWorkerOptions> options,
    ILogger<UpdateTimerTrigger> logger)
{
    private readonly UpdateWorkerOptions _options = options.Value;

    [Function(nameof(WeeklyUpdateCheck))]
    public async Task WeeklyUpdateCheck(
        [TimerTrigger("0 0 2 * * 1")] TimerInfo timer,
        [DurableClient] DurableTaskClient durableClient,
        CancellationToken cancellationToken)
    {
        UpdateWorkerLogging.TimerTriggered(logger, DateTimeOffset.UtcNow);

        if (!_options.Enabled)
        {
            UpdateWorkerLogging.WorkerDisabled(logger);
            return;
        }

        // Discover all language best-practices files
        var languagesDir = Path.Combine(AppContext.BaseDirectory, "Languages");
        var inputs = new List<LanguageCheckInput>();

        if (!Directory.Exists(languagesDir))
        {
            UpdateWorkerLogging.LanguagesDirectoryNotFound(logger, languagesDir);
            return;
        }

        foreach (var langDir in Directory.GetDirectories(languagesDir))
        {
            var langName = Path.GetFileName(langDir);
            var mdFiles = Directory.GetFiles(langDir, "*-best-practices.md");

            foreach (var mdFile in mdFiles)
            {
                var content = await File.ReadAllTextAsync(mdFile, cancellationToken).ConfigureAwait(false);
                inputs.Add(new LanguageCheckInput
                {
                    LanguageName = langName,
                    FilePath = mdFile,
                    CurrentContent = content,
                });
            }
        }

        UpdateWorkerLogging.LanguagesDiscovered(logger, inputs.Count);

        if (inputs.Count == 0)
        {
            UpdateWorkerLogging.NoLanguageFilesFound(logger);
            return;
        }

        var orchestrationInput = new LanguageUpdateOrchestrationInput
        {
            Languages = inputs,
            MaxParallelAgentRuns = Math.Max(1, _options.MaxParallelAgentRuns),
            EstimatedCharsPerToken = Math.Clamp(_options.EstimatedCharsPerToken, 1, 12),
            TriggerTimeUtc = DateTimeOffset.UtcNow,
        };

        // Start the durable orchestration
        var instanceId = await durableClient.ScheduleNewOrchestrationInstanceAsync(
            nameof(UpdateOrchestrator.OrchestrateLanguageUpdates),
            orchestrationInput,
            cancellationToken).ConfigureAwait(false);

        UpdateWorkerLogging.OrchestrationInstanceStarted(logger, instanceId);
    }
}
