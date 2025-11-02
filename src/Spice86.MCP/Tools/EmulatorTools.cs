namespace Spice86.MCP.Tools;

using Iced.Intel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.VM;
using Spice86.Core.Emulator.VM.Breakpoint;
using Spice86.Shared.Emulator.Memory;
using System.ComponentModel;
using System.Globalization;
using System.Text;

/// <summary>
/// MCP tools for inspecting and controlling the Spice86 emulator.
/// </summary>
[McpServerToolType]
public sealed class EmulatorTools {
    private readonly State _state;
    private readonly IMemory _memory;
    private readonly EmulatorBreakpointsManager _breakpointsManager;
    private readonly IPauseHandler _pauseHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmulatorTools"/> class.
    /// </summary>
    /// <param name="state">The CPU state containing registers and flags.</param>
    /// <param name="memory">The memory interface for reading/writing memory.</param>
    /// <param name="breakpointsManager">The breakpoints manager for managing breakpoints.</param>
    /// <param name="pauseHandler">The pause handler for controlling emulation flow.</param>
    public EmulatorTools(State state, IMemory memory, EmulatorBreakpointsManager breakpointsManager, IPauseHandler pauseHandler) {
        _state = state;
        _memory = memory;
        _breakpointsManager = breakpointsManager;
        _pauseHandler = pauseHandler;
    }

    [McpServerTool]
    [Description("Get the current CPU register values from the emulator")]
    public Task<string> GetCpuRegisters() {
        var result = new StringBuilder();
        result.AppendLine("CPU Registers:");
        result.AppendLine($"EAX: 0x{_state.EAX:X8}");
        result.AppendLine($"EBX: 0x{_state.EBX:X8}");
        result.AppendLine($"ECX: 0x{_state.ECX:X8}");
        result.AppendLine($"EDX: 0x{_state.EDX:X8}");
        result.AppendLine($"ESI: 0x{_state.ESI:X8}");
        result.AppendLine($"EDI: 0x{_state.EDI:X8}");
        result.AppendLine($"ESP: 0x{_state.ESP:X8}");
        result.AppendLine($"EBP: 0x{_state.EBP:X8}");
        result.AppendLine();
        result.AppendLine("Segment Registers:");
        result.AppendLine($"CS: 0x{_state.CS:X4}");
        result.AppendLine($"DS: 0x{_state.DS:X4}");
        result.AppendLine($"ES: 0x{_state.ES:X4}");
        result.AppendLine($"FS: 0x{_state.FS:X4}");
        result.AppendLine($"GS: 0x{_state.GS:X4}");
        result.AppendLine($"SS: 0x{_state.SS:X4}");
        result.AppendLine();
        result.AppendLine($"IP: 0x{_state.IP:X4}");
        result.AppendLine($"Flags: 0x{_state.Flags.FlagRegister:X4}");
        result.AppendLine($"Cycles: {_state.Cycles}");
        
        return Task.FromResult(result.ToString());
    }

    [McpServerTool]
    [Description("Read memory from the emulator at a specific address")]
    public Task<string> ReadMemory(
        [Description("The memory address to read from (hex format, e.g., 0x1000)")] string address,
        [Description("The number of bytes to read (default: 16)")] int length = 16) {
        
        if (!TryParseAddress(address, out uint addr)) {
            return Task.FromResult($"Error: Invalid address format '{address}'. Use hex format like 0x1000 or 1000");
        }

        if (length <= 0 || length > 1024) {
            return Task.FromResult($"Error: Length must be between 1 and 1024 bytes.");
        }

        var result = new StringBuilder();
        result.AppendLine($"Memory at 0x{addr:X} (length: {length} bytes):");
        result.AppendLine();
        
        int bytesPerLine = 16;
        for (int i = 0; i < length; i += bytesPerLine) {
            result.Append($"{addr + i:X8}: ");
            
            // Hex bytes
            int lineLength = Math.Min(bytesPerLine, length - i);
            for (int j = 0; j < lineLength; j++) {
                byte value = _memory.SneakilyRead(addr + (uint)(i + j));
                result.Append($"{value:X2} ");
            }
            
            // Padding for alignment
            for (int j = lineLength; j < bytesPerLine; j++) {
                result.Append("   ");
            }
            
            result.Append(" ");
            
            // ASCII representation
            for (int j = 0; j < lineLength; j++) {
                byte value = _memory.SneakilyRead(addr + (uint)(i + j));
                char c = (value >= 32 && value <= 126) ? (char)value : '.';
                result.Append(c);
            }
            
            result.AppendLine();
        }
        
        return Task.FromResult(result.ToString());
    }

