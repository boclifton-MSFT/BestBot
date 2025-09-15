using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BestPracticesMcp.Utilities;

/// <summary>
/// Lightweight, process-wide file content cache keyed by absolute file path.
///
/// - Keeps a cached value per file path and validates it against the file's
///   last-write timestamp to ensure staleness is detected when the file changes.
/// - Uses a per-entry <see cref="SemaphoreSlim"/> to avoid thundering-herd
///   file reads when multiple callers request the same file concurrently.
/// - Intended to be a small utility usable by multiple MCP tools in the function
///   app; it is deliberately conservative in behavior and surface area.
/// </summary>
internal static class FileCache
{
    private sealed class CacheEntry
    {
        public SemaphoreSlim Lock { get; } = new(1, 1);
        public string? Content;
        public DateTimeOffset LastWrite;
        public DateTimeOffset Expires;
    }

    private static readonly ConcurrentDictionary<string, CacheEntry> Entries = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Returns true when the cache contains a valid, unexpired entry that matches the
    /// current file's last-write timestamp. Caller may assume the returned content is safe
    /// to serve without further IO.
    /// </summary>
    public static bool TryGetValid(string filePath, out string? content)
    {
        content = null;
        if (!Entries.TryGetValue(filePath, out var entry))
            return false;

        if (!File.Exists(filePath))
            return false;

        var lastWrite = File.GetLastWriteTimeUtc(filePath);
        if (entry.Content is not null && entry.Expires > DateTimeOffset.UtcNow && entry.LastWrite == lastWrite)
        {
            content = entry.Content;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Ensures the file content is loaded into the cache and returns it. If the cache
    /// contains a valid entry it is returned immediately; otherwise the provided
    /// loader is invoked while holding a per-entry lock to prevent concurrent loads.
    ///
    /// Returns null when the file does not exist.
    /// </summary>
    public static async Task<string?> GetOrLoadAsync(string filePath, TimeSpan ttl, Func<Task<string>> loader, CancellationToken cancellationToken)
    {
        var entry = Entries.GetOrAdd(filePath, _ => new CacheEntry());

        // Quick check before acquiring the lock
        if (File.Exists(filePath))
        {
            var lastWrite = File.GetLastWriteTimeUtc(filePath);
            if (entry.Content is not null && entry.Expires > DateTimeOffset.UtcNow && entry.LastWrite == lastWrite)
                return entry.Content;
        }

        await entry.Lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!File.Exists(filePath))
                return null;

            var lastWrite = File.GetLastWriteTimeUtc(filePath);
            if (entry.Content is not null && entry.Expires > DateTimeOffset.UtcNow && entry.LastWrite == lastWrite)
                return entry.Content;

            // Load fresh content
            var content = await loader().ConfigureAwait(false);
            entry.Content = content;
            entry.LastWrite = lastWrite;
            entry.Expires = DateTimeOffset.UtcNow.Add(ttl);
            return content;
        }
        finally
        {
            entry.Lock.Release();
        }
    }
}
