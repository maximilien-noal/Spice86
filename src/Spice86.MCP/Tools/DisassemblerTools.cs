namespace Spice86.MCP.Tools;

using Iced.Intel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Spice86.Core.Emulator.Memory;
using Spice86.Shared.Utils;
using System.ComponentModel;
using System.Text;

/// <summary>
/// MCP tools for disassembly and instruction analysis using ICED disassembler.
/// Provides a generic C# interface for working with x86 instructions and AST preparation.
/// </summary>
[McpServerToolType]
public sealed class DisassemblerTools {
    private readonly IMemory _memory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisassemblerTools"/> class.
    /// </summary>
    /// <param name="memory">The memory interface for reading instruction bytes.</param>
    public DisassemblerTools(IMemory memory) {
        _memory = memory;
    }

    [McpServerTool]
    [Description("Disassemble instructions at an address with detailed information including operands and flags")]
    public Task<string> DisassembleDetailed(
        [Description("The memory address to disassemble from (hex format, e.g., 0x1000)")] string address,
        [Description("The number of instructions to disassemble (default: 10)")] int count = 10) {
        
        if (!TryParseAddress(address, out uint addr)) {
            return Task.FromResult($"Error: Invalid address format '{address}'. Use hex format like 0x1000 or 1000");
        }

        if (count <= 0 || count > 100) {
            return Task.FromResult($"Error: Count must be between 1 and 100 instructions.");
        }

        var result = new StringBuilder();
        result.AppendLine($"Detailed Disassembly at 0x{addr:X} ({count} instructions):");
        result.AppendLine();

        var codeReader = new MemoryCodeReader(_memory, addr);
        var decoder = Iced.Intel.Decoder.Create(16, codeReader);
        decoder.IP = addr;

        int instructionCount = 0;
        while (instructionCount < count && decoder.IP < (ulong)_memory.Length) {
            decoder.Decode(out var instruction);
            
            result.AppendLine($"Address: 0x{instruction.IP:X8}");
            result.AppendLine($"Length: {instruction.Length} bytes");
            result.AppendLine($"Mnemonic: {instruction.Mnemonic}");
            result.AppendLine($"Op Code: {instruction.Code}");
            
            // Get instruction bytes
            var instrBytes = new byte[instruction.Length];
            for (int i = 0; i < instruction.Length; i++) {
                instrBytes[i] = _memory.SneakilyRead((uint)(instruction.IP + (ulong)i));
            }
            result.Append("Bytes: ");
            foreach (byte b in instrBytes) {
                result.Append($"{b:X2} ");
            }
            result.AppendLine();
            
            // Operand information
            result.AppendLine($"Operand Count: {instruction.OpCount}");
            for (int i = 0; i < instruction.OpCount; i++) {
                var opKind = instruction.GetOpKind(i);
                result.AppendLine($"  Operand {i}: {opKind}");
                
                switch (opKind) {
                    case OpKind.Register:
                        result.AppendLine($"    Register: {instruction.GetOpRegister(i)}");
                        break;
                    case OpKind.Memory:
                        result.AppendLine($"    Memory Segment: {instruction.MemorySegment}");
                        result.AppendLine($"    Memory Base: {instruction.MemoryBase}");
                        result.AppendLine($"    Memory Index: {instruction.MemoryIndex}");
                        result.AppendLine($"    Memory Displacement: 0x{instruction.MemoryDisplacement64:X}");
                        break;
                    case OpKind.Immediate8:
                        result.AppendLine($"    Immediate8: 0x{instruction.GetImmediate(i):X2}");
                        break;
                    case OpKind.Immediate16:
                        result.AppendLine($"    Immediate16: 0x{instruction.GetImmediate(i):X4}");
                        break;
                    case OpKind.Immediate32:
                        result.AppendLine($"    Immediate32: 0x{instruction.GetImmediate(i):X8}");
                        break;
                    case OpKind.NearBranch16:
                    case OpKind.NearBranch32:
                    case OpKind.NearBranch64:
                        result.AppendLine($"    Branch Target: 0x{instruction.NearBranchTarget:X}");
                        break;
                }
            }
            
            // Control flow information
            result.AppendLine($"Flow Control: {instruction.FlowControl}");
            bool isControlFlow = instruction.FlowControl != FlowControl.Next;
            result.AppendLine($"Is Control Flow: {isControlFlow}");
            bool isJump = instruction.FlowControl == FlowControl.UnconditionalBranch || 
                          instruction.FlowControl == FlowControl.IndirectBranch;
            bool isCall = instruction.FlowControl == FlowControl.Call || 
                          instruction.FlowControl == FlowControl.IndirectCall;
            bool isReturn = instruction.FlowControl == FlowControl.Return;
            result.AppendLine($"Is Jump: {isJump}");
            result.AppendLine($"Is Call: {isCall}");
            result.AppendLine($"Is Return: {isReturn}");
            
            // CPU flags
            result.AppendLine($"RFLAGS Read: {instruction.RflagsRead}");
            result.AppendLine($"RFLAGS Written: {instruction.RflagsWritten}");
            result.AppendLine($"RFLAGS Modified: {instruction.RflagsModified}");
            
            result.AppendLine();
            
            instructionCount++;
            
            if (instruction.Code == Code.INVALID) {
                result.AppendLine("(Invalid instruction encountered)");
                break;
            }
        }
        
        return Task.FromResult(result.ToString());
    }

