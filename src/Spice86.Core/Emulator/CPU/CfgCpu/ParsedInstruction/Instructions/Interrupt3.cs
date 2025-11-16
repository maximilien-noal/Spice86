namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Builder;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Instruction;
using Spice86.Core.Emulator.CPU.CfgCpu.InstructionExecutor;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.Interfaces;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents the Interrupt3 class.
/// </summary>
public class Interrupt3 : CfgInstruction, ICallInstruction {
    public Interrupt3(SegmentedAddress address, InstructionField<ushort> opcodeField) : base(address, opcodeField, null) {
    }

    /// <summary>
    /// void method.
    /// </summary>
    public override void Execute(InstructionExecutionHelper helper) {
        helper.HandleInterruptInstruction(this, 3);
    }

    /// <summary>
    /// InstructionNode method.
    /// </summary>
    public override InstructionNode ToInstructionAst(AstBuilder builder) {
        return new InstructionNode(InstructionOperation.INT, builder.Constant.ToNode((byte)3));
    }
}