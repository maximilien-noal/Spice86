namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Builder;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Instruction;
using Spice86.Core.Emulator.CPU.CfgCpu.InstructionExecutor;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents the Lahf class.
/// </summary>
public class Lahf : CfgInstruction {
    public Lahf(SegmentedAddress address, InstructionField<ushort> opcodeField) :
        base(address, opcodeField, 1) {
    }

    /// <summary>
    /// void method.
    /// </summary>
    public override void Execute(InstructionExecutionHelper helper) {
        helper.State.AH = (byte)helper.State.Flags.FlagRegister;
        helper.MoveIpAndSetNextNode(this);
    }

    /// <summary>
    /// InstructionNode method.
    /// </summary>
    public override InstructionNode ToInstructionAst(AstBuilder builder) {
        return new InstructionNode(InstructionOperation.LAHF);
    }
}