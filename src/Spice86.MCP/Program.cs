using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spice86.Core.CLI;
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

// Create MCP emulator bridge in standalone mode with shared logger
var mcpBridge = new McpEmulatorBridge(configuration, loggerService);

// Register logger service and MCP bridge components as singletons
builder.Services.AddSingleton<ILoggerService>(loggerService);
builder.Services.AddSingleton(mcpBridge);
builder.Services.AddSingleton(mcpBridge.State);
builder.Services.AddSingleton(mcpBridge.Memory);
builder.Services.AddSingleton(mcpBridge.BreakpointsManager);
builder.Services.AddSingleton(mcpBridge.IOPortDispatcher);

// Register optional components (may be null in standalone mode)
if (mcpBridge.CfgCpu != null) {
    builder.Services.AddSingleton(mcpBridge.CfgCpu);
}
if (mcpBridge.BiosDataArea != null) {
    builder.Services.AddSingleton(mcpBridge.BiosDataArea);
}
if (mcpBridge.PauseHandler != null) {
    builder.Services.AddSingleton(mcpBridge.PauseHandler);
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
    mcpServerBuilder.WithTools<CfgCpuTools>();
}

builder.Logging.AddConsole(options => {
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

var app = builder.Build();

// Ensure MCP bridge is disposed on shutdown
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() => mcpBridge.Dispose());

await app.RunAsync();
