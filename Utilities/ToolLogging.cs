using System;
using Microsoft.Extensions.Logging;

namespace BestPracticesMcp.Utilities;

/// <summary>
/// Centralized logging helpers for MCP tools.
///
/// - Uses a single source-generated static partial class (<see cref="ToolLoggingSource"/>) to host
///   LoggerMessage declarations (enabling high-performance, allocation-free logging via source gen).
/// - Exposes a small, generic wrapper <see cref="ToolLogging{T}"/> that derives a friendly tool name
///   from the invoking type and forwards strongly-typed calls to the generated methods.
///
/// This approach keeps the LoggerMessage definitions in one place while providing a convenient
/// type-safe surface for each tool to use. It is intentionally conservative and easy to extend
/// for new message shapes in the future.
/// </summary>
internal static class ToolLogging<T>
{
    private static readonly string FriendlyName = GetFriendlyName();

    private static string GetFriendlyName()
    {
        var name = typeof(T).Name;
        if (name.EndsWith("Tools", StringComparison.Ordinal))
        {
            return name[..^"Tools".Length];
        }

        // Fallback to the runtime type name for clarity
        return name;
    }

    public static void Serving(ILogger logger, string toolName)
        => ToolLoggingSource.Serving(logger, FriendlyName, toolName);

    public static void Loading(ILogger logger, string filePath)
        => ToolLoggingSource.Loading(logger, FriendlyName, filePath);

    public static void ServingCached(ILogger logger, string filePath)
        => ToolLoggingSource.ServingCached(logger, FriendlyName, filePath);

    public static void FileNotFound(ILogger logger, string filePath)
        => ToolLoggingSource.FileNotFound(logger, FriendlyName, filePath);

    public static void FailedToLoad(ILogger logger, Exception exception)
        => ToolLoggingSource.FailedToLoad(logger, FriendlyName, exception);
}

// Source-generated LoggerMessage methods centralized in a single partial class to
// produce high-performance logging implementations at compile time. Keep the
// method shapes stable; add new entries here as needed for future tools.
internal static partial class ToolLoggingSource
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Serving {ToolFriendlyName} via MCP tool {ToolName}")]
    public static partial void Serving(ILogger logger, string ToolFriendlyName, string ToolName);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Loading {ToolFriendlyName} from {FilePath}")]
    public static partial void Loading(ILogger logger, string ToolFriendlyName, string FilePath);

    [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "Serving cached {ToolFriendlyName} content from {FilePath}")]
    public static partial void ServingCached(ILogger logger, string ToolFriendlyName, string FilePath);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "{ToolFriendlyName} file not found at {FilePath}; serving fallback")]
    public static partial void FileNotFound(ILogger logger, string ToolFriendlyName, string FilePath);

    [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "Failed to load {ToolFriendlyName} content; serving fallback")]
    public static partial void FailedToLoad(ILogger logger, string ToolFriendlyName, Exception exception);
}
