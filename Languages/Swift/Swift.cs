using BestPracticesMcp.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace BestPracticesMcp.Functions;

/// <summary>
/// Provides tools for retrieving best practices for the Swift programming language.
/// Utilizes a process-wide cache to minimize disk reads and improve performance.
/// </summary>
/// <remarks>
/// This class is intended for internal use and is designed to be used within a function app context.
/// It reads best practices from a markdown file and caches the content for a short duration.
/// </remarks>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1852:Type can be sealed", Justification = "Instantiated by Functions host via reflection; suppress analyzer to avoid sealing suggestion")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Functions host via reflection")]
internal class SwiftTools(ILogger<SwiftTools> logger)
{
    [Function(nameof(GetSwiftBestPractices))]
    public async Task<string> GetSwiftBestPractices([
        McpToolTrigger("get_swift_best_practices", "Retrieves best practices for the Swift programming language")]
        ToolInvocationContext toolContext, CancellationToken cancellationToken)
    {
        ToolLogging<SwiftTools>.Serving(logger, "get_swift_best_practices");

        var filePath = Path.Combine(AppContext.BaseDirectory, "Languages", "Swift", "swift-best-practices.md");

        // Fast-path: serve from cache if valid
        if (FileCache.TryGetValid(filePath, out var cached))
        {
            ToolLogging<SwiftTools>.ServingCached(logger, filePath);
            return cached!;
        }

        try
        {
            var content = await FileCache.GetOrLoadAsync(filePath, TimeSpan.FromMinutes(5), async () =>
            {
                ToolLogging<SwiftTools>.Loading(logger, filePath);
                return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            if (content is not null)
                return content;

            ToolLogging<SwiftTools>.FileNotFound(logger, filePath);
        }
        catch (OperationCanceledException)
        {
            // Propagate cancellation so the host can stop gracefully
            throw;
        }
        catch (IOException ex)
        {
            ToolLogging<SwiftTools>.FailedToLoad(logger, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ToolLogging<SwiftTools>.FailedToLoad(logger, ex);
        }
        catch (NotSupportedException ex)
        {
            ToolLogging<SwiftTools>.FailedToLoad(logger, ex);
        }

        // Return fallback content if file is unavailable
        string[] fallback = new[]
        {
            "# Swift Best Practices",
            "- Use clear, descriptive naming that expresses intent rather than implementation.",
            "- Prefer value types (structs, enums) over reference types (classes) when appropriate.",
            "- Leverage Swift's type system: use optionals safely, prefer non-optional types when possible.",
            "- Follow Swift naming conventions: lowerCamelCase for variables/functions, UpperCamelCase for types.",
            "- Use `let` for immutable values, `var` only when mutation is needed.",
            "- Prefer guard statements for early returns and safe unwrapping.",
            "- Use extensions to organize code and add protocol conformances.",
            "- Write self-documenting code with clear function signatures and meaningful variable names.",
            "- Handle errors explicitly using Swift's error handling mechanisms.",
            "- Leverage modern Swift features: property wrappers, result builders, async/await."
        };
        return string.Join(Environment.NewLine, fallback);
    }
}