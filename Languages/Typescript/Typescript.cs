using BestPracticesMcp.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace BestPracticesMcp.Functions;

/// <summary>
/// Provides tools for retrieving best practices for the TypeScript programming language.
/// Utilizes a process-wide cache to minimize disk reads and improve performance.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1852:Type can be sealed", Justification = "Instantiated by Functions host via reflection; suppress analyzer to avoid sealing suggestion")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Functions host via reflection")]
internal class TypescriptTools(ILogger<TypescriptTools> logger)
{
    [Function(nameof(GetTypescriptBestPractices))]
    public async Task<string> GetTypescriptBestPractices([
        McpToolTrigger("get_typescript_best_practices", "Retrieves best practices for the TypeScript programming language")]
        ToolInvocationContext toolContext, CancellationToken cancellationToken)
    {
        ToolLogging<TypescriptTools>.Serving(logger, "get_typescript_best_practices");

        var filePath = Path.Combine(AppContext.BaseDirectory, "Languages", "Typescript", "typescript-best-practices.md");

        if (FileCache.TryGetValid(filePath, out var cached))
        {
            ToolLogging<TypescriptTools>.ServingCached(logger, filePath);
            return cached!;
        }

        try
        {
            var content = await FileCache.GetOrLoadAsync(filePath, TimeSpan.FromMinutes(5), async () =>
            {
                ToolLogging<TypescriptTools>.Loading(logger, filePath);
                return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            if (content is not null)
                return content;

            ToolLogging<TypescriptTools>.FileNotFound(logger, filePath);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (IOException ex)
        {
            ToolLogging<TypescriptTools>.FailedToLoad(logger, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ToolLogging<TypescriptTools>.FailedToLoad(logger, ex);
        }
        catch (NotSupportedException ex)
        {
            ToolLogging<TypescriptTools>.FailedToLoad(logger, ex);
        }

        string[] fallback = new[]
        {
            "# TypeScript Best Practices",
            "- Enable strict type checking (\"strict\": true in tsconfig.json).",
            "- Prefer const and readonly for values that don't change.",
            "- Avoid any whenever possible; prefer unknown or precise types.",
            "- Use linters and formatters: eslint with @typescript-eslint and prettier.",
            "- Write unit tests and include type checks in CI (tsc --noEmit)."
        };
        return string.Join(Environment.NewLine, fallback);
    }
}
