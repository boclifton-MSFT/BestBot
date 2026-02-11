namespace BestPracticesMcp.UpdateWorker.Models;

/// <summary>
/// Result returned by the PrCreationAgent after creating a GitHub pull request.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by JSON deserialization from agent responses")]
internal sealed class PrCreationResult
{
    /// <summary>
    /// The HTML URL of the created pull request.
    /// </summary>
    public string PrUrl { get; set; } = string.Empty;
}
