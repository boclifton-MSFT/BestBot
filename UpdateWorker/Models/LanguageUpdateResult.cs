namespace BestPracticesMcp.UpdateWorker.Models;

/// <summary>
/// Output DTO from each language agent invocation.
/// Contains the proposed updated content and a summary of what changed.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by JSON deserialization from agent responses")]
internal sealed class LanguageUpdateResult
{
    /// <summary>
    /// The language that was evaluated.
    /// </summary>
    public string LanguageName { get; set; } = string.Empty;

    /// <summary>
    /// Whether the agent determined the document needs updating.
    /// </summary>
    public bool NeedsUpdate { get; set; }

    /// <summary>
    /// The complete updated markdown content (including frontmatter) if <see cref="NeedsUpdate"/> is true.
    /// Empty string if no update is needed.
    /// </summary>
    public string UpdatedContent { get; set; } = string.Empty;

    /// <summary>
    /// A human-readable summary of what changed and why the update was proposed.
    /// </summary>
    public string ChangeSummary { get; set; } = string.Empty;

    /// <summary>
    /// The file path for the best-practices markdown file.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
}
