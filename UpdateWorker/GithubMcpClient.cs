using BestPracticesMcp.UpdateWorker.Models;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;

namespace BestPracticesMcp.UpdateWorker;

/// <summary>
/// Wraps the GitHub remote MCP server via HTTP transport.
/// Connects to <c>https://api.githubcopilot.com/mcp/</c> using a PAT for auth
/// and exposes the server's tools as <see cref="McpClientTool"/> instances
/// that inherit from <see cref="Microsoft.Extensions.AI.AIFunction"/>.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by DI container")]
internal sealed class GithubMcpClient : IAsyncDisposable
{
    private static readonly Uri GitHubMcpEndpoint = new("https://api.githubcopilot.com/mcp/");

    private readonly string _token;
    private McpClient? _client;
    private HttpClientTransport? _transport;
    private IList<McpClientTool>? _tools;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public GithubMcpClient(IOptions<UpdateWorkerOptions> options)
    {
        _token = options.Value.GitHubToken;
    }

    /// <summary>
    /// Returns the MCP tools exposed by the GitHub remote server.
    /// Lazily initializes the connection on first call.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "HttpClient ownership is transferred to HttpClientTransport via ownsHttpClient: true")]
    public async Task<IList<McpClientTool>> GetToolsAsync(CancellationToken cancellationToken = default)
    {
        if (_tools is not null)
        {
            return _tools;
        }

        await _initLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Double-check after acquiring lock (valid concurrency pattern)
#pragma warning disable CA1508 // Avoid dead conditional code
            if (_tools is not null)
            {
                return _tools;
            }
#pragma warning restore CA1508

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

            _transport = new HttpClientTransport(
                new HttpClientTransportOptions
                {
                    Endpoint = GitHubMcpEndpoint,
                    TransportMode = HttpTransportMode.AutoDetect,
                    Name = "GithubMcpServer",
                    AdditionalHeaders = new Dictionary<string, string>
                    {
                        // Limit to only the toolsets we need for PR creation and repo interaction
                        ["X-MCP-Toolsets"] = "repos,pull_requests",
                    },
                },
                httpClient,
                ownsHttpClient: true);

#pragma warning disable CA2016 // API does not accept CancellationToken
            _client = await McpClient.CreateAsync(_transport).ConfigureAwait(false);
            _tools = await _client.ListToolsAsync().ConfigureAwait(false);
#pragma warning restore CA2016
            return _tools;
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_client is not null)
        {
            await _client.DisposeAsync().ConfigureAwait(false);
        }

        if (_transport is not null)
        {
            await _transport.DisposeAsync().ConfigureAwait(false);
        }

        _initLock.Dispose();
    }
}