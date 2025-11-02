# Spice86.MCP - Model Context Protocol Server

This project implements a Model Context Protocol (MCP) server for the Spice86 emulator, enabling AI tools and external applications to interact with the emulator's live state.

## What is MCP?

The Model Context Protocol (MCP) is an open protocol that standardizes how applications provide context to Large Language Models (LLMs). It enables secure integration between LLMs and various data sources and tools.

For more information:
- [Official Documentation](https://modelcontextprotocol.io/)
- [Protocol Specification](https://spec.modelcontextprotocol.io/)
- [C# SDK Repository](https://github.com/modelcontextprotocol/csharp-sdk)

## Features

The MCP server exposes the following tools for interacting with the emulator:

### CPU & Memory Inspection
- **GetCpuRegisters**: Get current CPU register values (EAX, EBX, ECX, CS, IP, flags, cycles, etc.)
- **ReadMemory**: Read memory from a specific address with hex dump format
- **WriteMemory**: Write bytes to memory at a specific address
- **GetEmulatorState**: Get overall emulator state (cycles, A20 gate, physical address)

### Disassembly
- **Disassemble**: Disassemble instructions at a specific address using Iced.Intel

### Breakpoints
- **AddBreakpoint**: Add execution breakpoint at a specific address
- **RemoveBreakpoint**: Remove execution breakpoint
- **AddMemoryReadBreakpoint**: Add breakpoint on memory reads
- **AddMemoryWriteBreakpoint**: Add breakpoint on memory writes
- **AddInterruptBreakpoint**: Add breakpoint on specific interrupts (INT 0x00-0xFF)
- **ListBreakpoints**: List all active breakpoints

### Emulation Control
- **PauseEmulation**: Pause the emulator execution
- **ResumeEmulation**: Resume the emulator execution
- **GetPauseState**: Query current pause state

### Future Features
- CFG CPU graph exploration
- MemoryBasedDataStructures interaction
- Conditional breakpoints with custom C# conditions
- CFGCPU disassembler/AST integration (when Iced is replaced)

## Building

```bash
dotnet build src/Spice86.MCP/Spice86.MCP.csproj
```

## Running

The MCP server uses stdio transport for communication:

```bash
dotnet run --project src/Spice86.MCP/Spice86.MCP.csproj
```

## Usage with AI Tools

The MCP server can be used with any MCP-compatible AI tool or LLM client. Configure your AI tool to connect to this server via stdio transport.

### Example Configuration (for Claude Desktop)

Add to your `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "spice86": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/Spice86/src/Spice86.MCP/Spice86.MCP.csproj"]
    }
  }
}
```

## Architecture

The MCP server is structured as follows:

```
Spice86.MCP/
├── Program.cs           # Entry point and server configuration
├── Tools/
│   └── EmulatorTools.cs # MCP tool implementations
└── README.md
```

The server uses the `ModelContextProtocol` NuGet package and follows the patterns from the official C# SDK examples.

## Requirements

- .NET 10 RC2 or later
- ModelContextProtocol package (0.4.0-preview.3)

## Future Enhancements

- Integration with live emulator instance
- Support for multiple concurrent connections
- HTTP transport option (via ModelContextProtocol.AspNetCore)
- Authentication and security features
- Streaming support for large data queries
- Event notifications for emulator state changes
