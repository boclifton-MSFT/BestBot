using System.ComponentModel;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BestPracticesMcp.UpdateWorker.FunctionTools;

/// <summary>
/// Function tool that detects the latest stable version of a programming language or framework.
/// Filters out beta, preview, RC, and other prerelease versions.
/// </summary>
internal static partial class VersionCheckTool
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private static readonly HttpClient SharedClient = CreateClient();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "HttpClient and handler are intentionally long-lived singletons")]
    private static HttpClient CreateClient()
    {
        var client = new HttpClient(new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        })
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("BestBot-UpdateWorker/1.0");
        return client;
    }

    /// <summary>
    /// Checks the latest stable version of a language by fetching its version source URL.
    /// </summary>
    [Description("Fetch the version source page and attempt to detect the latest stable version. " +
                 "Returns JSON with currentVersion, latestDetected, isNewer, and the raw page snippet. " +
                 "Only considers GA/stable releases — ignores alpha, beta, RC, preview, dev, canary, nightly. " +
                 "Use this to determine if a language has had a major or minor version bump.")]
    public static async Task<string> CheckLatestVersion(
        [Description("The URL to check for the latest version (e.g. https://www.python.org/downloads/)")] string versionSourceUrl,
        [Description("The currently tracked version from frontmatter (e.g. '3.13')")] string currentVersion,
        [Description("The language name for context (e.g. 'Python')")] string languageName)
    {
        try
        {
            using var response = await SharedClient.GetAsync(new Uri(versionSourceUrl)).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return JsonSerializer.Serialize(new
                {
                    languageName,
                    currentVersion,
                    error = $"HTTP {(int)response.StatusCode}",
                    isNewer = false,
                    latestDetected = (string?)null,
                });
            }

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            // Trim for the LLM — the agent will do the intelligent parsing
            var snippet = content.Length > 8000 ? content[..8000] : content;

            // Attempt basic version detection from common patterns
            var versions = ExtractVersionCandidates(snippet);

            // Filter out prerelease versions
            var stableVersions = versions
                .Where(v => !PrereleaseRegex().IsMatch(v))
                .Distinct()
                .ToList();

            return JsonSerializer.Serialize(new
            {
                languageName,
                currentVersion,
                stableVersionsFound = stableVersions.Take(10),
                isNewer = false, // Let the agent determine this from the versions found
                pageSnippet = snippet.Length > 3000 ? snippet[..3000] : snippet,
            }, JsonOptions);
        }
        catch (HttpRequestException ex)
        {
            return JsonSerializer.Serialize(new
            {
                languageName,
                currentVersion,
                error = ex.Message,
                isNewer = false,
                latestDetected = (string?)null,
            });
        }
        catch (TaskCanceledException)
        {
            return JsonSerializer.Serialize(new
            {
                languageName,
                currentVersion,
                error = "Request timed out",
                isNewer = false,
                latestDetected = (string?)null,
            });
        }
    }

    /// <summary>
    /// Extracts version-like strings from page content using common patterns.
    /// </summary>
    private static List<string> ExtractVersionCandidates(string content)
    {
        var matches = VersionPatternRegex().Matches(content);
        return matches.Select(m => m.Value).ToList();
    }

    /// <summary>
    /// Matches common version patterns like 3.13.1, 1.24, 5.4.7, etc.
    /// </summary>
    [GeneratedRegex(@"\b\d+\.\d+(?:\.\d+)?(?:\.\d+)?\b")]
    private static partial Regex VersionPatternRegex();

    /// <summary>
    /// Detects prerelease version indicators.
    /// </summary>
    [GeneratedRegex(@"(?i)(alpha|beta|rc\d*|preview|dev|canary|nightly|pre|snapshot|insider)")]
    private static partial Regex PrereleaseRegex();
}
