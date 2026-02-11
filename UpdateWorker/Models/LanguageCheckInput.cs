namespace BestPracticesMcp.UpdateWorker.Models;

/// <summary>
/// Input DTO for per-language agent invocation during the update orchestration.
/// Contains all the information the agent needs to evaluate a single language document.
/// </summary>
internal sealed class LanguageCheckInput
{
    /// <summary>
    /// Display name of the language (e.g. "Python", "Go", "Csharp").
    /// </summary>
    public string LanguageName { get; set; } = string.Empty;

    /// <summary>
    /// Absolute path to the best-practices markdown file.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// The full current content of the markdown file (including frontmatter).
    /// </summary>
    public string CurrentContent { get; set; } = string.Empty;
}
