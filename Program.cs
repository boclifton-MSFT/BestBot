using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.EnableMcpToolMetadata();

// Adapter: map IOptionsSnapshot<ToolOptions> to IOptionsMonitor<ToolOptions>
builder.Services.AddSingleton<IOptionsSnapshot<ToolOptions>>(sp =>
    new OptionsSnapshotFromMonitor<ToolOptions>(sp.GetRequiredService<IOptionsMonitor<ToolOptions>>()));

builder.Build().Run();

// Workaround: The MCP preview extension tries to resolve IOptionsSnapshot<ToolOptions>
// from the root provider. In .NET 9+ this throws. Adapt snapshot from monitor as singleton.
internal sealed class OptionsSnapshotFromMonitor<T> : IOptionsSnapshot<T> where T : class
{
    private readonly IOptionsMonitor<T> _monitor;

    public OptionsSnapshotFromMonitor(IOptionsMonitor<T> monitor)
        => _monitor = monitor;

    public T Value => _monitor.CurrentValue;

    public T Get(string? name) => _monitor.Get(name);
}

// Register the adapter for ToolOptions
internal partial class Program
{
    static Program()
    {
        // This static constructor is used to register services on startup.
        // Since FunctionsApplicationBuilder is already created above, we add via global ServiceCollection extensions.
    }
}
