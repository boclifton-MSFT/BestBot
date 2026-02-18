using System.ComponentModel;
using System.Net;
using System.Text.Json;

namespace BestPracticesMcp.UpdateWorker.FunctionTools;

/// <summary>
/// Function tool that checks resource URLs for availability and content changes.
/// The agent calls this tool to validate that documentation references are still accessible.
/// </summary>
internal static class ResourceCheckTool
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private static readonly HttpClient SharedClient = CreateClient();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "HttpClient and handler are intentionally long-lived singletons")]
    private static HttpClient CreateClient()
    {
        var client = new HttpClient(new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            MaxConnectionsPerServer = 10,
        })
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("BestBot-UpdateWorker/1.0");
        return client;
    }

    /// <summary>
    /// Checks a list of URLs for availability. Returns a JSON array with status for each URL.
    /// </summary>
    [Description("Check a list of resource URLs for availability. Returns JSON with url, statusCode, isAccessible, and contentSnippet (first 500 chars) for each URL. Use this to verify documentation references are still live.")]
    public static async Task<string> CheckResourceUrls(
        [Description("Comma-separated list of URLs to check")] string urls)
    {
        var urlList = urls.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var results = new List<object>();

        foreach (var url in urlList)
        {
            try
            {
                using var response = await SharedClient.GetAsync(new Uri(url), HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                var statusCode = (int)response.StatusCode;
                var isAccessible = response.IsSuccessStatusCode;
                var contentSnippet = string.Empty;

                if (isAccessible)
                {
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    contentSnippet = content.Length > 500 ? content[..500] : content;
                }

                results.Add(new { url, statusCode, isAccessible, contentSnippet });
            }
            catch (UriFormatException ex)
            {
                results.Add(new { url, statusCode = 0, isAccessible = false, contentSnippet = $"Error: Invalid URL format - {ex.Message}" });
            }
            catch (HttpRequestException ex)
            {
                results.Add(new { url, statusCode = 0, isAccessible = false, contentSnippet = $"Error: {ex.Message}" });
            }
            catch (TaskCanceledException)
            {
                results.Add(new { url, statusCode = 0, isAccessible = false, contentSnippet = "Error: Request timed out" });
            }
        }

        return JsonSerializer.Serialize(results, JsonOptions);
    }

    /// <summary>
    /// Fetches the full content of a single URL for deeper analysis.
    /// </summary>
    [Description("Fetch the full content of a single URL for detailed analysis. Returns the text content of the page (up to 10000 characters). Use when you need to analyze page content for version information or significant changes.")]
    public static async Task<string> FetchUrlContent(
        [Description("The URL to fetch content from")] string url)
    {
        try
        {
            using var response = await SharedClient.GetAsync(new Uri(url)).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return $"Error: HTTP {(int)response.StatusCode} {response.StatusCode}";
            }

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return content.Length > 10000 ? content[..10000] : content;
        }
        catch (UriFormatException ex)
        {
            return $"Error: Invalid URL format - {ex.Message}";
        }
        catch (HttpRequestException ex)
        {
            return $"Error: {ex.Message}";
        }
        catch (TaskCanceledException)
        {
            return "Error: Request timed out";
        }
    }
}
