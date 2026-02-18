using System.Text;
using System.Text.RegularExpressions;
using BestPracticesMcp.UpdateWorker.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace BestPracticesMcp.UpdateWorker.Services;

/// <summary>
/// Parses and writes YAML frontmatter in best-practices markdown files.
/// Frontmatter is delimited by "---" fences at the top of the file.
/// </summary>
internal static partial class FrontmatterParser
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    private static readonly ISerializer Serializer = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .DisableAliases()
        .Build();

    /// <summary>
    /// Parses YAML frontmatter from the given markdown content.
    /// Returns the metadata and the body content (everything after the closing "---").
    /// </summary>
    public static (FrontmatterMetadata Metadata, string Body) Parse(string markdownContent)
    {
        ArgumentNullException.ThrowIfNull(markdownContent);

        var match = FrontmatterRegex().Match(markdownContent);
        if (!match.Success)
        {
            return (new FrontmatterMetadata(), markdownContent);
        }

        var yamlContent = match.Groups["yaml"].Value;
        var body = markdownContent[(match.Index + match.Length)..].TrimStart('\r', '\n');

        var metadata = Deserializer.Deserialize<FrontmatterMetadata>(yamlContent) ?? new FrontmatterMetadata();
        return (metadata, body);
    }

    /// <summary>
    /// Writes YAML frontmatter to the beginning of the given body content.
    /// </summary>
    public static string Write(FrontmatterMetadata metadata, string body)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(body);

        var sb = new StringBuilder();
        sb.AppendLine("---");

        var yaml = Serializer.Serialize(metadata).TrimEnd();
        sb.AppendLine(yaml);
        sb.AppendLine("---");
        sb.AppendLine();
        sb.Append(body);

        return sb.ToString();
    }

    /// <summary>
    /// Extracts all URLs from the ## Resources section of a markdown document.
    /// </summary>
    public static List<string> ExtractResourceUrls(string markdownContent)
    {
        var urls = new List<string>();

        // Find the ## Resources section
        var resourcesMatch = ResourcesSectionRegex().Match(markdownContent);
        if (!resourcesMatch.Success)
        {
            return urls;
        }

        var resourcesSection = markdownContent[resourcesMatch.Index..];

        // Find the next ## heading (if any) to bound the resources section
        var nextSectionMatch = NextSectionRegex().Match(resourcesSection, resourcesMatch.Length);
        if (nextSectionMatch.Success)
        {
            resourcesSection = resourcesSection[..nextSectionMatch.Index];
        }

        // Extract URLs from the resources section
        foreach (Match urlMatch in UrlRegex().Matches(resourcesSection))
        {
            urls.Add(urlMatch.Value);
        }

        return urls;
    }

    [GeneratedRegex(@"^---\s*\n(?<yaml>.*?)\n---\s*\n", RegexOptions.Singleline)]
    private static partial Regex FrontmatterRegex();

    [GeneratedRegex(@"^## Resources", RegexOptions.Multiline)]
    private static partial Regex ResourcesSectionRegex();

    [GeneratedRegex(@"^## ", RegexOptions.Multiline)]
    private static partial Regex NextSectionRegex();

    [GeneratedRegex(@"https?://[^\s\)>]+")]
    private static partial Regex UrlRegex();
}
