using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace BestPracticesMcp.Functions;

/// <summary>
/// Provides tools for retrieving best practices for Vue 3 development.
/// Utilizes a process-wide cache to minimize disk reads and improve performance.
/// </summary>
/// <remarks>
/// This class is intended for internal use and is designed to be used within a function app context.
/// It reads best practices from a markdown file and caches the content for a short duration.
/// </remarks>
internal class Vue3Tools(ILogger<Vue3Tools> logger)
{
    [Function(nameof(GetVue3BestPractices))]
    public async Task<string> GetVue3BestPractices(
        [McpToolTrigger("get_vue3_best_practices", "Retrieves best practices for Vue 3 development")]
            ToolInvocationContext toolContext,
        CancellationToken cancellationToken)
    {
        ToolLogging<Vue3Tools>.Serving(logger, "get_vue3_best_practices");

        var filePath = Path.Combine(AppContext.BaseDirectory, "Resources", "vue3-best-practices.md");

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
            "- Use multi-word component names to avoid conflicts.",
            "- Define detailed prop types with validation.",
            "- Always use :key with v-for for predictable rendering.",
            "- Avoid v-if with v-for on the same element.",
            "- Use component-scoped styling (scoped CSS or CSS modules).",
            "- Prefer Composition API for better TypeScript support.",
            "- Keep template expressions simple; use computed properties for complex logic.",
            "- Follow consistent component naming and file organization."
        };
        return string.Join(Environment.NewLine, fallback);
    }
}