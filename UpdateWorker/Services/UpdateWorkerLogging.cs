using Microsoft.Extensions.Logging;

namespace BestPracticesMcp.UpdateWorker.Services;

/// <summary>
/// Source-generated logging for the update worker.
/// Follows the same pattern as <see cref="BestPracticesMcp.Utilities.ToolLoggingSource"/>
/// for high-performance, allocation-free logging.
/// </summary>
internal static partial class UpdateWorkerLogging
{
    [LoggerMessage(EventId = 100, Level = LogLevel.Information, Message = "Update orchestration started for {LanguageCount} languages")]
    public static partial void OrchestrationStarted(ILogger logger, int languageCount);

    [LoggerMessage(EventId = 101, Level = LogLevel.Information, Message = "Update orchestration completed: {UpdatedCount}/{TotalCount} languages had updates")]
    public static partial void OrchestrationCompleted(ILogger logger, int updatedCount, int totalCount);

    [LoggerMessage(EventId = 102, Level = LogLevel.Information, Message = "Agent session started for {Language}")]
    public static partial void AgentSessionStarted(ILogger logger, string language);

    [LoggerMessage(EventId = 103, Level = LogLevel.Information, Message = "Agent session completed for {Language}, needs update: {NeedsUpdate}")]
    public static partial void AgentSessionCompleted(ILogger logger, string language, bool needsUpdate);

    [LoggerMessage(EventId = 104, Level = LogLevel.Warning, Message = "Resource URL check failed for {Url} with status {StatusCode}")]
    public static partial void UrlCheckFailed(ILogger logger, string url, int statusCode);

    [LoggerMessage(EventId = 105, Level = LogLevel.Information, Message = "Resource content changed for {Language}: old hash {OldHash} -> new hash {NewHash}")]
    public static partial void ContentChanged(ILogger logger, string language, string oldHash, string newHash);

    [LoggerMessage(EventId = 106, Level = LogLevel.Information, Message = "Version bump detected for {Language}: {OldVersion} -> {NewVersion}")]
    public static partial void VersionBumpDetected(ILogger logger, string language, string oldVersion, string newVersion);

    [LoggerMessage(EventId = 107, Level = LogLevel.Information, Message = "GitHub PR created: {PrUrl}")]
    public static partial void PrCreated(ILogger logger, string prUrl);

    [LoggerMessage(EventId = 108, Level = LogLevel.Error, Message = "Failed to create GitHub PR")]
    public static partial void PrCreationFailed(ILogger logger, Exception exception);

    [LoggerMessage(EventId = 109, Level = LogLevel.Debug, Message = "Skipped prerelease version for {Language}: {Version}")]
    public static partial void SkippedPrerelease(ILogger logger, string language, string version);

    [LoggerMessage(EventId = 110, Level = LogLevel.Information, Message = "Update worker timer triggered at {TriggerTime}")]
    public static partial void TimerTriggered(ILogger logger, DateTimeOffset triggerTime);

    [LoggerMessage(EventId = 111, Level = LogLevel.Warning, Message = "Update worker is disabled via configuration")]
    public static partial void WorkerDisabled(ILogger logger);

    [LoggerMessage(EventId = 112, Level = LogLevel.Information, Message = "Discovered {Count} language files for update checking")]
    public static partial void LanguagesDiscovered(ILogger logger, int count);

    [LoggerMessage(EventId = 113, Level = LogLevel.Warning, Message = "Languages directory not found at {Path}")]
    public static partial void LanguagesDirectoryNotFound(ILogger logger, string path);

    [LoggerMessage(EventId = 114, Level = LogLevel.Warning, Message = "No language files found for update checking")]
    public static partial void NoLanguageFilesFound(ILogger logger);

    [LoggerMessage(EventId = 115, Level = LogLevel.Information, Message = "Started update orchestration with instance ID: {InstanceId}")]
    public static partial void OrchestrationInstanceStarted(ILogger logger, string instanceId);

    [LoggerMessage(EventId = 116, Level = LogLevel.Warning, Message = "File not found in repo: {Path}")]
    public static partial void FileNotFoundInRepo(ILogger logger, string path);

    [LoggerMessage(EventId = 117, Level = LogLevel.Information, Message = "PR created at: {PrUrl}")]
    public static partial void PrCreatedAtUrl(ILogger logger, string prUrl);
}