    [McpServerTool]
    [Description("Write memory to the emulator at a specific address")]
    public Task<string> WriteMemory(
        [Description("The memory address to write to (hex format, e.g., 0x1000)")] string address,
        [Description("The hex bytes to write, space-separated (e.g., '90 90 90' for three NOP instructions)")] string hexBytes) {
        
        if (!TryParseAddress(address, out uint addr)) {
            return Task.FromResult($"Error: Invalid address format '{address}'. Use hex format like 0x1000 or 1000");
        }

        string[] byteStrings = hexBytes.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (byteStrings.Length == 0) {
            return Task.FromResult("Error: No bytes provided to write.");
        }

        byte[] bytes = new byte[byteStrings.Length];
        for (int i = 0; i < byteStrings.Length; i++) {
            if (!byte.TryParse(byteStrings[i], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bytes[i])) {
                return Task.FromResult($"Error: Invalid hex byte '{byteStrings[i]}' at position {i}.");
            }
        }

        for (int i = 0; i < bytes.Length; i++) {
            _memory.SneakilyWrite(addr + (uint)i, bytes[i]);
        }

        var result = new StringBuilder();
        result.AppendLine($"Successfully wrote {bytes.Length} byte(s) to 0x{addr:X}");
        result.Append("Bytes: ");
        foreach (byte b in bytes) {
            result.Append($"{b:X2} ");
        }
        
        return Task.FromResult(result.ToString());
    }

    [McpServerTool]
    [Description("Get information about the current emulator state")]
    public Task<string> GetEmulatorState() {
        var result = new StringBuilder();
        result.AppendLine("Emulator State:");
        result.AppendLine($"Current IP: 0x{_state.IP:X4}");
        result.AppendLine($"Current CS: 0x{_state.CS:X4}");
        result.AppendLine($"Physical address: 0x{_state.IpPhysicalAddress:X}");
        result.AppendLine($"Cycles: {_state.Cycles}");
        result.AppendLine($"A20 Gate: {(_memory.A20Gate.IsEnabled ? "Enabled" : "Disabled")}");
        
        return Task.FromResult(result.ToString());
    }

    [McpServerTool]
    [Description("Disassemble instructions at a specific address")]
    public Task<string> Disassemble(
        [Description("The memory address to disassemble from (hex format, e.g., 0x1000)")] string address,
        [Description("The number of instructions to disassemble (default: 10)")] int count = 10) {
        
        if (!TryParseAddress(address, out uint addr)) {
            return Task.FromResult($"Error: Invalid address format '{address}'. Use hex format like 0x1000 or 1000");
        }

        if (count <= 0 || count > 100) {
            return Task.FromResult($"Error: Count must be between 1 and 100 instructions.");
        }

        var result = new StringBuilder();
        result.AppendLine($"Disassembly at 0x{addr:X} ({count} instructions):");
        result.AppendLine();

        var codeReader = new MemoryCodeReader(_memory, addr);
        var decoder = Iced.Intel.Decoder.Create(16, codeReader);
        decoder.IP = addr;

        var formatter = new IntelFormatter();
        var output = new StringOutput();

        int instructionCount = 0;
        while (instructionCount < count && decoder.IP < (ulong)_memory.Length) {
            decoder.Decode(out var instruction);
            
            result.Append($"{instruction.IP:X8}: ");
            formatter.Format(instruction, output);
            result.AppendLine(output.ToStringAndReset());
            
            instructionCount++;
            
            if (instruction.Code == Code.INVALID) {
                result.AppendLine("(Invalid instruction encountered)");
                break;
            }
        }
        
        return Task.FromResult(result.ToString());
    }

    [McpServerTool]
    [Description("Add an execution breakpoint at a specific address")]
    public Task<string> AddBreakpoint(
        [Description("The memory address for the breakpoint (hex format, e.g., 0x1000)")] string address) {
        
        if (!TryParseAddress(address, out uint addr)) {
            return Task.FromResult($"Error: Invalid address format '{address}'. Use hex format like 0x1000 or 1000");
        }

        var breakpoint = new AddressBreakPoint(
            Spice86.Shared.Emulator.VM.Breakpoint.BreakPointType.CPU_EXECUTION_ADDRESS, 
            addr, 
            bp => { /* Breakpoint triggered */ }, 
            false);
        _breakpointsManager.ToggleBreakPoint(breakpoint, true);
        
        return Task.FromResult($"Added execution breakpoint at 0x{addr:X}");
    }

    [McpServerTool]
    [Description("Remove an execution breakpoint at a specific address")]
    public Task<string> RemoveBreakpoint(
        [Description("The memory address of the breakpoint to remove (hex format, e.g., 0x1000)")] string address) {
        
        if (!TryParseAddress(address, out uint addr)) {
            return Task.FromResult($"Error: Invalid address format '{address}'. Use hex format like 0x1000 or 1000");
        }

        var breakpoint = new AddressBreakPoint(
            Spice86.Shared.Emulator.VM.Breakpoint.BreakPointType.CPU_EXECUTION_ADDRESS, 
            addr, 
            bp => { /* Breakpoint triggered */ }, 
            false);
        _breakpointsManager.ToggleBreakPoint(breakpoint, false);
        
        return Task.FromResult($"Removed execution breakpoint at 0x{addr:X}");
    }

