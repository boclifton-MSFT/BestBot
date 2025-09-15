using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace BestPracticesMcp.Functions;

/// <summary>
/// Provides tools for retrieving best practices for the C# programming language.
/// Utilizes a process-wide cache to minimize disk reads and improve performance.
/// </summary>
/// <remarks>
/// This class is intended for internal use and is designed to be used within a function app context.
/// It reads best practices from a markdown file and caches the content for a short duration.
/// </remarks>
public class McpTools(ILogger<McpTools> logger)
{
    [Function(nameof(GetCsharpBestPractices))]
    public async Task<string> GetCsharpBestPractices(
        [McpToolTrigger("get_csharp_best_practices", "Retrieves best practices for the C# programming language")]
            ToolInvocationContext toolContext,
        CancellationToken cancellationToken)
    {
        ToolLogging<McpTools>.Serving(logger, "get_csharp_best_practices");

        var filePath = Path.Combine(AppContext.BaseDirectory, "Resources", "csharp-best-practices.md");

        // Fast-path: serve from cache if valid
        if (FileCache.TryGetValid(filePath, out var cached))
        {
            ToolLogging<McpTools>.ServingCached(logger, filePath);
            return cached!;
        }

        try
        {
            var content = await FileCache.GetOrLoadAsync(filePath, TimeSpan.FromMinutes(5), async () =>
            {
                ToolLogging<McpTools>.Loading(logger, filePath);
                return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            if (content is not null)
                return content;

            ToolLogging<McpTools>.FileNotFound(logger, filePath);
        }
        catch (OperationCanceledException)
        {
            // Propagate cancellation so the host can stop gracefully
            throw;
        }
        catch (IOException ex)
        {
            ToolLogging<McpTools>.FailedToLoad(logger, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ToolLogging<McpTools>.FailedToLoad(logger, ex);
        }
        catch (NotSupportedException ex)
        {
            ToolLogging<McpTools>.FailedToLoad(logger, ex);
        }

        string[] fallback = new[]
        {
            "# C# Best Practices",
            "- Use meaningful names and clear intent.",
            "- Keep methods small and single-purpose.",
            "- Handle exceptions precisely; preserve stack traces.",
            "- Embrace async/await and cancellation.",
            "- Write tests and keep them deterministic.",
            "- Follow SOLID and prefer immutability where practical."
        };
        return string.Join(Environment.NewLine, fallback);
    }
}