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

// Create emulator service
var emulatorService = new EmulatorService(configuration);

// Register emulator service as singleton
builder.Services.AddSingleton(emulatorService);
builder.Services.AddSingleton(emulatorService.State);
builder.Services.AddSingleton(emulatorService.Memory);
builder.Services.AddSingleton(emulatorService.BreakpointsManager);
builder.Services.AddSingleton(emulatorService.PauseHandler);

// Register MCP server with emulator tools
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<EmulatorTools>();

builder.Logging.AddConsole(options => {
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

var app = builder.Build();

// Ensure emulator service is disposed on shutdown
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() => emulatorService.Dispose());

await app.RunAsync();
