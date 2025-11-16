namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Builder;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Instruction;
using Spice86.Core.Emulator.CPU.CfgCpu.InstructionExecutor;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents the Cbw32 class.
/// </summary>
public class Cbw32 : CfgInstruction {
    public Cbw32(SegmentedAddress address, InstructionField<ushort> opcodeField, List<InstructionPrefix> prefixes) :
        base(address, opcodeField, prefixes, 1) {
    }

    /// <summary>
    /// void method.
    /// </summary>
    public override void Execute(InstructionExecutionHelper helper) {
        // CBW, Convert word to dword
        int shortValue = (short)helper.State.AX;
        helper.State.EAX = (uint)shortValue;
        helper.MoveIpAndSetNextNode(this);
    }

    /// <summary>
    /// InstructionNode method.
    /// </summary>
    public override InstructionNode ToInstructionAst(AstBuilder builder) {
        return new InstructionNode(InstructionOperation.CBWE);
    }
}