    [McpServerTool]
    [Description("Analyze control flow instructions (jumps, calls, returns) at an address")]
    public Task<string> AnalyzeControlFlow(
        [Description("The memory address to analyze (hex format, e.g., 0x1000)")] string address,
        [Description("The number of instructions to analyze (default: 20)")] int count = 20) {
        
        if (!TryParseAddress(address, out uint addr)) {
            return Task.FromResult($"Error: Invalid address format '{address}'");
        }

        if (count <= 0 || count > 100) {
            return Task.FromResult($"Error: Count must be between 1 and 100");
        }

        var result = new StringBuilder();
        result.AppendLine($"Control Flow Analysis at 0x{addr:X}:");
        result.AppendLine();

        var codeReader = new MemoryCodeReader(_memory, addr);
        var decoder = Iced.Intel.Decoder.Create(16, codeReader);
        decoder.IP = addr;
        var formatter = new IntelFormatter();

        int instructionCount = 0;
        var controlFlowInstructions = new List<(ulong address, string instruction, string type, ulong? target)>();
        
        while (instructionCount < count && decoder.IP < (ulong)_memory.Length) {
            decoder.Decode(out var instruction);
            
            // Check if this is a control flow instruction
            if (instruction.FlowControl != FlowControl.Next) {
                var output = new StringOutput();
                formatter.Format(instruction, output);
                string instrText = output.ToStringAndReset();
                
                string type = instruction.FlowControl.ToString();
                ulong? target = null;
                
                switch (instruction.FlowControl) {
                    case FlowControl.UnconditionalBranch:
                        type = "Jump (Unconditional)";
                        if (instruction.Op0Kind == OpKind.NearBranch16 || 
                            instruction.Op0Kind == OpKind.NearBranch32 || 
                            instruction.Op0Kind == OpKind.NearBranch64) {
                            target = instruction.NearBranchTarget;
                        }
                        break;
                    case FlowControl.ConditionalBranch:
                        type = "Conditional Branch";
                        if (instruction.Op0Kind == OpKind.NearBranch16 || 
                            instruction.Op0Kind == OpKind.NearBranch32 || 
                            instruction.Op0Kind == OpKind.NearBranch64) {
                            target = instruction.NearBranchTarget;
                        }
                        break;
                    case FlowControl.Call:
                        type = "Call (Near)";
                        if (instruction.Op0Kind == OpKind.NearBranch16 || 
                            instruction.Op0Kind == OpKind.NearBranch32 || 
                            instruction.Op0Kind == OpKind.NearBranch64) {
                            target = instruction.NearBranchTarget;
                        }
                        break;
                    case FlowControl.IndirectCall:
                        type = "Call (Indirect)";
                        break;
                    case FlowControl.IndirectBranch:
                        type = "Jump (Indirect)";
                        break;
                    case FlowControl.Return:
                        type = "Return";
                        break;
                }
                
                controlFlowInstructions.Add((instruction.IP, instrText, type, target));
            }
            
            instructionCount++;
            
            if (instruction.Code == Code.INVALID) {
                break;
            }
        }
        
        if (controlFlowInstructions.Count == 0) {
            result.AppendLine("No control flow instructions found in the analyzed range.");
        } else {
            result.AppendLine($"Found {controlFlowInstructions.Count} control flow instruction(s):");
            result.AppendLine();
            
            foreach (var (instructionAddress, instruction, type, target) in controlFlowInstructions) {
                result.AppendLine($"0x{instructionAddress:X8}: {instruction}");
                result.AppendLine($"  Type: {type}");
                if (target.HasValue) {
                    result.AppendLine($"  Target: 0x{target.Value:X}");
                }
                result.AppendLine();
            }
        }
        
        return Task.FromResult(result.ToString());
    }

