namespace BestPracticesMcp.UpdateWorker.Models;

/// <summary>
/// Input DTO for the language update orchestration.
/// Carries language work items plus orchestration-level execution limits.
/// </summary>
internal sealed class LanguageUpdateOrchestrationInput
{
    /// <summary>
    /// Per-language update inputs discovered by the timer trigger.
    /// </summary>
    public List<LanguageCheckInput> Languages { get; set; } = [];

    /// <summary>
    /// Maximum number of language agent sessions to run in parallel.
    /// </summary>
    public int MaxParallelAgentRuns { get; set; } = 2;

    /// <summary>
    /// Approximate characters-per-token ratio used for prompt budget estimates.
    /// </summary>
    public int EstimatedCharsPerToken { get; set; } = 4;
}