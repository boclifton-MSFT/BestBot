using BestPracticesMcp.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace BestPracticesMcp.Functions;

/// <summary>
/// Provides tools for retrieving best practices for the Java programming language.
/// Utilizes a process-wide cache to minimize disk reads and improve performance.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1852:Type can be sealed", Justification = "Instantiated by Functions host via reflection; suppress analyzer to avoid sealing suggestion")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Functions host via reflection")]
internal class JavaTools(ILogger<JavaTools> logger)
{
    [Function(nameof(GetJavaBestPractices))]
    public async Task<string> GetJavaBestPractices([
        McpToolTrigger("get_java_best_practices", "Retrieves best practices for the Java programming language")]
        ToolInvocationContext toolContext, CancellationToken cancellationToken)
    {
        ToolLogging<JavaTools>.Serving(logger, "get_java_best_practices");

        var filePath = Path.Combine(AppContext.BaseDirectory, "Languages", "Java", "java-best-practices.md");

        if (FileCache.TryGetValid(filePath, out var cached))
        {
            ToolLogging<JavaTools>.ServingCached(logger, filePath);
            return cached!;
        }

        try
        {
            var content = await FileCache.GetOrLoadAsync(filePath, TimeSpan.FromMinutes(5), async () =>
            {
                ToolLogging<JavaTools>.Loading(logger, filePath);
                return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            if (content is not null)
                return content;

            ToolLogging<JavaTools>.FileNotFound(logger, filePath);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (IOException ex)
        {
            ToolLogging<JavaTools>.FailedToLoad(logger, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ToolLogging<JavaTools>.FailedToLoad(logger, ex);
        }
        catch (NotSupportedException ex)
        {
            ToolLogging<JavaTools>.FailedToLoad(logger, ex);
        }

        string[] fallback = new[]
        {
            "# Java Best Practices",
            "- Follow established formatting (google-java-format) and style guides (Google Java Style Guide).",
            "- Use proper package layout (src/main/java, src/test/java) and naming conventions.",
            "- Prefer immutability and use records or builders for value types.",
            "- Use try-with-resources for AutoCloseable resources.",
            "- Prefer java.util.concurrent utilities for concurrency.",
            "- Write focused, deterministic unit tests; use integration tests only when needed.",
            "- Use static analysis tools (SpotBugs, Error Prone) in CI.",
            "- Document public APIs with Javadoc; include concise summary fragments."
        };
        return string.Join(Environment.NewLine, fallback);
    }
}
