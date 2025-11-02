using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spice86.Core.CLI;
using Spice86.Core.Emulator.InterruptHandlers.Bios.Structures;
using Spice86.Logging;
using Spice86.MCP;
using Spice86.MCP.Tools;
using Spice86.Shared.Interfaces;

var builder = Host.CreateApplicationBuilder(args);

// Parse configuration from command line arguments
var configuration = new Configuration();
// Use default configuration for now - in a real scenario, this would parse args

// Create shared logger service
var loggerService = new LoggerService();

// NOTE: In production, you should provide actual CfgCpu and BiosDataArea instances
// from your emulator setup. This standalone mode requires all components to be provided.
// For a complete example, see the integration with Spice86DependencyInjection where
// these components are created as part of the full emulator initialization.

// Create minimal BIOS data area for standalone testing
// In production, this should come from the actual emulator setup
var mcpBridge = new McpEmulatorBridge(configuration, loggerService, 
    cfgCpu: null!, // Must be provided in production
    biosDataArea: null!); // Must be provided in production

// Register logger service and MCP bridge components as singletons
builder.Services.AddSingleton<ILoggerService>(loggerService);
builder.Services.AddSingleton(mcpBridge);
builder.Services.AddSingleton(mcpBridge.State);
builder.Services.AddSingleton(mcpBridge.Memory);
builder.Services.AddSingleton(mcpBridge.BreakpointsManager);
builder.Services.AddSingleton(mcpBridge.IOPortDispatcher);
builder.Services.AddSingleton(mcpBridge.CfgCpu);
builder.Services.AddSingleton(mcpBridge.BiosDataArea);
builder.Services.AddSingleton(mcpBridge.PauseHandler);

// Register MCP server with all emulator tools
builder.Services.AddMcpServer()
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

await app.RunAsync();
