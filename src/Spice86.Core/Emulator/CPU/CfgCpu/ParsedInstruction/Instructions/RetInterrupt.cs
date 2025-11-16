namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Builder;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Instruction;
using Spice86.Core.Emulator.CPU.CfgCpu.InstructionExecutor;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.Interfaces;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents the RetInterrupt class.
/// </summary>
public class RetInterrupt : CfgInstruction, IReturnInstruction {
    public RetInterrupt(SegmentedAddress address, InstructionField<ushort> opcodeField) : base(address, opcodeField, null) {
    }

    public CfgInstruction? CurrentCorrespondingCallInstruction { get; set; }

    /// <summary>
    /// The bool.
    /// </summary>
    public override bool CanCauseContextRestore => true;

    /// <summary>
    /// void method.
    /// </summary>
    public override void Execute(InstructionExecutionHelper helper) {
        helper.HandleInterruptRet(this);
    }

    /// <summary>
    /// InstructionNode method.
    /// </summary>
    public override InstructionNode ToInstructionAst(AstBuilder builder) {
        return new InstructionNode(InstructionOperation.IRET);
    }
}