    [McpServerTool]
    [Description("Get information about instruction operands and their types for AST preparation")]
    public Task<string> GetInstructionOperandInfo(
        [Description("The memory address of the instruction (hex format)")] string address) {
        
        if (!TryParseAddress(address, out uint addr)) {
            return Task.FromResult($"Error: Invalid address format '{address}'");
        }

        var result = new StringBuilder();
        
        var codeReader = new MemoryCodeReader(_memory, addr);
        var decoder = Iced.Intel.Decoder.Create(16, codeReader);
        decoder.IP = addr;
        decoder.Decode(out var instruction);
        
        if (instruction.Code == Code.INVALID) {
            return Task.FromResult("Error: Invalid instruction at this address");
        }
        
        result.AppendLine($"Instruction Operand Information for 0x{addr:X}:");
        result.AppendLine();
        
        var formatter = new IntelFormatter();
        var output = new StringOutput();
        formatter.Format(instruction, output);
        result.AppendLine($"Assembly: {output.ToStringAndReset()}");
        result.AppendLine($"Mnemonic: {instruction.Mnemonic}");
        result.AppendLine($"Op Code: {instruction.Code}");
        result.AppendLine();
        
        result.AppendLine("Operand Details (for AST construction):");
        for (int i = 0; i < instruction.OpCount; i++) {
            result.AppendLine($"Operand {i}:");
            
            var opKind = instruction.GetOpKind(i);
            
            result.AppendLine($"  Kind: {opKind}");
            
            switch (opKind) {
                case OpKind.Register:
                    var register = instruction.GetOpRegister(i);
                    result.AppendLine($"  Register: {register}");
                    result.AppendLine($"  Register Type: {register.GetFullRegister()}");
                    result.AppendLine($"  Register Size: {register.GetSize()} bytes");
                    break;
                    
                case OpKind.Memory:
                    result.AppendLine($"  Memory Segment: {instruction.MemorySegment}");
                    result.AppendLine($"  Memory Base Register: {instruction.MemoryBase}");
                    result.AppendLine($"  Memory Index Register: {instruction.MemoryIndex}");
                    result.AppendLine($"  Memory Index Scale: {instruction.MemoryIndexScale}");
                    result.AppendLine($"  Memory Displacement: 0x{instruction.MemoryDisplacement64:X}");
                    result.AppendLine($"  Memory Size: {instruction.MemorySize}");
                    break;
                    
                case OpKind.Immediate8:
                case OpKind.Immediate16:
                case OpKind.Immediate32:
                case OpKind.Immediate64:
                case OpKind.Immediate8to16:
                case OpKind.Immediate8to32:
                case OpKind.Immediate8to64:
                case OpKind.Immediate32to64:
                    var immediate = instruction.GetImmediate(i);
                    result.AppendLine($"  Immediate Value: 0x{immediate:X}");
                    result.AppendLine($"  Immediate Decimal: {immediate}");
                    break;
                    
                case OpKind.NearBranch16:
                case OpKind.NearBranch32:
                case OpKind.NearBranch64:
                    result.AppendLine($"  Near Branch Target: 0x{instruction.NearBranchTarget:X}");
                    break;
                    
                case OpKind.FarBranch16:
                case OpKind.FarBranch32:
                    result.AppendLine($"  Far Branch Selector: 0x{instruction.FarBranchSelector:X}");
                    result.AppendLine($"  Far Branch Target: 0x{instruction.FarBranch32:X}");
                    break;
            }
            result.AppendLine();
        }
        
        // Additional AST-relevant information
        result.AppendLine("Additional AST Information:");
        result.AppendLine($"  Condition Code (for branches): {instruction.ConditionCode}");
        result.AppendLine($"  Stack Pointer Increment: {instruction.StackPointerIncrement}");
        result.AppendLine($"  Is String Instruction: {instruction.IsStringInstruction}");
        result.AppendLine($"  Is Repeat Prefixed: {instruction.HasRepPrefix || instruction.HasRepePrefix || instruction.HasRepnePrefix}");
        result.AppendLine($"  Is Lock Prefixed: {instruction.HasLockPrefix}");
        result.AppendLine($"  Segment Override: {instruction.SegmentPrefix}");
        
        return Task.FromResult(result.ToString());
    }

