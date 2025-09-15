using BestPracticesMcp.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace BestPracticesMcp.Functions;

/// <summary>
/// Provides tools for retrieving best practices for the PHP programming language.
/// Utilizes a process-wide cache to minimize disk reads and improve performance.
/// </summary>
/// <remarks>
/// This class is intended for internal use and is designed to be used within a function app context.
/// It reads best practices from a markdown file and caches the content for a short duration.
/// </remarks>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1852:Type can be sealed", Justification = "Instantiated by Functions host via reflection; suppress analyzer to avoid sealing suggestion")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Functions host via reflection")]
internal class PhpTools(ILogger<PhpTools> logger)
{
    [Function(nameof(GetPhpBestPractices))]
    public async Task<string> GetPhpBestPractices(
        [McpToolTrigger("get_php_best_practices", "Retrieves best practices for the PHP programming language")]
            ToolInvocationContext toolContext, CancellationToken cancellationToken)
    {
        ToolLogging<PhpTools>.Serving(logger, "get_php_best_practices");

        var filePath = Path.Combine(AppContext.BaseDirectory, "Languages", "Php", "php-best-practices.md");

        if (FileCache.TryGetValid(filePath, out var cached))
        {
            ToolLogging<PhpTools>.ServingCached(logger, filePath);
            return cached!;
        }

        try
        {
            var content = await FileCache.GetOrLoadAsync(filePath, TimeSpan.FromMinutes(5), async () =>
            {
                ToolLogging<PhpTools>.Loading(logger, filePath);
                return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            if (content is not null)
                return content;

            ToolLogging<PhpTools>.FileNotFound(logger, filePath);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (IOException ex)
        {
            ToolLogging<PhpTools>.FailedToLoad(logger, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ToolLogging<PhpTools>.FailedToLoad(logger, ex);
        }
        catch (NotSupportedException ex)
        {
            ToolLogging<PhpTools>.FailedToLoad(logger, ex);
        }

        string[] fallback = new[]
        {
            "# PHP Best Practices",
            "- Follow PSR standards for consistent code style and formatting.",
            "- Use strict types with declare(strict_types=1) at the top of files.",
            "- Validate all input and escape all output to prevent security vulnerabilities.",
            "- Use prepared statements for database queries to prevent SQL injection.",
            "- Implement proper error handling with exceptions rather than error codes.",
            "- Write comprehensive tests and use static analysis tools like PHPStan."
        };
        return string.Join(Environment.NewLine, fallback);
    }
}