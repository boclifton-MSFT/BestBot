using BestPracticesMcp.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace BestPracticesMcp.Functions;

/// <summary>
/// Provides tools for retrieving best practices for the React JavaScript library.
/// Utilizes a process-wide cache to minimize disk reads and improve performance.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1852:Type can be sealed", Justification = "Instantiated by Functions host via reflection; suppress analyzer to avoid sealing suggestion")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Functions host via reflection")]
internal class ReactTools(ILogger<ReactTools> logger)
{
    [Function(nameof(GetReactBestPractices))]
    public async Task<string> GetReactBestPractices(
        [McpToolTrigger("get_react_best_practices", "Retrieves best practices for the React JavaScript library")]
            ToolInvocationContext toolContext, CancellationToken cancellationToken)
    {
        ToolLogging<ReactTools>.Serving(logger, "get_react_best_practices");

        var filePath = Path.Combine(AppContext.BaseDirectory, "Languages", "React", "react-best-practices.md");

        if (FileCache.TryGetValid(filePath, out var cached))
        {
            ToolLogging<ReactTools>.ServingCached(logger, filePath);
            return cached!;
        }

        try
        {
            var content = await FileCache.GetOrLoadAsync(filePath, TimeSpan.FromMinutes(5), async () =>
            {
                ToolLogging<ReactTools>.Loading(logger, filePath);
                return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            if (content is not null)
                return content;

            ToolLogging<ReactTools>.FileNotFound(logger, filePath);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (IOException ex)
        {
            ToolLogging<ReactTools>.FailedToLoad(logger, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ToolLogging<ReactTools>.FailedToLoad(logger, ex);
        }
        catch (NotSupportedException ex)
        {
            ToolLogging<ReactTools>.FailedToLoad(logger, ex);
        }

        string[] fallback = new[]
        {
            "# React Best Practices",
            "- Use functional components with hooks instead of class components.",
            "- Follow the single responsibility principle; keep components focused and small.",
            "- Use React.memo() for performance optimization of components that render frequently.",
            "- Prefer composition over inheritance for component reusability.",
            "- Use TypeScript for better type safety and developer experience.",
            "- Handle state with useState, useReducer, or external state management libraries.",
            "- Use useEffect properly with correct dependencies to avoid memory leaks.",
            "- Write comprehensive tests with React Testing Library.",
            "- Use ESLint with React-specific rules and Prettier for consistent formatting.",
            "- Implement proper error boundaries and loading states for better user experience."
        };
        return string.Join(Environment.NewLine, fallback);
    }
}