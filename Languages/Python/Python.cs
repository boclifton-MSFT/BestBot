using BestPracticesMcp.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace BestPracticesMcp.Functions;

/// <summary>
/// Provides tools for retrieving best practices for the Python programming language.
/// Utilizes a process-wide cache to minimize disk reads and improve performance.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1852:Type can be sealed", Justification = "Instantiated by Functions host via reflection; suppress analyzer to avoid sealing suggestion")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Functions host via reflection")]
internal class PythonTools(ILogger<PythonTools> logger)
{
    [Function(nameof(GetPythonBestPractices))]
    public async Task<string> GetPythonBestPractices(
        [McpToolTrigger("get_python_best_practices", "Retrieves best practices for the Python programming language")]
            ToolInvocationContext toolContext, CancellationToken cancellationToken)
    {
        ToolLogging<PythonTools>.Serving(logger, "get_python_best_practices");

        var filePath = Path.Combine(AppContext.BaseDirectory, "Languages", "Python", "python-best-practices.md");

        if (FileCache.TryGetValid(filePath, out var cached))
        {
            ToolLogging<PythonTools>.ServingCached(logger, filePath);
            return cached!;
        }

        try
        {
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
            "- Follow PEP 8 formatting; use a formatter (black) and enforce in CI.",
            "- Type-check with mypy where feasible; prefer gradual typing.",
            "- Write unit tests that run fast and are deterministic.",
            "- Use virtual environments (venv) or environment managers and pin dependencies.",
            "- Avoid mutable default arguments; prefer None + sentinel logic.",
            "- Use context managers for resource management (with statements)."
        };
        return string.Join(Environment.NewLine, fallback);
    }
}
