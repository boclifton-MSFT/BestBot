using BestPracticesMcp.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace BestPracticesMcp.Functions;

/// <summary>
/// Provides tools for retrieving best practices for the C++ programming language.
/// Utilizes a process-wide cache to minimize disk reads and improve performance.
/// </summary>
/// <remarks>
/// This class is intended for internal use and is designed to be used within a function app context.
/// It reads best practices from a markdown file and caches the content for a short duration.
/// </remarks>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1852:Type can be sealed", Justification = "Instantiated by Functions host via reflection; suppress analyzer to avoid sealing suggestion")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Functions host via reflection")]
internal class CppTools(ILogger<CppTools> logger)
{
    [Function(nameof(GetCppBestPractices))]
    public async Task<string> GetCppBestPractices(
        [McpToolTrigger("get_cpp_best_practices", "Retrieves best practices for the C++ programming language")]
            ToolInvocationContext toolContext,
        CancellationToken cancellationToken)
    {
        ToolLogging<CppTools>.Serving(logger, "get_cpp_best_practices");

        var filePath = Path.Combine(AppContext.BaseDirectory, "Languages", "Cpp", "cpp-best-practices.md");

        // Fast-path: serve from cache if valid
        if (FileCache.TryGetValid(filePath, out var cached))
        {
            ToolLogging<CppTools>.ServingCached(logger, filePath);
            return cached!;
        }

        try
        {
            var content = await FileCache.GetOrLoadAsync(filePath, TimeSpan.FromMinutes(5), async () =>
            {
                ToolLogging<CppTools>.Loading(logger, filePath);
                return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            if (content is not null)
                return content;

            ToolLogging<CppTools>.FileNotFound(logger, filePath);
        }
        catch (OperationCanceledException)
        {
            // Propagate cancellation so the host can stop gracefully
            throw;
        }
        catch (IOException ex)
        {
            ToolLogging<CppTools>.FailedToLoad(logger, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ToolLogging<CppTools>.FailedToLoad(logger, ex);
        }
        catch (NotSupportedException ex)
        {
            ToolLogging<CppTools>.FailedToLoad(logger, ex);
        }

        // Fallback content if file is unavailable
        string[] fallback = new[]
        {
            "# C++ Best Practices",
            "- Use modern C++ features (C++11 and later) for safer, more expressive code.",
            "- Prefer smart pointers (std::unique_ptr, std::shared_ptr) over raw pointers.",
            "- Use RAII (Resource Acquisition Is Initialization) for automatic resource management.",
            "- Follow established style guides (Google C++ Style Guide, C++ Core Guidelines).",
            "- Enable compiler warnings and use static analysis tools (clang-tidy, cppcheck).",
            "- Use CMake 3.20+ for cross-platform builds and vcpkg for dependency management.",
            "- Write comprehensive unit tests with Google Test framework.",
            "- Validate all external input and prefer containers over C-style arrays.",
            "- Use const-correctness and avoid global variables.",
            "- Profile before optimizing; prefer clarity over premature optimization."
        };
        return string.Join(Environment.NewLine, fallback);
    }
}