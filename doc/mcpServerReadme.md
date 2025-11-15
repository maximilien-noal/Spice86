# Spice86 MCP Server

## Overview

The Spice86 MCP (Model Context Protocol) Server is an in-process server that exposes emulator state inspection capabilities to AI models and applications. MCP is a standardized protocol introduced by Anthropic that enables AI models to interact with external tools and resources in a consistent way.

## Features

The MCP server provides three tools for inspecting the emulator state:

### 1. Read CPU Registers (`read_cpu_registers`)

Retrieves the current values of all CPU registers, including:
- General purpose registers (EAX, EBX, ECX, EDX, ESI, EDI, ESP, EBP)
- Segment registers (CS, DS, ES, FS, GS, SS)
- Instruction pointer (IP)
- CPU flags (Carry, Parity, Auxiliary, Zero, Sign, Direction, Overflow, Interrupt)

**Usage:**
```json
{
  "jsonrpc": "2.0",
  "method": "tools/call",
  "params": {
    "name": "read_cpu_registers",
    "arguments": {}
  },
  "id": 1
}
```

**Response:**
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "content": [{
      "type": "text",
      "text": "{
        \"generalPurpose\": {
          \"EAX\": 305419896,
          \"EBX\": 2882400001,
          ...
        },
        \"segments\": {
          \"CS\": 4096,
          ...
        },
        \"instructionPointer\": {
          \"IP\": 256
        },
        \"flags\": {
          \"CarryFlag\": false,
          ...
        }
      }"
    }]
  }
}
```

### 2. Read Memory (`read_memory`)

Reads a range of bytes from the emulator's memory.

**Parameters:**
- `address` (integer, required): The starting linear memory address
- `length` (integer, required): The number of bytes to read (maximum 4096)

**Usage:**
```json
{
  "jsonrpc": "2.0",
  "method": "tools/call",
  "params": {
    "name": "read_memory",
    "arguments": {
      "address": 4096,
      "length": 16
    }
  },
  "id": 2
}
```

**Response:**
```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "result": {
    "content": [{
      "type": "text",
      "text": "{
        \"address\": 4096,
        \"length\": 16,
        \"data\": \"0102030405060708090A0B0C0D0E0F10\"
      }"
    }]
  }
}
```

### 3. List Functions (`list_functions`)

Lists known functions from the function catalogue, ordered by call count (most frequently called first).

**Parameters:**
- `limit` (integer, optional): Maximum number of functions to return (default: 100)

**Usage:**
```json
{
  "jsonrpc": "2.0",
  "method": "tools/call",
  "params": {
    "name": "list_functions",
    "arguments": {
      "limit": 10
    }
  },
  "id": 3
}
```

**Response:**
```json
{
  "jsonrpc": "2.0",
  "id": 3,
  "result": {
    "content": [{
      "type": "text",
      "text": "{
        \"functions\": [
          {
            \"address\": \"1000:0000\",
            \"name\": \"MainFunction\",
            \"calledCount\": 42,
            \"hasOverride\": false
          },
          ...
        ],
        \"totalCount\": 125
      }"
    }]
  }
}
```

## Architecture

The MCP server implementation follows these key principles:

1. **External Dependency**: Uses the `ModelContextProtocol.Core` NuGet package for protocol types (e.g., `Tool`, `InitializeResult`, `ListToolsResult`) and standard .NET libraries (System.Text.Json) for JSON-RPC message handling
2. **No Microsoft DI**: Follows Spice86's manual dependency injection pattern
3. **In-Process**: Runs in the same process as the emulator for minimal latency
4. **JSON-RPC 2.0**: Implements the MCP protocol over JSON-RPC 2.0

### Components

- **`IMcpServer`**: Interface defining the MCP server contract
- **`McpServer`**: Implementation of the MCP server with tool handlers
- **`Tool`**: Type (from `ModelContextProtocol.Protocol`) describing available tools

## Integration

The MCP server is instantiated in `Spice86DependencyInjection.cs` and receives:
- `IMemory` - for memory inspection
- `State` - for CPU register inspection  
- `FunctionCatalogue` - for function listing
- `IPauseHandler` - for automatic pause/resume during inspection
- `ILoggerService` - for diagnostic logging

### Thread-Safe State Inspection

The MCP server automatically pauses the emulator before inspecting state and resumes it afterward. This ensures:
- **Consistent snapshots**: State doesn't change mid-inspection
- **Thread safety**: No race conditions when reading registers/memory
- **Automatic management**: Tools handle pause/resume transparently

If the emulator is already paused when a tool is called, the server preserves that state and doesn't resume automatically.

## Protocol Compliance

The server implements core MCP protocol methods:

- `initialize`: Handshake and capability negotiation
- `tools/list`: Enumerate available tools
- `tools/call`: Execute a specific tool

Error handling follows JSON-RPC 2.0 conventions with appropriate error codes:
- `-32700`: Parse error (invalid JSON)
- `-32600`: Invalid request (missing required fields)
- `-32601`: Method not found
- `-32602`: Invalid params
- `-32603`: Internal/tool execution error

## Testing

Integration tests in `tests/Spice86.Tests/McpServerTest.cs` verify:
- Protocol initialization and handshake
- Tool listing and discovery
- CPU register reading
- Memory reading with validation
- Function catalogue querying
- Error handling for malformed requests

All tests use the standard Spice86 test infrastructure with `Spice86Creator` for consistent emulator setup.

## Future Enhancements

Potential future additions:
- Write operations (memory, registers)
- Breakpoint management
- Single-step execution
- Disassembly inspection
- Real-time event streaming

## References

- [Model Context Protocol Specification](https://modelcontextprotocol.io/specification/2025-06-18/basic/transports)
- [JSON-RPC 2.0 Specification](https://www.jsonrpc.org/specification)
- Spice86 Dependency Injection: `src/Spice86/Spice86DependencyInjection.cs`
- GDB Server: `src/Spice86.Core/Emulator/Gdb/GdbServer.cs` (similar remote access pattern)
