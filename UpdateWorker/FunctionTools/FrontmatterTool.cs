using System.ComponentModel;
using BestPracticesMcp.UpdateWorker.Models;
using BestPracticesMcp.UpdateWorker.Services;

namespace BestPracticesMcp.UpdateWorker.FunctionTools;

/// <summary>
/// Function tool for reading and interpreting YAML frontmatter from best-practices markdown files.
/// </summary>
internal static class FrontmatterTool
{
    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <summary>
    /// Reads and returns the YAML frontmatter metadata from a markdown document.
    /// </summary>
    [Description("Parse the YAML frontmatter from a best-practices markdown document. " +
                 "Returns the language_version, last_checked, resource_hash, and version_source_url. " +
                 "Also returns the list of resource URLs found in the ## Resources section.")]
    public static string ReadFrontmatter(
        [Description("The full markdown content of the best-practices file")] string markdownContent)
    {
        var (metadata, body) = FrontmatterParser.Parse(markdownContent);
        var resourceUrls = FrontmatterParser.ExtractResourceUrls(markdownContent);

        return System.Text.Json.JsonSerializer.Serialize(new
        {
            languageVersion = metadata.LanguageVersion,
            lastChecked = metadata.LastChecked,
            resourceHash = metadata.ResourceHash,
            versionSourceUrl = metadata.VersionSourceUrl,
            resourceUrls,
            bodyLineCount = body.Split('\n').Length,
        }, JsonOptions);
    }
}
