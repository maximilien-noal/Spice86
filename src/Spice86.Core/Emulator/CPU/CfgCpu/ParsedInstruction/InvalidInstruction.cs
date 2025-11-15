namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Builder;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Instruction;
using Spice86.Core.Emulator.CPU.CfgCpu.InstructionExecutor;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Core.Emulator.CPU.Exceptions;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents invalid instruction.
/// </summary>
public class InvalidInstruction : CfgInstruction {
    private readonly CpuException _cpuException;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="opcodeField">The opcode field.</param>
    /// <param name="prefixes">The prefixes.</param>
    /// <param name="cpuException">The cpu exception.</param>
    public InvalidInstruction(SegmentedAddress address, InstructionField<ushort> opcodeField,
        List<InstructionPrefix> prefixes, CpuException cpuException) : base(address, opcodeField, prefixes, null) {
        _cpuException = cpuException;
    }

    /// <summary>
    /// Executes .
    /// </summary>
    /// <param name="helper">The helper.</param>
    public override void Execute(InstructionExecutionHelper helper) {
        helper.HandleCpuException(this, _cpuException);
    }

    /// <summary>
    /// Converts to instruction ast.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The result of the operation.</returns>
    public override InstructionNode ToInstructionAst(AstBuilder builder) {
        return new InstructionNode(InstructionOperation.INVALID);
    }
}