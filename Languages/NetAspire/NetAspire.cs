using BestPracticesMcp.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace BestPracticesMcp.Functions;

/// <summary>
/// Provides tools for retrieving best practices for the .NET Aspire cloud-native framework.
/// Utilizes a process-wide cache to minimize disk reads and improve performance.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1852:Type can be sealed", Justification = "Instantiated by Functions host via reflection; suppress analyzer to avoid sealing suggestion")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Functions host via reflection")]
internal class NetAspireTools(ILogger<NetAspireTools> logger)
{
    [Function(nameof(GetNetAspireBestPractices))]
    public async Task<string> GetNetAspireBestPractices(
        [McpToolTrigger("get_netaspire_best_practices", "Retrieves best practices for the .NET Aspire cloud-native framework")]
            ToolInvocationContext toolContext,
        CancellationToken cancellationToken)
    {
        ToolLogging<NetAspireTools>.Serving(logger, "get_netaspire_best_practices");

        var filePath = Path.Combine(AppContext.BaseDirectory, "Languages", "NetAspire", "netaspire-best-practices.md");

        // Fast-path: serve from cache if valid
        if (FileCache.TryGetValid(filePath, out var cached))
        {
            ToolLogging<NetAspireTools>.ServingCached(logger, filePath);
            return cached!;
        }

        try
        {
            var content = await FileCache.GetOrLoadAsync(filePath, TimeSpan.FromMinutes(5), async () =>
            {
                ToolLogging<NetAspireTools>.Loading(logger, filePath);
                return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            if (content is not null)
                return content;

            ToolLogging<NetAspireTools>.FileNotFound(logger, filePath);
        }
        catch (OperationCanceledException)
        {
            // Propagate cancellation so the host can stop gracefully
            throw;
        }
        catch (IOException ex)
        {
            ToolLogging<NetAspireTools>.FailedToLoad(logger, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ToolLogging<NetAspireTools>.FailedToLoad(logger, ex);
        }
        catch (NotSupportedException ex)
        {
            ToolLogging<NetAspireTools>.FailedToLoad(logger, ex);
        }

        string[] fallback = new[]
        {
            "# .NET Aspire Best Practices",
            "- Build cloud-native applications with built-in observability and service discovery.",
            "- Use the Aspire App Host for local development and orchestration.",
            "- Leverage Aspire components for databases, caches, and messaging services.",
            "- Follow container-first development with Docker and Kubernetes deployment.",
            "- Implement health checks and telemetry using OpenTelemetry integration.",
            "- Use managed identities and secure configuration for production deployments.",
            "- Structure applications with clear service boundaries and communication patterns.",
            "- Test services in isolation and with integration tests using test containers.",
            "- Deploy to Azure Container Apps or Kubernetes with proper scaling and monitoring.",
            "- Follow .NET 8+ best practices and embrace async/await patterns throughout."
        };
        return string.Join(Environment.NewLine, fallback);
    }
}