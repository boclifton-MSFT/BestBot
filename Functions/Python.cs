using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace BestPracticesMcp.Functions;

/// <summary>
/// Provides tools for retrieving best practices for Python development.
/// Utilizes a process-wide cache to minimize disk reads and improve performance.
/// </summary>
/// <remarks>
/// This class is intended for internal use and is designed to be used within a function app context.
/// It reads best practices from a markdown file and caches the content for a short duration.
/// </remarks>
public class PythonTools(ILogger<PythonTools> logger)
{
    [Function(nameof(GetPythonBestPractices))]
    public async Task<string> GetPythonBestPractices(
        [McpToolTrigger("get_python_best_practices", "Retrieves best practices for Python development")]
            ToolInvocationContext toolContext,
        CancellationToken cancellationToken)
    {
        ToolLogging<PythonTools>.Serving(logger, "get_python_best_practices");

        var filePath = Path.Combine(AppContext.BaseDirectory, "Resources", "python-best-practices.md");

        // Fast-path: serve from cache if valid
        if (FileCache.TryGetValid(filePath, out var cached))
        {
            ToolLogging<PythonTools>.ServingCached(logger, filePath);
            return cached!;
        }

        try
        {
            // Attempt to load and cache the file (loader emits the "Loading" log)
            var content = await FileCache.GetOrLoadAsync(filePath, TimeSpan.FromMinutes(5), async () =>
            {
                ToolLogging<PythonTools>.Loading(logger, filePath);
                return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            if (content is not null)
                return content;

            ToolLogging<PythonTools>.FileNotFound(logger, filePath);
        }
        catch (OperationCanceledException)
        {
            // Propagate cancellation so the host can stop gracefully
            throw;
        }
        catch (IOException ex)
        {
            ToolLogging<PythonTools>.FailedToLoad(logger, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ToolLogging<PythonTools>.FailedToLoad(logger, ex);
        }
        catch (NotSupportedException ex)
        {
            ToolLogging<PythonTools>.FailedToLoad(logger, ex);
        }

        string[] fallback = new[]
        {
            "# Python Best Practices",
            "- Follow PEP 8 style guidelines; use automated formatters.",
            "- Write self-documenting code with clear, descriptive names.",
            "- Use type hints for function signatures and complex data structures.",
            "- Prefer list/dict comprehensions and generators for readability.",
            "- Handle exceptions explicitly; use specific exception types.",
            "- Write docstrings for modules, classes, and public functions.",
            "- Use virtual environments and pin dependencies with requirements files.",
            "- Follow the principle of least surprise; be explicit rather than implicit.",
            "- Write unit tests with meaningful assertions and good coverage.",
            "- Use logging instead of print statements for debugging and monitoring."
        };
        return string.Join(Environment.NewLine, fallback);
    }
}