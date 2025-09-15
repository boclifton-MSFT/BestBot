using BestPracticesMcp.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace BestPracticesMcp.Functions;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1852:Type can be sealed", Justification = "Instantiated by Functions host via reflection; suppress analyzer to avoid sealing suggestion")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Functions host via reflection")]
internal class ElixirTools(ILogger<ElixirTools> logger)
{
    [Function(nameof(GetElixirBestPractices))]
    public async Task<string> GetElixirBestPractices(
        [McpToolTrigger("get_elixir_best_practices", "Retrieves best practices for the Elixir programming language")]
            ToolInvocationContext toolContext, CancellationToken cancellationToken)
    {
        ToolLogging<ElixirTools>.Serving(logger, "get_elixir_best_practices");

        var filePath = Path.Combine(AppContext.BaseDirectory, "Languages", "Elixir", "elixir-best-practices.md");

        if (FileCache.TryGetValid(filePath, out var cached))
        {
            ToolLogging<ElixirTools>.ServingCached(logger, filePath);
            return cached!;
        }

        try
        {
            var content = await FileCache.GetOrLoadAsync(filePath, TimeSpan.FromMinutes(5), async () =>
            {
                ToolLogging<ElixirTools>.Loading(logger, filePath);
                return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            if (content is not null)
                return content;

            ToolLogging<ElixirTools>.FileNotFound(logger, filePath);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (IOException ex)
        {
            ToolLogging<ElixirTools>.FailedToLoad(logger, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ToolLogging<ElixirTools>.FailedToLoad(logger, ex);
        }
        catch (NotSupportedException ex)
        {
            ToolLogging<ElixirTools>.FailedToLoad(logger, ex);
        }

        string[] fallback = new[]
        {
            "# Elixir Best Practices",
            "- Use pattern matching instead of conditional statements when possible.",
            "- Embrace immutability and functional programming principles.",
            "- Design for fault tolerance with supervision trees and 'let it crash' philosophy.",
            "- Use GenServer and OTP behaviors for stateful processes.",
            "- Write comprehensive doctests alongside regular tests.",
            "- Use pipe operator (|>) for data transformation chains.",
            "- Handle errors explicitly with {:ok, result} and {:error, reason} tuples.",
            "- Follow Elixir naming conventions: snake_case for functions, PascalCase for modules."
        };
        return string.Join(Environment.NewLine, fallback);
    }
}