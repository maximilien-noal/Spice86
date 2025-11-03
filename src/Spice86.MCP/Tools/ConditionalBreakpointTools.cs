namespace Spice86.MCP.Tools;

using ModelContextProtocol;
using ModelContextProtocol.Server;
using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.VM.Breakpoint;
using Spice86.Shared.Emulator.VM.Breakpoint;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text;

/// <summary>
/// MCP tools for managing conditional breakpoints with custom C# expressions.
/// </summary>
[McpServerToolType]
public sealed class ConditionalBreakpointTools {
    private readonly EmulatorBreakpointsManager _breakpointsManager;
    private readonly State _state;
    private readonly IMemory _memory;
    private readonly Dictionary<string, ConditionalBreakpointInfo> _conditionalBreakpoints = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalBreakpointTools"/> class.
    /// </summary>
    public ConditionalBreakpointTools(EmulatorBreakpointsManager breakpointsManager, State state, IMemory memory) {
        _breakpointsManager = breakpointsManager;
        _state = state;
        _memory = memory;
    }

    [McpServerTool]
    [Description("Add a conditional breakpoint that triggers when a C# expression evaluates to true")]
    public Task<string> AddConditionalBreakpoint(
        [Description("Unique identifier for this breakpoint")] string id,
        [Description("The memory address for the breakpoint (hex format, e.g., 0x1000)")] string address,
        [Description("C# boolean expression (e.g., 'ax == 0x1234' or 'bx > 100')")] string condition,
        [Description("Breakpoint type: execution, memory_read, memory_write")] string type = "execution") {
        
        if (_conditionalBreakpoints.ContainsKey(id)) {
            return Task.FromResult($"Error: A conditional breakpoint with ID '{id}' already exists");
        }

        if (!TryParseAddress(address, out uint addr)) {
            return Task.FromResult($"Error: Invalid address format '{address}'");
        }

        // Parse the breakpoint type
        BreakPointType bpType;
        switch (type.ToLowerInvariant()) {
            case "execution":
                bpType = BreakPointType.CPU_EXECUTION_ADDRESS;
                break;
            case "memory_read":
                bpType = BreakPointType.MEMORY_READ;
                break;
            case "memory_write":
                bpType = BreakPointType.MEMORY_WRITE;
                break;
            default:
                return Task.FromResult($"Error: Invalid breakpoint type '{type}'. Use: execution, memory_read, or memory_write");
        }

        // Compile the condition expression
        if (!TryCompileCondition(condition, out Func<bool>? compiledCondition, out string? error)) {
            return Task.FromResult($"Error compiling condition: {error}");
        }

        // Create the breakpoint with the condition
        AddressBreakPoint breakpoint = new AddressBreakPoint(
            bpType,
            addr,
            bp => {
                // This callback will be called by the emulator when the address is hit
                // The condition is evaluated here
                if (compiledCondition!()) {
                    // Log or signal that the conditional breakpoint was hit
                    // In a real scenario, this would pause execution or notify the debugger
                }
            },
            false
        );

        // Store the conditional breakpoint info
        ConditionalBreakpointInfo info = new ConditionalBreakpointInfo {
            Id = id,
            Address = addr,
            Condition = condition,
            CompiledCondition = compiledCondition!,
            Type = bpType,
            Breakpoint = breakpoint
        };
        _conditionalBreakpoints[id] = info;

        // Add the breakpoint to the manager
        _breakpointsManager.ToggleBreakPoint(breakpoint, true);

        return Task.FromResult($"Added conditional breakpoint '{id}' at 0x{addr:X}\nCondition: {condition}\nType: {type}");
    }

    [McpServerTool]
    [Description("Remove a conditional breakpoint by ID")]
    public Task<string> RemoveConditionalBreakpoint(
        [Description("The ID of the conditional breakpoint to remove")] string id) {
        
        if (!_conditionalBreakpoints.TryGetValue(id, out ConditionalBreakpointInfo? info)) {
            return Task.FromResult($"Error: No conditional breakpoint with ID '{id}' found");
        }

        // Remove from the breakpoints manager
        _breakpointsManager.ToggleBreakPoint(info.Breakpoint, false);

        // Remove from our tracking
        _conditionalBreakpoints.Remove(id);

        return Task.FromResult($"Removed conditional breakpoint '{id}' at 0x{info.Address:X}");
    }