    [McpServerTool]
    [Description("Extract instruction opcodes and operand patterns for code generation")]
    public Task<string> GetInstructionEncoding(
        [Description("The memory address of the instruction (hex format)")] string address) {
        
        if (!TryParseAddress(address, out uint addr)) {
            return Task.FromResult($"Error: Invalid address format '{address}'");
        }

        var result = new StringBuilder();
        
        var codeReader = new MemoryCodeReader(_memory, addr);
        var decoder = Iced.Intel.Decoder.Create(16, codeReader);
        decoder.IP = addr;
        decoder.Decode(out var instruction);
        
        if (instruction.Code == Code.INVALID) {
            return Task.FromResult("Error: Invalid instruction at this address");
        }
        
        result.AppendLine($"Instruction Encoding at 0x{addr:X}:");
        result.AppendLine();
        
        // Get instruction bytes
        var instrBytes = new byte[instruction.Length];
        for (int i = 0; i < instruction.Length; i++) {
            instrBytes[i] = _memory.SneakilyRead((uint)(instruction.IP + (ulong)i));
        }
        
        result.Append("Raw Bytes: ");
        foreach (byte b in instrBytes) {
            result.Append($"{b:X2} ");
        }
        result.AppendLine();
        result.AppendLine();
        
        result.AppendLine("Encoding Details:");
        result.AppendLine($"  Instruction Length: {instruction.Length} bytes");
        result.AppendLine($"  Opcode: {instruction.Code}");
        
        // Prefix information
        if (instruction.HasRepPrefix) {
            result.AppendLine($"  Prefix: REP");
        }
        if (instruction.HasRepePrefix) {
            result.AppendLine($"  Prefix: REPE");
        }
        if (instruction.HasRepnePrefix) {
            result.AppendLine($"  Prefix: REPNE");
        }
        if (instruction.HasLockPrefix) {
            result.AppendLine($"  Prefix: LOCK");
        }
        if (instruction.SegmentPrefix != Register.None) {
            result.AppendLine($"  Segment Prefix: {instruction.SegmentPrefix}");
        }
        
        // OpCode details
        result.AppendLine($"  Encoding: {instruction.Encoding}");
        result.AppendLine($"  CPUID Features: {string.Join(", ", instruction.CpuidFeatures)}");
        
        return Task.FromResult(result.ToString());
    }

    private static bool TryParseAddress(string addressStr, out uint address) {
        uint? parsed = AddressParser.ParseHex(addressStr);
        if (parsed != null) {
            address = parsed.Value;
            return true;
        }
        address = 0;
        return false;
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
    /// String output for formatted text
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
