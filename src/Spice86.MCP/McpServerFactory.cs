namespace Spice86.MCP;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.CPU.CfgCpu;
using Spice86.Core.Emulator.InterruptHandlers.Bios.Structures;
using Spice86.Core.Emulator.IOPorts;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.VM;
using Spice86.Core.Emulator.VM.Breakpoint;
using Spice86.MCP.Tools;
using Spice86.Shared.Interfaces;

/// <summary>
/// Helper class for creating and configuring MCP server instances for Spice86.
/// </summary>
public static class McpServerFactory {
    /// <summary>
    /// Creates a hosted MCP server instance from individual emulator components.
    /// </summary>
    /// <param name="state">The CPU state containing registers and flags.</param>
    /// <param name="memory">The memory interface.</param>
    /// <param name="breakpointsManager">The breakpoints manager.</param>
    /// <param name="ioPortDispatcher">The IO port dispatcher.</param>
    /// <param name="cfgCpu">The CFG CPU instance.</param>
    /// <param name="biosDataArea">The BIOS data area.</param>
    /// <param name="pauseHandler">The pause handler for emulation control.</param>
    /// <param name="loggerService">Shared logger service instance.</param>
    /// <returns>A configured IHost ready to run the MCP server.</returns>
    public static IHost CreateMcpServerHost(
        State state,
        IMemory memory,
        EmulatorBreakpointsManager breakpointsManager,
        IOPortDispatcher ioPortDispatcher,
        CfgCpu cfgCpu,
        BiosDataArea biosDataArea,
        IPauseHandler pauseHandler,
        ILoggerService loggerService) {
        var builder = Host.CreateApplicationBuilder();

        // Enable DI validation to catch dependency issues at build time
        builder.Services.Configure<ServiceProviderOptions>(options => {
            options.ValidateOnBuild = true;
            options.ValidateScopes = true;
        });

        // Create MCP emulator bridge from individual components with shared logger
        var mcpBridge = new McpEmulatorBridge(
            state,
            memory,
            breakpointsManager,
            ioPortDispatcher,
            cfgCpu,
            biosDataArea,
            pauseHandler,
            loggerService);

        // Register logger service and MCP bridge components as singletons
        builder.Services.AddSingleton(loggerService);
        builder.Services.AddSingleton(mcpBridge);
        builder.Services.AddSingleton(mcpBridge.State);
        builder.Services.AddSingleton(mcpBridge.Memory);
        builder.Services.AddSingleton(mcpBridge.BreakpointsManager);
        builder.Services.AddSingleton(mcpBridge.IOPortDispatcher);
        builder.Services.AddSingleton(mcpBridge.BiosDataArea);
        builder.Services.AddSingleton(mcpBridge.PauseHandler);
        builder.Services.AddSingleton(mcpBridge.CfgCpu);

        // Register MCP server with all emulator tools
        var mcpServerBuilder = builder.Services.AddMcpServer()
            .WithStdioServerTransport()
            .WithTools<EmulatorTools>()
            .WithTools<DisassemblerTools>()
            .WithTools<StructureExplorationTools>()
            .WithTools<StructureViewerTools>()
            .WithTools<ConditionalBreakpointTools>()
            .WithTools<CfgCpuTools>();

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
    /// Creates and runs an MCP server asynchronously from individual emulator components.
    /// This is a convenience method that creates the host and runs it.
    /// </summary>
    /// <param name="state">The CPU state containing registers and flags.</param>
    /// <param name="memory">The memory interface.</param>
    /// <param name="breakpointsManager">The breakpoints manager.</param>
    /// <param name="ioPortDispatcher">The IO port dispatcher.</param>
    /// <param name="cfgCpu">The CFG CPU instance.</param>
    /// <param name="biosDataArea">The BIOS data area.</param>
    /// <param name="pauseHandler">The pause handler for emulation control.</param>
    /// <param name="loggerService">Shared logger service instance.</param>
    /// <param name="cancellationToken">Cancellation token to stop the server.</param>
    /// <returns>A task representing the running server.</returns>
    public static async Task RunMcpServerAsync(
        State state,
        IMemory memory,
        EmulatorBreakpointsManager breakpointsManager,
        IOPortDispatcher ioPortDispatcher,
        CfgCpu cfgCpu,
        BiosDataArea biosDataArea,
        IPauseHandler pauseHandler,
        ILoggerService loggerService,
        CancellationToken cancellationToken = default) {
        using var host = CreateMcpServerHost(
            state,
            memory,
            breakpointsManager,
            ioPortDispatcher,
            cfgCpu,
            biosDataArea,
            pauseHandler,
            loggerService);
        await host.RunAsync(cancellationToken);
    }
}