    [McpServerTool]
    [Description("Add a memory read breakpoint at a specific address")]
    public Task<string> AddMemoryReadBreakpoint(
        [Description("The memory address for the breakpoint (hex format, e.g., 0x1000)")] string address) {
        
        if (!TryParseAddress(address, out uint addr)) {
            return Task.FromResult($"Error: Invalid address format '{address}'. Use hex format like 0x1000 or 1000");
        }

        var breakpoint = new AddressBreakPoint(
            Spice86.Shared.Emulator.VM.Breakpoint.BreakPointType.MEMORY_READ, 
            addr, 
            bp => { /* Breakpoint triggered */ }, 
            false);
        _breakpointsManager.ToggleBreakPoint(breakpoint, true);
        
        return Task.FromResult($"Added memory read breakpoint at 0x{addr:X}");
    }

    [McpServerTool]
    [Description("Add a memory write breakpoint at a specific address")]
    public Task<string> AddMemoryWriteBreakpoint(
        [Description("The memory address for the breakpoint (hex format, e.g., 0x1000)")] string address) {
        
        if (!TryParseAddress(address, out uint addr)) {
            return Task.FromResult($"Error: Invalid address format '{address}'. Use hex format like 0x1000 or 1000");
        }

        var breakpoint = new AddressBreakPoint(
            Spice86.Shared.Emulator.VM.Breakpoint.BreakPointType.MEMORY_WRITE, 
            addr, 
            bp => { /* Breakpoint triggered */ }, 
            false);
        _breakpointsManager.ToggleBreakPoint(breakpoint, true);
        
        return Task.FromResult($"Added memory write breakpoint at 0x{addr:X}");
    }

    [McpServerTool]
    [Description("Add an interrupt breakpoint for a specific interrupt number")]
    public Task<string> AddInterruptBreakpoint(
        [Description("The interrupt number (0-255)")] int interruptNumber) {
        
        if (interruptNumber < 0 || interruptNumber > 255) {
            return Task.FromResult($"Error: Interrupt number must be between 0 and 255.");
        }

        var breakpoint = new AddressBreakPoint(
            Spice86.Shared.Emulator.VM.Breakpoint.BreakPointType.CPU_INTERRUPT, 
            interruptNumber, 
            bp => { /* Breakpoint triggered */ }, 
            false);
        _breakpointsManager.ToggleBreakPoint(breakpoint, true);
        
        return Task.FromResult($"Added interrupt breakpoint for INT 0x{interruptNumber:X2}");
    }

    [McpServerTool]
    [Description("List all active breakpoints")]
    public Task<string> ListBreakpoints() {
        var result = new StringBuilder();
        result.AppendLine("Active Breakpoints:");
        result.AppendLine();
        
        // Note: The BreakpointsManager doesn't expose a way to list all breakpoints
        // This would require enhancing the EmulatorBreakpointsManager
        result.AppendLine("(Breakpoint listing requires EmulatorBreakpointsManager enhancement)");
        result.AppendLine("Breakpoints have been registered but cannot be listed in current implementation.");
        
        return Task.FromResult(result.ToString());
    }

    [McpServerTool]
    [Description("Pause the emulation")]
    public Task<string> PauseEmulation() {
        _pauseHandler.RequestPause("MCP server requested pause");
        return Task.FromResult($"Emulation paused. IsPaused: {_pauseHandler.IsPaused}");
    }

    [McpServerTool]
    [Description("Resume the emulation")]
    public Task<string> ResumeEmulation() {
        _pauseHandler.Resume();
        return Task.FromResult($"Emulation resumed. IsPaused: {_pauseHandler.IsPaused}");
    }

    [McpServerTool]
    [Description("Get the current pause state of the emulation")]
    public Task<string> GetPauseState() {
        return Task.FromResult($"Emulation is {(_pauseHandler.IsPaused ? "paused" : "running")}");
    }

    private static bool TryParseAddress(string addressStr, out uint address) {
        address = 0;
        
        if (string.IsNullOrWhiteSpace(addressStr)) {
            return false;
        }

        // Remove 0x prefix if present
        if (addressStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) {
            addressStr = addressStr.Substring(2);
        }

        return uint.TryParse(addressStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out address);
    }

    /// <summary>
    /// Code reader that reads from emulator memory for disassembly
    /// </summary>
    private sealed class MemoryCodeReader : CodeReader {
        private readonly IMemory _memory;
        private uint _currentAddress;

        public MemoryCodeReader(IMemory memory, uint startAddress) {
            _memory = memory;
            _currentAddress = startAddress;
        }

        public override int ReadByte() {
            if (_currentAddress >= _memory.Length) {
                return -1;
            }
            
            byte value = _memory.SneakilyRead(_currentAddress);
            _currentAddress++;
            return value;
        }
    }

    /// <summary>
    /// String output for formatted disassembly
    /// </summary>
    private sealed class StringOutput : FormatterOutput {
        private readonly StringBuilder _sb = new();

        public override void Write(string text, FormatterTextKind kind) {
            _sb.Append(text);
        }

        public string ToStringAndReset() {
            string result = _sb.ToString();
            _sb.Clear();
            return result;
        }
    }
}
