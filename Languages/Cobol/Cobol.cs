using BestPracticesMcp.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace BestPracticesMcp.Functions;

/// <summary>
/// Provides tools for retrieving best practices for the COBOL programming language.
/// Utilizes a process-wide cache to minimize disk reads and improve performance.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1852:Type can be sealed", Justification = "Instantiated by Functions host via reflection; suppress analyzer to avoid sealing suggestion")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Functions host via reflection")]
internal class CobolTools(ILogger<CobolTools> logger)
{
    [Function(nameof(GetCobolBestPractices))]
    public async Task<string> GetCobolBestPractices(
        [McpToolTrigger("get_cobol_best_practices", "Retrieves best practices for the COBOL programming language")]
            ToolInvocationContext toolContext, CancellationToken cancellationToken)
    {
        ToolLogging<CobolTools>.Serving(logger, "get_cobol_best_practices");

        var filePath = Path.Combine(AppContext.BaseDirectory, "Languages", "Cobol", "cobol-best-practices.md");

        if (FileCache.TryGetValid(filePath, out var cached))
        {
            ToolLogging<CobolTools>.ServingCached(logger, filePath);
            return cached!;
        }

        try
        {
            var content = await FileCache.GetOrLoadAsync(filePath, TimeSpan.FromMinutes(5), async () =>
            {
                ToolLogging<CobolTools>.Loading(logger, filePath);
                return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            if (content is not null)
                return content;

            ToolLogging<CobolTools>.FileNotFound(logger, filePath);
        }
        catch (OperationCanceledException)
        {
            // Propagate cancellation so the host can stop gracefully
            throw;
        }
        catch (IOException ex)
        {
            ToolLogging<CobolTools>.FailedToLoad(logger, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ToolLogging<CobolTools>.FailedToLoad(logger, ex);
        }
        catch (NotSupportedException ex)
        {
            ToolLogging<CobolTools>.FailedToLoad(logger, ex);
        }

        // Fallback best practices if file cannot be loaded
        string[] fallback = new[]
        {
            "# COBOL Best Practices",
            "- Follow enterprise naming conventions: meaningful data names with appropriate prefixes and hyphen-separated words.",
            "- Use structured programming constructs; avoid ALTER and GO TO when possible.",
            "- Leverage modern COBOL features (EVALUATE, inline PERFORM, object-oriented COBOL when appropriate).",
            "- Keep paragraphs focused on single responsibilities; use modular design.",
            "- Use copybooks for shared data definitions and reduce duplication.",
            "- Write clear, descriptive PROCEDURE DIVISION section headers.",
            "- Test programs thoroughly; include unit tests for critical business logic.",
            "- Document complex business rules and calculations inline with meaningful comments.",
            "- Use file status codes and proper error handling for all I/O operations."
        };
        return string.Join(Environment.NewLine, fallback);
    }
}
