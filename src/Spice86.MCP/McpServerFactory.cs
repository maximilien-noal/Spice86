namespace Spice86.MCP;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spice86.Core.Emulator.VM;
using Spice86.MCP.Tools;

/// <summary>
/// Helper class for creating and configuring MCP server instances for Spice86.
/// </summary>
public static class McpServerFactory {
    /// <summary>
    /// Creates a hosted MCP server instance using a fully-configured Machine.
    /// </summary>
    /// <param name="machine">The configured Machine instance from Spice86DependencyInjection.</param>
    /// <returns>A configured IHost ready to run the MCP server.</returns>
    public static IHost CreateMcpServerHost(Machine machine) {
        var builder = Host.CreateApplicationBuilder();

        // Create MCP emulator bridge from the Machine
        var mcpBridge = new McpEmulatorBridge(machine);

        // Register MCP bridge components as singletons
        builder.Services.AddSingleton(mcpBridge);
        builder.Services.AddSingleton(mcpBridge.State);
        builder.Services.AddSingleton(mcpBridge.Memory);
        builder.Services.AddSingleton(mcpBridge.BreakpointsManager);
        builder.Services.AddSingleton(mcpBridge.IOPortDispatcher);

        // Register optional components (may be null in some configurations)
        if (mcpBridge.BiosDataArea != null) {
            builder.Services.AddSingleton(mcpBridge.BiosDataArea);
        }

        // Register MCP server with all emulator tools
        var mcpServerBuilder = builder.Services.AddMcpServer()
            .WithStdioServerTransport()
            .WithTools<EmulatorTools>()
            .WithTools<DisassemblerTools>()
            .WithTools<StructureExplorationTools>()
            .WithTools<StructureViewerTools>()
            .WithTools<ConditionalBreakpointTools>();

        // Add CFG CPU tools if CFG CPU is available
        if (mcpBridge.CfgCpu != null) {
            builder.Services.AddSingleton(mcpBridge.CfgCpu);
            mcpServerBuilder.WithTools<CfgCpuTools>();
        }

        builder.Logging.AddConsole(options => {
            options.LogToStandardErrorThreshold = LogLevel.Trace;
        });

        var app = builder.Build();

        // Ensure MCP bridge is disposed on shutdown
        var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStopping.Register(() => mcpBridge.Dispose());

        return app;
    }

    /// <summary>
    /// Creates and runs an MCP server asynchronously using a fully-configured Machine.
    /// This is a convenience method that creates the host and runs it.
    /// </summary>
    /// <param name="machine">The configured Machine instance.</param>
    /// <param name="cancellationToken">Cancellation token to stop the server.</param>
    /// <returns>A task representing the running server.</returns>
    public static async Task RunMcpServerAsync(Machine machine, CancellationToken cancellationToken = default) {
        using var host = CreateMcpServerHost(machine);
        await host.RunAsync(cancellationToken);
    }
}