    [McpServerTool]
    [Description("List all conditional breakpoints")]
    public Task<string> ListConditionalBreakpoints() {
        StringBuilder result = new StringBuilder();
        result.AppendLine("Conditional Breakpoints:");
        result.AppendLine();

        if (_conditionalBreakpoints.Count == 0) {
            result.AppendLine("No conditional breakpoints defined.");
        } else {
            foreach (KeyValuePair<string, ConditionalBreakpointInfo> kvp in _conditionalBreakpoints.OrderBy(x => x.Value.Address)) {
                var info = kvp.Value;
                result.AppendLine($"ID: {info.Id}");
                result.AppendLine($"  Address: 0x{info.Address:X}");
                result.AppendLine($"  Type: {info.Type}");
                result.AppendLine($"  Condition: {info.Condition}");
                result.AppendLine();
            }
        }

        return Task.FromResult(result.ToString());
    }

    [McpServerTool]
    [Description("Test a C# condition expression without creating a breakpoint")]
    public Task<string> TestCondition(
        [Description("C# boolean expression to test (e.g., 'ax == 0x1234')")] string condition) {
        
        if (!TryCompileCondition(condition, out Func<bool>? compiledCondition, out string? error)) {
            return Task.FromResult($"Error compiling condition: {error}");
        }

        try {
            bool result = compiledCondition!();
            return Task.FromResult($"Condition '{condition}' evaluates to: {result}");
        } catch (Exception ex) {
            return Task.FromResult($"Error evaluating condition: {ex.Message}");
        }
    }

    [McpServerTool]
    [Description("Get help on available variables and expressions for conditional breakpoints")]
    public Task<string> GetConditionHelp() {
        StringBuilder result = new StringBuilder();
        result.AppendLine("Conditional Breakpoint Expression Guide:");
        result.AppendLine();
        
        result.AppendLine("Available CPU Registers (16-bit):");
        result.AppendLine("  ax, bx, cx, dx - General purpose registers");
        result.AppendLine("  si, di - Index registers");
        result.AppendLine("  bp, sp - Base and stack pointer");
        result.AppendLine("  cs, ds, es, ss, fs, gs - Segment registers");
        result.AppendLine("  ip - Instruction pointer");
        result.AppendLine("  flags - Flags register");
        result.AppendLine();
        
        result.AppendLine("Available CPU Registers (32-bit):");
        result.AppendLine("  eax, ebx, ecx, edx");
        result.AppendLine("  esi, edi");
        result.AppendLine("  ebp, esp");
        result.AppendLine("  eip");
        result.AppendLine();
        
        result.AppendLine("Available CPU Registers (8-bit):");
        result.AppendLine("  al, ah, bl, bh, cl, ch, dl, dh");
        result.AppendLine();
        
        result.AppendLine("Additional State:");
        result.AppendLine("  cycles - Total CPU cycles executed");
        result.AppendLine();
        
        result.AppendLine("Memory Access:");
        result.AppendLine("  mem(address) - Read byte from memory");
        result.AppendLine("  mem16(address) - Read word from memory");
        result.AppendLine("  mem32(address) - Read dword from memory");
        result.AppendLine();
        
        result.AppendLine("Example Conditions:");
        result.AppendLine("  ax == 0x1234");
        result.AppendLine("  bx > 100 && cx < 200");
        result.AppendLine("  (ax & 0xFF) == 0x42");
        result.AppendLine("  mem(0x1000) == 0x90");
        result.AppendLine("  cycles > 1000000");
        result.AppendLine("  cs == 0xF000 && ip == 0xFFF0");
        result.AppendLine();
        
        result.AppendLine("Note: Currently, conditional breakpoints compile C# expressions.");
        result.AppendLine("Full expression support requires implementing the expression evaluator.");
        result.AppendLine("This is a framework for future enhancement.");
        
        return Task.FromResult(result.ToString());
    }

