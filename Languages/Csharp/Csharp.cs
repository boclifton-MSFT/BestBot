using BestPracticesMcp.Utilities;
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
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1852:Type can be sealed", Justification = "Instantiated by Functions host via reflection; suppress analyzer to avoid sealing suggestion")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Functions host via reflection")]
internal class CsharpTools(ILogger<CsharpTools> logger)
{
    [Function(nameof(GetCsharpBestPractices))]
    public async Task<string> GetCsharpBestPractices(
        [McpToolTrigger("get_csharp_best_practices", "Retrieves best practices for the C# programming language")]
            ToolInvocationContext toolContext,
        CancellationToken cancellationToken)
    {
        ToolLogging<CsharpTools>.Serving(logger, "get_csharp_best_practices");

        var filePath = Path.Combine(AppContext.BaseDirectory, "Languages", "Csharp", "csharp-best-practices.md");

        // Fast-path: serve from cache if valid
        if (FileCache.TryGetValid(filePath, out var cached))
        {
            ToolLogging<CsharpTools>.ServingCached(logger, filePath);
            return cached!;
        }

        try
        {
            var content = await FileCache.GetOrLoadAsync(filePath, TimeSpan.FromMinutes(5), async () =>
            {
                ToolLogging<CsharpTools>.Loading(logger, filePath);
                return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            if (content is not null)
                return content;

            ToolLogging<CsharpTools>.FileNotFound(logger, filePath);
        }
        catch (OperationCanceledException)
        {
            // Propagate cancellation so the host can stop gracefully
            throw;
        }
        catch (IOException ex)
        {
            ToolLogging<CsharpTools>.FailedToLoad(logger, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ToolLogging<CsharpTools>.FailedToLoad(logger, ex);
        }
        catch (NotSupportedException ex)
        {
            ToolLogging<CsharpTools>.FailedToLoad(logger, ex);
        }

        string[] fallback = new[]
        {
            "# C# Best Practices",
            "- Prefer clarity over cleverness; name things for intent.",
            "- Keep methods small; a function should do one thing well.",
            "- Embrace async/await end-to-end; pass CancellationToken.",
            "- Validate inputs and fail fast with helpful messages.",
            "- Catch only what you can handle; preserve stack traces.",
            "- Use nullable reference types and guard against nulls.",
            "- Favor immutability (records/readonly) for data models.",
            "- Dispose resources deterministically; prefer `await using`.",
            "- Measure before optimizing; avoid premature micro-opts.",
            "- Write tests for behavior and edge cases; keep them stable."
        };
        return string.Join(Environment.NewLine, fallback);
    }
}
