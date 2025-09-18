using BestPracticesMcp.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace BestPracticesMcp.Functions;

/// <summary>
/// Provides tools for retrieving best practices for the Kotlin programming language.
/// Utilizes a process-wide cache to minimize disk reads and improve performance.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1852:Type can be sealed", Justification = "Instantiated by Functions host via reflection; suppress analyzer to avoid sealing suggestion")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Functions host via reflection")]
internal class KotlinTools(ILogger<KotlinTools> logger)
{
    [Function(nameof(GetKotlinBestPractices))]
    public async Task<string> GetKotlinBestPractices(
        [McpToolTrigger("get_kotlin_best_practices", "Retrieves best practices for the Kotlin programming language")]
            ToolInvocationContext toolContext,
        CancellationToken cancellationToken)
    {
        ToolLogging<KotlinTools>.Serving(logger, "get_kotlin_best_practices");

        var filePath = Path.Combine(AppContext.BaseDirectory, "Languages", "Kotlin", "kotlin-best-practices.md");

        // Fast-path: serve from cache if valid
        if (FileCache.TryGetValid(filePath, out var cached))
        {
            ToolLogging<KotlinTools>.ServingCached(logger, filePath);
            return cached!;
        }

        try
        {
            var content = await FileCache.GetOrLoadAsync(filePath, TimeSpan.FromMinutes(5), async () =>
            {
                ToolLogging<KotlinTools>.Loading(logger, filePath);
                return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            if (content is not null)
                return content;

            ToolLogging<KotlinTools>.FileNotFound(logger, filePath);
        }
        catch (OperationCanceledException)
        {
            // Propagate cancellation so the host can stop gracefully
            throw;
        }
        catch (IOException ex)
        {
            ToolLogging<KotlinTools>.FailedToLoad(logger, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ToolLogging<KotlinTools>.FailedToLoad(logger, ex);
        }
        catch (NotSupportedException ex)
        {
            ToolLogging<KotlinTools>.FailedToLoad(logger, ex);
        }

        string[] fallback = new[]
        {
            "# Kotlin Best Practices",
            "- Leverage null safety with safe calls (?.) and elvis operator (?:)",
            "- Use data classes for simple value objects and POJOs",
            "- Prefer val over var for immutable references",
            "- Use coroutines for asynchronous programming instead of callbacks",
            "- Apply extension functions to add functionality to existing classes",
            "- Follow official Kotlin coding conventions and style guide",
            "- Use when expressions instead of long if-else chains",
            "- Prefer standard library functions (let, run, apply, also) for scoping",
            "- Write comprehensive unit tests using JUnit and MockK",
            "- Use ktlint and detekt for code quality and consistency"
        };
        return string.Join(Environment.NewLine, fallback);
    }
}