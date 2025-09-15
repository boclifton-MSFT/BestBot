using BestPracticesMcp.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace BestPracticesMcp.Functions;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1852:Type can be sealed", Justification = "Instantiated by Functions host via reflection; suppress analyzer to avoid sealing suggestion")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Functions host via reflection")]
internal class Vue3Tools(ILogger<Vue3Tools> logger)
{
    [Function(nameof(GetVue3BestPractices))]
    public async Task<string> GetVue3BestPractices(
        [McpToolTrigger("get_vue3_best_practices", "Retrieves best practices for building apps with Vue 3")]
            ToolInvocationContext toolContext,
        CancellationToken cancellationToken)
    {
        ToolLogging<Vue3Tools>.Serving(logger, "get_vue3_best_practices");

        var filePath = Path.Combine(AppContext.BaseDirectory, "Languages", "Vue3", "vue3-best-practices.md");

        // Fast-path: serve from cache if valid
        if (FileCache.TryGetValid(filePath, out var cached))
        {
            ToolLogging<Vue3Tools>.ServingCached(logger, filePath);
            return cached!;
        }

        try
        {
            var content = await FileCache.GetOrLoadAsync(filePath, TimeSpan.FromMinutes(5), async () =>
            {
                ToolLogging<Vue3Tools>.Loading(logger, filePath);
                return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            if (content is not null)
                return content;

            ToolLogging<Vue3Tools>.FileNotFound(logger, filePath);
        }
        catch (OperationCanceledException)
        {
            // Propagate cancellation so the host can stop gracefully
            throw;
        }
        catch (IOException ex)
        {
            ToolLogging<Vue3Tools>.FailedToLoad(logger, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ToolLogging<Vue3Tools>.FailedToLoad(logger, ex);
        }
        catch (NotSupportedException ex)
        {
            ToolLogging<Vue3Tools>.FailedToLoad(logger, ex);
        }

        string[] fallback = new[]
        {
            "# Vue 3 Best Practices",
            "- Prefer composition API for new components; keep components small and focused.",
            "- Use script setup syntax for concise single-file components when appropriate.",
            "- Organize state using composables or a lightweight store (Pinia).",
            "- Write component-level unit tests and end-to-end tests for critical flows.",
            "- Use linters/formatters (eslint + prettier) and enforce in CI."
        };
        return string.Join(Environment.NewLine, fallback);
    }
}
