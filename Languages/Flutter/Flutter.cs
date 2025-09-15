using BestPracticesMcp.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace BestPracticesMcp.Functions;

/// <summary>
/// Provides tools for retrieving best practices for the Flutter framework and Dart programming language.
/// Utilizes a process-wide cache to minimize disk reads and improve performance.
/// </summary>
/// <remarks>
/// This class is intended for internal use and is designed to be used within a function app context.
/// It reads best practices from a markdown file and caches the content for a short duration.
/// </remarks>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1852:Type can be sealed", Justification = "Instantiated by Functions host via reflection; suppress analyzer to avoid sealing suggestion")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Functions host via reflection")]
internal class FlutterTools(ILogger<FlutterTools> logger)
{
    [Function(nameof(GetFlutterBestPractices))]
    public async Task<string> GetFlutterBestPractices(
        [McpToolTrigger("get_flutter_best_practices", "Retrieves best practices for the Flutter framework and Dart programming language")]
            ToolInvocationContext toolContext,
        CancellationToken cancellationToken)
    {
        ToolLogging<FlutterTools>.Serving(logger, "get_flutter_best_practices");

        var filePath = Path.Combine(AppContext.BaseDirectory, "Languages", "Flutter", "flutter-best-practices.md");

        // Fast-path: serve from cache if valid
        if (FileCache.TryGetValid(filePath, out var cached))
        {
            ToolLogging<FlutterTools>.ServingCached(logger, filePath);
            return cached!;
        }

        try
        {
            var content = await FileCache.GetOrLoadAsync(filePath, TimeSpan.FromMinutes(5), async () =>
            {
                ToolLogging<FlutterTools>.Loading(logger, filePath);
                return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            if (content is not null)
                return content;

            ToolLogging<FlutterTools>.FileNotFound(logger, filePath);
        }
        catch (OperationCanceledException)
        {
            // Propagate cancellation so the host can stop gracefully
            throw;
        }
        catch (IOException ex)
        {
            ToolLogging<FlutterTools>.FailedToLoad(logger, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ToolLogging<FlutterTools>.FailedToLoad(logger, ex);
        }
        catch (NotSupportedException ex)
        {
            ToolLogging<FlutterTools>.FailedToLoad(logger, ex);
        }

        string[] fallback = new[]
        {
            "# Flutter Best Practices",
            "- Follow official Dart style guide and use automated formatting with `dart format`.",
            "- Organize project structure using feature-based folder hierarchy.",
            "- Use meaningful widget and class names that describe their purpose.",
            "- Prefer stateless widgets when state management isn't needed for better performance.",
            "- Implement proper state management patterns (Provider, Bloc, Riverpod, or GetX).",
            "- Always dispose controllers, streams, and listeners to prevent memory leaks.",
            "- Use const constructors wherever possible to optimize widget rebuilds.",
            "- Handle asynchronous operations with proper error handling and loading states.",
            "- Implement responsive design using MediaQuery, LayoutBuilder, and flexible widgets.",
            "- Write widget tests, unit tests, and integration tests for critical functionality."
        };
        return string.Join(Environment.NewLine, fallback);
    }
}