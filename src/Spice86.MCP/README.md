# Spice86.MCP - Model Context Protocol Server

This project implements a comprehensive Model Context Protocol (MCP) server for the Spice86 emulator, enabling AI tools and external applications to interact with the emulator's live state, explore DOS/BIOS structures, analyze control flow, and perform advanced debugging operations.

## What is MCP?

The Model Context Protocol (MCP) is an open protocol that standardizes how applications provide context to Large Language Models (LLMs). It enables secure integration between LLMs and various data sources and tools.

For more information:
- [Official Documentation](https://modelcontextprotocol.io/)
- [Protocol Specification](https://spec.modelcontextprotocol.io/)
- [C# SDK Repository](https://github.com/modelcontextprotocol/csharp-sdk)

## Features

The MCP server exposes comprehensive tools organized into categories:

### EmulatorTools - Core Emulation Control
- **GetCpuRegisters**: Get current CPU register values (EAX, EBX, ECX, CS, IP, flags, cycles, etc.)
- **ReadMemory**: Read memory from a specific address with hex dump format
- **WriteMemory**: Write bytes to memory at a specific address
- **GetEmulatorState**: Get overall emulator state (cycles, A20 gate, physical address)
- **Disassemble**: Disassemble instructions at a specific address using Iced.Intel
- **AddBreakpoint**: Add execution breakpoint at a specific address
- **RemoveBreakpoint**: Remove execution breakpoint
- **AddMemoryReadBreakpoint**: Add breakpoint on memory reads
- **AddMemoryWriteBreakpoint**: Add breakpoint on memory writes
- **AddMemoryAccessBreakpoint**: Add breakpoint on memory read or write
- **AddInterruptBreakpoint**: Add breakpoint on specific interrupts (INT 0x00-0xFF)
- **AddIoReadBreakpoint**: Add breakpoint on IO port reads
- **AddIoWriteBreakpoint**: Add breakpoint on IO port writes
- **AddIoAccessBreakpoint**: Add breakpoint on IO port read or write
- **ListBreakpoints**: List all active breakpoints
- **PauseEmulation**: Pause the emulator execution
- **ResumeEmulation**: Resume the emulator execution
- **GetPauseState**: Query current pause state

### CfgCpuTools - Control Flow Graph Exploration
- **GetExecutionContext**: Get information about the current CFG execution context
- **ListEntryPoints**: List all entry points registered in the CFG
- **GetCfgNodeInfo**: Get information about a specific CFG node at an address
- **GetCfgStatistics**: Get statistics about CFG execution
- **ExploreControlFlow**: Explore the control flow graph starting from a specific address

### StructureExplorationTools - DOS/BIOS/Device Exploration
- **GetBiosDataAreaInfo**: View BIOS Data Area structure and values
- **GetDosStructureTypes**: Get documentation about DOS structure types
- **GetBiosStructureTypes**: Get documentation about BIOS structure types
- **GetIoPortHandlers**: Get information about registered IO port handlers
- **GetDeviceInfo**: Get detailed information about device categories (DMA, PIC, Timer, Keyboard, Serial, Parallel, Video, Sound, MIDI, Joystick)

### StructureViewerTools - Memory Structure Interpretation
- **GetStructureDefinitions**: Get definitions for common DOS/BIOS memory structures
- **ReadStructure**: Read and interpret memory structures (BDA, PSP, MCB, IVT entries)

### DisassemblerTools - ICED Integration & AST Preparation
- **DisassembleDetailed**: Disassemble with detailed information including operands and flags
- **AnalyzeControlFlow**: Analyze control flow instructions (jumps, calls, returns)
- **GetInstructionOperandInfo**: Get operand information for AST construction
- **GetInstructionEncoding**: Extract instruction opcodes and operand patterns

### ConditionalBreakpointTools - Advanced Breakpoint Management
- **AddConditionalBreakpoint**: Add breakpoint with C# expression condition
- **RemoveConditionalBreakpoint**: Remove conditional breakpoint by ID
- **ListConditionalBreakpoints**: List all conditional breakpoints
- **TestCondition**: Test a C# condition expression
- **GetConditionHelp**: Get help on available variables and expressions

## Building

```bash
dotnet build src/Spice86.MCP/Spice86.MCP.csproj
```

## Running

### Standalone Mode (Testing)
The MCP server can run standalone with minimal emulator components:

```bash
dotnet run --project src/Spice86.MCP/Spice86.MCP.csproj
```

### Integration with Spice86 Machine
Create an MCP server instance from a fully-configured Machine:

```csharp
using Spice86.MCP;
using Spice86.Logging;

// After creating your Machine instance with a shared logger
var loggerService = new LoggerService();
var machine = new Machine(...);

// Create and run the MCP server with shared logger
var mcpHost = McpServerFactory.CreateMcpServerHost(machine, loggerService);
await mcpHost.RunAsync();

// Or use the convenience method
await McpServerFactory.RunMcpServerAsync(machine, loggerService, cancellationToken);
```

## Usage with AI Tools

The MCP server can be used with any MCP-compatible AI tool or LLM client.

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

## Tool Examples

### Explore Control Flow
```
Tool: GetExecutionContext
Returns: Current CFG execution context with entry point, depth, and next node information
```

### Read BIOS Data Area
```
Tool: GetBiosDataAreaInfo
Returns: COM/LPT ports, equipment flags, memory size, video mode, timer counter, etc.
```

### Conditional Breakpoint
```
Tool: AddConditionalBreakpoint
Parameters:
  - id: "ax_check"
  - address: "0x1234"
  - condition: "ax == 0x1234"
  - type: "execution"
```

### Detailed Disassembly
```
Tool: DisassembleDetailed
Parameters:
  - address: "0xF000:0xFFF0"
  - count: 5
Returns: Instruction bytes, opcodes, operands, control flow info, CPU flags affected
```

### Device Information
```
Tool: GetDeviceInfo
Parameters:
  - deviceCategory: "keyboard"
Returns: Keyboard controller ports, commands, and documentation
```

### Read Structure
```
Tool: ReadStructure
Parameters:
  - structureType: "psp"
  - segment: "0x1000"
Returns: Interpreted PSP structure with all fields
```

## Architecture

```
Spice86.MCP/
├── Program.cs              # Standalone entry point
├── McpEmulatorBridge.cs    # Bridge between MCP and Spice86
├── McpServerFactory.cs     # Factory for creating MCP servers
├── Tools/
│   ├── EmulatorTools.cs             # Core emulation tools
│   ├── CfgCpuTools.cs               # CFG exploration tools
│   ├── StructureExplorationTools.cs # DOS/BIOS/Device exploration
│   ├── StructureViewerTools.cs      # Memory structure interpretation
│   ├── DisassemblerTools.cs         # ICED disassembly & AST prep
│   └── ConditionalBreakpointTools.cs # Advanced breakpoints
└── README.md
```

## Requirements

- .NET 8.0 or later
- ModelContextProtocol package (0.4.0-preview.3)
- Microsoft.Extensions.Hosting
- ICED disassembler library
- Spice86.Core

## License

This project is part of Spice86 and follows the same Apache 2.0 license.
