using BestPracticesMcp.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace BestPracticesMcp.Functions;

/// <summary>
/// Provides tools for retrieving best practices for the Go programming language.
/// Utilizes a process-wide cache to minimize disk reads and improve performance.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1852:Type can be sealed", Justification = "Instantiated by Functions host via reflection; suppress analyzer to avoid sealing suggestion")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Functions host via reflection")]
internal class GoTools(ILogger<GoTools> logger)
{
    [Function(nameof(GetGoBestPractices))]
    public async Task<string> GetGoBestPractices(
        [McpToolTrigger("get_go_best_practices", "Retrieves best practices for the Go programming language")]
            ToolInvocationContext toolContext, CancellationToken cancellationToken)
    {
        ToolLogging<GoTools>.Serving(logger, "get_go_best_practices");

        var filePath = Path.Combine(AppContext.BaseDirectory, "Languages", "Go", "go-best-practices.md");

        if (FileCache.TryGetValid(filePath, out var cached))
        {
            ToolLogging<GoTools>.ServingCached(logger, filePath);
            return cached!;
        }

        try
        {
            var content = await FileCache.GetOrLoadAsync(filePath, TimeSpan.FromMinutes(5), async () =>
            {
                ToolLogging<GoTools>.Loading(logger, filePath);
                return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            if (content is not null)
                return content;

            ToolLogging<GoTools>.FileNotFound(logger, filePath);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (IOException ex)
        {
            ToolLogging<GoTools>.FailedToLoad(logger, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ToolLogging<GoTools>.FailedToLoad(logger, ex);
        }
        catch (NotSupportedException ex)
        {
            ToolLogging<GoTools>.FailedToLoad(logger, ex);
        }

        string[] fallback = new[]
        {
            "# Go Best Practices",
            "- Use gofmt for consistent formatting; enforce in CI.",
            "- Handle errors explicitly; avoid ignoring error returns.",
            "- Use go mod for dependency management and versioning.",
            "- Write idiomatic Go: prefer channels over shared memory for concurrency.",
            "- Keep packages focused; follow the single responsibility principle.",
            "- Use context.Context for cancellation and timeouts.",
            "- Test with go test; aim for good coverage of critical paths."
        };
        return string.Join(Environment.NewLine, fallback);
    }
}