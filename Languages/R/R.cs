using BestPracticesMcp.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace BestPracticesMcp.Functions;

/// <summary>
/// Provides tools for retrieving best practices for the R programming language.
/// Utilizes a process-wide cache to minimize disk reads and improve performance.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1852:Type can be sealed", Justification = "Instantiated by Functions host via reflection; suppress analyzer to avoid sealing suggestion")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Functions host via reflection")]
internal class RTools(ILogger<RTools> logger)
{
    [Function(nameof(GetRBestPractices))]
    public async Task<string> GetRBestPractices(
        [McpToolTrigger("get_r_best_practices", "Retrieves best practices for the R programming language")]
            ToolInvocationContext toolContext, CancellationToken cancellationToken)
    {
        ToolLogging<RTools>.Serving(logger, "get_r_best_practices");

        var filePath = Path.Combine(AppContext.BaseDirectory, "Languages", "R", "r-best-practices.md");

        if (FileCache.TryGetValid(filePath, out var cached))
        {
            ToolLogging<RTools>.ServingCached(logger, filePath);
            return cached!;
        }

        try
        {
            var content = await FileCache.GetOrLoadAsync(filePath, TimeSpan.FromMinutes(5), async () =>
            {
                ToolLogging<RTools>.Loading(logger, filePath);
                return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            if (content is not null)
                return content;

            ToolLogging<RTools>.FileNotFound(logger, filePath);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (IOException ex)
        {
            ToolLogging<RTools>.FailedToLoad(logger, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ToolLogging<RTools>.FailedToLoad(logger, ex);
        }
        catch (NotSupportedException ex)
        {
            ToolLogging<RTools>.FailedToLoad(logger, ex);
        }

        string[] fallback = new[]
        {
            "# R Best Practices",
            "- Use consistent naming conventions; prefer snake_case for variables and functions.",
            "- Keep functions small and focused; avoid deep nesting.",
            "- Use vectorized operations instead of explicit loops when possible.",
            "- Document functions with roxygen2 comments for clarity and reusability.",
            "- Handle missing values (NA) explicitly and consistently.",
            "- Use packages from CRAN or Bioconductor; pin versions for reproducibility.",
            "- Write unit tests with testthat; test edge cases and error conditions.",
            "- Use projects and renv for dependency management.",
            "- Avoid global variables; pass parameters explicitly.",
            "- Profile code before optimizing; measure performance impact."
        };
        return string.Join(Environment.NewLine, fallback);
    }
}