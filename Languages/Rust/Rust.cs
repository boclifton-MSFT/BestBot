using BestPracticesMcp.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace BestPracticesMcp.Functions;

/// <summary>
/// Provides tools for retrieving best practices for the Rust programming language.
/// Utilizes a process-wide cache to minimize disk reads and improve performance.
/// </summary>
/// <remarks>
/// This class is intended for internal use and is designed to be used within a function app context.
/// It reads best practices from a markdown file and caches the content for a short duration.
/// </remarks>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1852:Type can be sealed", Justification = "Instantiated by Functions host via reflection; suppress analyzer to avoid sealing suggestion")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Functions host via reflection")]
internal class RustTools(ILogger<RustTools> logger)
{
    [Function(nameof(GetRustBestPractices))]
    public async Task<string> GetRustBestPractices(
        [McpToolTrigger("get_rust_best_practices", "Retrieves best practices for the Rust programming language")]
            ToolInvocationContext toolContext,
        CancellationToken cancellationToken)
    {
        ToolLogging<RustTools>.Serving(logger, "get_rust_best_practices");

        var filePath = Path.Combine(AppContext.BaseDirectory, "Languages", "Rust", "rust-best-practices.md");

        // Fast-path: serve from cache if valid
        if (FileCache.TryGetValid(filePath, out var cached))
        {
            ToolLogging<RustTools>.ServingCached(logger, filePath);
            return cached!;
        }

        try
        {
            var content = await FileCache.GetOrLoadAsync(filePath, TimeSpan.FromMinutes(5), async () =>
            {
                ToolLogging<RustTools>.Loading(logger, filePath);
                return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            if (content is not null)
                return content;

            ToolLogging<RustTools>.FileNotFound(logger, filePath);
        }
        catch (OperationCanceledException)
        {
            // Propagate cancellation so the host can stop gracefully
            throw;
        }
        catch (IOException ex)
        {
            ToolLogging<RustTools>.FailedToLoad(logger, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ToolLogging<RustTools>.FailedToLoad(logger, ex);
        }
        catch (NotSupportedException ex)
        {
            ToolLogging<RustTools>.FailedToLoad(logger, ex);
        }

        string[] fallback = new[]
        {
            "# Rust Best Practices",
            "- Embrace ownership and borrowing; design APIs that make invalid states unrepresentable.",
            "- Use `clippy` for linting and let it guide you to idiomatic Rust patterns.",
            "- Prefer `Result<T, E>` for error handling; avoid `panic!` in library code.",
            "- Use `cargo fmt` for consistent formatting and `cargo test` for comprehensive testing.",
            "- Write comprehensive documentation with `///` comments and examples that run as tests.",
            "- Prefer composition over inheritance; use traits for shared behavior.",
            "- Use structured error types with the `thiserror` crate for libraries.",
            "- Keep dependencies minimal; prefer std library when possible.",
            "- Use `#[derive(Debug)]` liberally and implement `Display` for user-facing types.",
            "- Design for zero-cost abstractions; measure performance when optimization matters."
        };
        return string.Join(Environment.NewLine, fallback);
    }
}