using BestPracticesMcp.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace BestPracticesMcp.Functions;

/// <summary>
/// Provides tools for retrieving best practices for the JavaScript programming language.
/// Utilizes a process-wide cache to minimize disk reads and improve performance.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1852:Type can be sealed", Justification = "Instantiated by Functions host via reflection; suppress analyzer to avoid sealing suggestion")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Functions host via reflection")]
internal class JavascriptTools(ILogger<JavascriptTools> logger)
{
    [Function(nameof(GetJavascriptBestPractices))]
    public async Task<string> GetJavascriptBestPractices(
        [McpToolTrigger("get_javascript_best_practices", "Retrieves best practices for the JavaScript programming language")]
            ToolInvocationContext toolContext, CancellationToken cancellationToken)
    {
        ToolLogging<JavascriptTools>.Serving(logger, "get_javascript_best_practices");

        var filePath = Path.Combine(AppContext.BaseDirectory, "Languages", "Javascript", "javascript-best-practices.md");

        if (FileCache.TryGetValid(filePath, out var cached))
        {
            ToolLogging<JavascriptTools>.ServingCached(logger, filePath);
            return cached!;
        }

        try
        {
            var content = await FileCache.GetOrLoadAsync(filePath, TimeSpan.FromMinutes(5), async () =>
            {
                ToolLogging<JavascriptTools>.Loading(logger, filePath);
                return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            if (content is not null)
                return content;

            ToolLogging<JavascriptTools>.FileNotFound(logger, filePath);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (IOException ex)
        {
            ToolLogging<JavascriptTools>.FailedToLoad(logger, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ToolLogging<JavascriptTools>.FailedToLoad(logger, ex);
        }
        catch (NotSupportedException ex)
        {
            ToolLogging<JavascriptTools>.FailedToLoad(logger, ex);
        }

        string[] fallback = new[]
        {
            "# JavaScript Best Practices",
            "- Use modern ES6+ features: const/let, arrow functions, destructuring, template literals.",
            "- Prefer strict mode ('use strict') or use a module system (ES modules).",
            "- Use meaningful variable and function names; avoid abbreviations and single-letter variables.",
            "- Handle errors explicitly with try-catch blocks and proper error propagation.",
            "- Use async/await for asynchronous code instead of nested callbacks or complex promise chains.",
            "- Write pure functions when possible; minimize side effects and mutations.",
            "- Use ESLint and Prettier for consistent code style and quality.",
            "- Prefer const for values that don't change, let for variables that do, avoid var.",
            "- Use strict equality (===) instead of loose equality (==).",
            "- Write unit tests with a testing framework like Jest, Vitest, or Mocha."
        };
        return string.Join(Environment.NewLine, fallback);
    }
}