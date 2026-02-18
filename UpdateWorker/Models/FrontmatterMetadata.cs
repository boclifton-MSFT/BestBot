namespace BestPracticesMcp.UpdateWorker.Models;

/// <summary>
/// Represents the YAML frontmatter metadata embedded in each best-practices markdown file.
/// Used by the update worker to track language versions, content hashes, and check dates.
/// </summary>
internal sealed class FrontmatterMetadata
{
    /// <summary>
    /// The human-readable display name of the language or framework.
    /// Example: "C#", "Python", "Vue 3", ".NET Aspire".
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// The current stable major.minor version of the language or framework.
    /// Example: "3.13" for Python, "1.24" for Go, "ES2025" for JavaScript.
    /// </summary>
    public string LanguageVersion { get; set; } = string.Empty;

    /// <summary>
    /// ISO date of the last automated check (e.g. "2026-02-11").
    /// </summary>
    public string LastChecked { get; set; } = string.Empty;

    /// <summary>
    /// SHA-256 hash of the concatenated resource URL response content.
    /// Used for change detection without external state.
    /// </summary>
    public string ResourceHash { get; set; } = string.Empty;

    /// <summary>
    /// The canonical URL to scrape for the latest stable release of this language.
    /// </summary>
    public string VersionSourceUrl { get; set; } = string.Empty;
}
