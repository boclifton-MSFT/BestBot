using BestPracticesMcp.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace BestPracticesMcp.Functions;

/// <summary>
/// Provides tools for retrieving best practices for the Ruby programming language.
/// Utilizes a process-wide cache to minimize disk reads and improve performance.
/// </summary>
/// <remarks>
/// This class is intended for internal use and is designed to be used within a function app context.
/// It reads best practices from a markdown file and caches the content for a short duration.
/// </remarks>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1852:Type can be sealed", Justification = "Instantiated by Functions host via reflection; suppress analyzer to avoid sealing suggestion")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Functions host via reflection")]
internal class RubyTools(ILogger<RubyTools> logger)
{
    [Function(nameof(GetRubyBestPractices))]
    public async Task<string> GetRubyBestPractices(
        [McpToolTrigger("get_ruby_best_practices", "Retrieves best practices for the Ruby programming language")]
            ToolInvocationContext toolContext, CancellationToken cancellationToken)
    {
        ToolLogging<RubyTools>.Serving(logger, "get_ruby_best_practices");

        var filePath = Path.Combine(AppContext.BaseDirectory, "Languages", "Ruby", "ruby-best-practices.md");

        if (FileCache.TryGetValid(filePath, out var cached))
        {
            ToolLogging<RubyTools>.ServingCached(logger, filePath);
            return cached!;
        }

        try
        {
            var content = await FileCache.GetOrLoadAsync(filePath, TimeSpan.FromMinutes(5), async () =>
            {
                ToolLogging<RubyTools>.Loading(logger, filePath);
                return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            if (content is not null)
                return content;

            ToolLogging<RubyTools>.FileNotFound(logger, filePath);
        }
        catch (OperationCanceledException)
        {
            // Propagate cancellation so the host can stop gracefully
            throw;
        }
        catch (IOException ex)
        {
            ToolLogging<RubyTools>.FailedToLoad(logger, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ToolLogging<RubyTools>.FailedToLoad(logger, ex);
        }
        catch (NotSupportedException ex)
        {
            ToolLogging<RubyTools>.FailedToLoad(logger, ex);
        }

        // Fallback best practices if file cannot be loaded
        string[] fallback = new[]
        {
            "# Ruby Best Practices",
            "- Follow Ruby Style Guide conventions; use automated formatters (RuboCop).",
            "- Write self-documenting code with clear, descriptive method and variable names.",
            "- Prefer blocks and iterators over traditional loops for better readability.",
            "- Use meaningful method names that express intent; avoid abbreviations.",
            "- Handle exceptions explicitly; prefer specific rescue clauses.",
            "- Write comprehensive tests using RSpec or Minitest.",
            "- Use bundler for dependency management and lock versions.",
            "- Follow the principle of least surprise; be explicit and clear.",
            "- Prefer symbols over strings for hash keys and constants.",
            "- Use duck typing and embrace Ruby's dynamic nature responsibly."
        };
        return string.Join(Environment.NewLine, fallback);
    }
}