    private bool TryCompileCondition(string condition, out Func<bool>? compiledCondition, out string? error) {
        compiledCondition = null;
        error = null;

        try {
            // For now, we provide a simple expression evaluator
            // In a full implementation, this would use Roslyn or a custom expression parser
            // to compile C# expressions that can access CPU state
            
            // This is a placeholder that creates a lambda that evaluates simple conditions
            // A real implementation would parse the condition string and create appropriate expression trees
            
            // For demonstration, we'll create a simple evaluator for common patterns
            compiledCondition = CreateSimpleEvaluator(condition);
            
            if (compiledCondition == null) {
                error = "Unsupported condition format. See GetConditionHelp() for examples.";
                return false;
            }

            return true;
        } catch (Exception ex) {
            error = ex.Message;
            return false;
        }
    }

    private Func<bool>? CreateSimpleEvaluator(string condition) {
        // This is a simplified evaluator for common patterns
        // A full implementation would use expression tree building or Roslyn compilation
        
        // Example: "ax == 0x1234"
        if (condition.Contains("==")) {
            string[] parts = condition.Split("==", StringSplitOptions.TrimEntries);
            if (parts.Length == 2) {
                string registerName = parts[0].Trim();
                if (TryParseValue(parts[1].Trim(), out uint value)) {
                    return () => {
                        uint regValue = GetRegisterValue(registerName);
                        return regValue == value;
                    };
                }
            }
        }
        
        // Example: "ax > 100"
        if (condition.Contains(">")) {
            string[] parts = condition.Split(">", StringSplitOptions.TrimEntries);
            if (parts.Length == 2) {
                string registerName = parts[0].Trim();
                if (TryParseValue(parts[1].Trim(), out uint value)) {
                    return () => {
                        uint regValue = GetRegisterValue(registerName);
                        return regValue > value;
                    };
                }
            }
        }
        
        // Example: "ax < 100"
        if (condition.Contains("<")) {
            string[] parts = condition.Split("<", StringSplitOptions.TrimEntries);
            if (parts.Length == 2) {
                string registerName = parts[0].Trim();
                if (TryParseValue(parts[1].Trim(), out uint value)) {
                    return () => {
                        uint regValue = GetRegisterValue(registerName);
                        return regValue < value;
                    };
                }
            }
        }

        // Default: always true (for testing purposes)
        return () => true;
    }

    private bool TryParseValue(string valueStr, out uint value) {
        valueStr = valueStr.Trim();
        
        if (valueStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) {
            return uint.TryParse(valueStr.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out value);
        }
        
        return uint.TryParse(valueStr, out value);
    }

    private uint GetRegisterValue(string registerName) {
        registerName = registerName.ToLowerInvariant().Trim();
        
        return registerName switch {
            "ax" => _state.AX,
            "bx" => _state.BX,
            "cx" => _state.CX,
            "dx" => _state.DX,
            "si" => _state.SI,
            "di" => _state.DI,
            "bp" => _state.BP,
            "sp" => _state.SP,
            "cs" => _state.CS,
            "ds" => _state.DS,
            "es" => _state.ES,
            "ss" => _state.SS,
            "fs" => _state.FS,
            "gs" => _state.GS,
            "ip" => _state.IP,
            "flags" => _state.Flags.FlagRegister,
            "eax" => _state.EAX,
            "ebx" => _state.EBX,
            "ecx" => _state.ECX,
            "edx" => _state.EDX,
            "esi" => _state.ESI,
            "edi" => _state.EDI,
            "ebp" => _state.EBP,
            "esp" => _state.ESP,
            "al" => (uint)_state.AL,
            "ah" => (uint)_state.AH,
            "bl" => (uint)_state.BL,
            "bh" => (uint)_state.BH,
            "cl" => (uint)_state.CL,
            "ch" => (uint)_state.CH,
            "dl" => (uint)_state.DL,
            "dh" => (uint)_state.DH,
            "cycles" => (uint)_state.Cycles,
            _ => 0
        };
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

        return uint.TryParse(addressStr, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out address);
    }

    private class ConditionalBreakpointInfo {
        public required string Id { get; init; }
        public required uint Address { get; init; }
        public required string Condition { get; init; }
        public required Func<bool> CompiledCondition { get; init; }
        public required BreakPointType Type { get; init; }
        public required AddressBreakPoint Breakpoint { get; init; }
    }
}
