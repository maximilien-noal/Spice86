using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spice86.Core.CLI;
using Spice86.MCP;
using Spice86.MCP.Tools;

var builder = Host.CreateApplicationBuilder(args);

// Parse configuration from command line arguments
var configuration = new Configuration();
// Use default configuration for now - in a real scenario, this would parse args

// Create MCP emulator bridge
var mcpBridge = new McpEmulatorBridge(configuration);

// Register MCP bridge components as singletons
builder.Services.AddSingleton(mcpBridge);
builder.Services.AddSingleton(mcpBridge.State);
builder.Services.AddSingleton(mcpBridge.Memory);
builder.Services.AddSingleton(mcpBridge.BreakpointsManager);
builder.Services.AddSingleton(mcpBridge.PauseHandler);

// Register MCP server with emulator tools
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<EmulatorTools>();

builder.Logging.AddConsole(options => {
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

var app = builder.Build();

// Ensure MCP bridge is disposed on shutdown
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() => mcpBridge.Dispose());

await app.RunAsync();
