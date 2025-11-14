# MCP Server Usage Example

This document provides a practical example of using the Spice86 MCP server to inspect emulator state.

## Basic Usage

The MCP server is automatically created when you instantiate `Spice86DependencyInjection`. Here's a complete example:

```csharp
using Spice86;
using Spice86.Core.CLI;
using Spice86.Core.Emulator.Mcp;

// Create configuration for a DOS program
var configuration = new Configuration {
    Exe = "path/to/program.exe",
    HeadlessMode = HeadlessType.Minimal,
    GdbPort = 0 // Disable GDB server if not needed
};

// Create the emulator with dependency injection
using var spice86 = new Spice86DependencyInjection(configuration);

// Access the MCP server
IMcpServer mcpServer = spice86.McpServer;

// Example 1: Initialize the MCP connection
string initRequest = """
{
  "jsonrpc": "2.0",
  "method": "initialize",
  "params": {
    "protocolVersion": "2025-06-18"
  },
  "id": 1
}
""";

string initResponse = mcpServer.HandleRequest(initRequest);
Console.WriteLine("Initialize Response:");
Console.WriteLine(initResponse);

// Example 2: List available tools
string toolsListRequest = """
{
  "jsonrpc": "2.0",
  "method": "tools/list",
  "id": 2
}
""";

string toolsListResponse = mcpServer.HandleRequest(toolsListRequest);
Console.WriteLine("\nAvailable Tools:");
Console.WriteLine(toolsListResponse);

// Example 3: Read CPU registers
string readRegistersRequest = """
{
  "jsonrpc": "2.0",
  "method": "tools/call",
  "params": {
    "name": "read_cpu_registers",
    "arguments": {}
  },
  "id": 3
}
""";

string registersResponse = mcpServer.HandleRequest(readRegistersRequest);
Console.WriteLine("\nCPU Registers:");
Console.WriteLine(registersResponse);

// Example 4: Read memory at a specific address
string readMemoryRequest = """
{
  "jsonrpc": "2.0",
  "method": "tools/call",
  "params": {
    "name": "read_memory",
    "arguments": {
      "address": 0,
      "length": 256
    }
  },
  "id": 4
}
""";

string memoryResponse = mcpServer.HandleRequest(readMemoryRequest);
Console.WriteLine("\nMemory Contents:");
Console.WriteLine(memoryResponse);

// Example 5: List functions
string listFunctionsRequest = """
{
  "jsonrpc": "2.0",
  "method": "tools/call",
  "params": {
    "name": "list_functions",
    "arguments": {
      "limit": 20
    }
  },
  "id": 5
}
""";

string functionsResponse = mcpServer.HandleRequest(listFunctionsRequest);
Console.WriteLine("\nFunction Catalogue:");
Console.WriteLine(functionsResponse);
```

## Integration with Debuggers

The MCP server can be used alongside the GDB server for comprehensive debugging:

```csharp
var configuration = new Configuration {
    Exe = "game.exe",
    HeadlessMode = HeadlessType.Minimal,
    GdbPort = 10000,  // Enable GDB server on port 10000
    Debug = true      // Start paused
};

using var spice86 = new Spice86DependencyInjection(configuration);

// Use GDB for step-by-step debugging
// Use MCP server for programmatic state inspection
IMcpServer mcpServer = spice86.McpServer;

// You can query state at any point
string state = mcpServer.HandleRequest("""
{
  "jsonrpc": "2.0",
  "method": "tools/call",
  "params": {
    "name": "read_cpu_registers",
    "arguments": {}
  },
  "id": 1
}
""");
```

## Automated Testing

The MCP server is particularly useful for automated testing and verification:

```csharp
// Load a test program
var config = new Configuration {
    Exe = "test_program.com",
    HeadlessMode = HeadlessType.Minimal
};

using var emulator = new Spice86DependencyInjection(config);

// Run the program for a certain number of cycles
// (integrate with your execution logic)

// Verify final state using MCP server
var mcpServer = emulator.McpServer;

// Check that AX register has expected value
string response = mcpServer.HandleRequest("""
{
  "jsonrpc": "2.0",
  "method": "tools/call",
  "params": {
    "name": "read_cpu_registers",
    "arguments": {}
  },
  "id": 1
}
""");

// Parse response and assert values
// (integrate with your test framework)
```

## Real-time Monitoring

Create a monitoring tool that periodically samples emulator state:

```csharp
var config = new Configuration {
    Exe = "application.exe",
    HeadlessMode = HeadlessType.Minimal
};

using var emulator = new Spice86DependencyInjection(config);
var mcpServer = emulator.McpServer;

// Start emulation in a background task
Task.Run(() => emulator.ProgramExecutor.Run());

// Monitor state every 100ms
using var timer = new System.Timers.Timer(100);
timer.Elapsed += (sender, args) => {
    string registers = mcpServer.HandleRequest("""
    {
      "jsonrpc": "2.0",
      "method": "tools/call",
      "params": {
        "name": "read_cpu_registers",
        "arguments": {}
      },
      "id": 1
    }
    """);
    
    // Log or visualize state
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {registers}");
};
timer.Start();

// Keep running until user stops
Console.WriteLine("Press Enter to stop monitoring...");
Console.ReadLine();
```

## Notes

- The MCP server is **thread-safe** and can be called from any thread
- Requests are **synchronous** - the server processes one request at a time
- The server does **not** modify emulator state - it's read-only by design
- All responses follow **JSON-RPC 2.0** format with proper error handling
- Memory reads are **limited to 4096 bytes** per request for safety

## Error Handling

Always handle potential errors in responses:

```csharp
var response = mcpServer.HandleRequest(request);
using var doc = JsonDocument.Parse(response);
var root = doc.RootElement;

if (root.TryGetProperty("error", out var error)) {
    var code = error.GetProperty("code").GetInt32();
    var message = error.GetProperty("message").GetString();
    Console.WriteLine($"Error {code}: {message}");
} else if (root.TryGetProperty("result", out var result)) {
    // Process successful result
    Console.WriteLine("Success!");
}
```
