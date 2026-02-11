using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;

namespace BestPracticesMcp.UpdateWorker.FunctionTools;

/// <summary>
/// Function tool for computing and comparing SHA-256 content hashes.
/// Used to detect whether resource content has changed since the last check.
/// </summary>
internal static class ContentHashTool
{
    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    /// <summary>
    /// Computes a SHA-256 hash of the provided content and compares it against a stored hash.
    /// </summary>
    [Description("Compute a SHA-256 hash of the provided content and compare it against a previously stored hash. " +
                 "Returns the new hash, the old hash, and whether they differ. " +
                 "Use this after fetching resource URL content to detect changes since the last check.")]
    public static string CompareContentHash(
        [Description("The content to hash (typically concatenated resource page content)")] string content,
        [Description("The previously stored hash from frontmatter (empty string if none)")] string storedHash)
    {
        var newHash = ComputeHash(content);
        var hasChanged = !string.IsNullOrEmpty(storedHash)
            && !string.Equals(storedHash, newHash, StringComparison.OrdinalIgnoreCase);

        return System.Text.Json.JsonSerializer.Serialize(new
        {
            newHash,
            storedHash,
            hasChanged,
            isFirstCheck = string.IsNullOrEmpty(storedHash),
        }, JsonOptions);
    }

    /// <summary>
    /// Computes a SHA-256 hash of the given string content.
    /// </summary>
    private static string ComputeHash(string content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexStringLower(bytes);
    }
}
