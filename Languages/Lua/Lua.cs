using BestPracticesMcp.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace BestPracticesMcp.Functions;

/// <summary>
/// Provides tools for retrieving best practices for the Lua programming language.
/// Utilizes a process-wide cache to minimize disk reads and improve performance.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1852:Type can be sealed", Justification = "Instantiated by Functions host via reflection; suppress analyzer to avoid sealing suggestion")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Functions host via reflection")]
internal class LuaTools(ILogger<LuaTools> logger)
{
    [Function(nameof(GetLuaBestPractices))]
    public async Task<string> GetLuaBestPractices(
        [McpToolTrigger("get_lua_best_practices", "Retrieves best practices for the Lua programming language")]
            ToolInvocationContext toolContext, CancellationToken cancellationToken)
    {
        ToolLogging<LuaTools>.Serving(logger, "get_lua_best_practices");

        var filePath = Path.Combine(AppContext.BaseDirectory, "Languages", "Lua", "lua-best-practices.md");

        if (FileCache.TryGetValid(filePath, out var cached))
        {
            ToolLogging<LuaTools>.ServingCached(logger, filePath);
            return cached!;
        }

        try
        {
            var content = await FileCache.GetOrLoadAsync(filePath, TimeSpan.FromMinutes(5), async () =>
            {
                ToolLogging<LuaTools>.Loading(logger, filePath);
                return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            if (content is not null)
                return content;

            ToolLogging<LuaTools>.FileNotFound(logger, filePath);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (IOException ex)
        {
            ToolLogging<LuaTools>.FailedToLoad(logger, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ToolLogging<LuaTools>.FailedToLoad(logger, ex);
        }
        catch (NotSupportedException ex)
        {
            ToolLogging<LuaTools>.FailedToLoad(logger, ex);
        }

        string[] fallback = new[]
        {
            "# Lua Best Practices",
            "- Always declare variables with `local` to avoid polluting the global namespace.",
            "- Use Luacheck for static analysis; enforce in CI.",
            "- Use LuaRocks for dependency management and packaging.",
            "- Handle errors with pcall/xpcall; return nil and error message for expected failures.",
            "- Sandbox untrusted code with restricted environments; never pass user input to loadstring or os.execute."
        };
        return string.Join(Environment.NewLine, fallback);
    }
}
