namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Builder;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Instruction;
using Spice86.Core.Emulator.CPU.CfgCpu.InstructionExecutor;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.Interfaces;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents ret interrupt.
/// </summary>
public class RetInterrupt : CfgInstruction, IReturnInstruction {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="opcodeField">The opcode field.</param>
    public RetInterrupt(SegmentedAddress address, InstructionField<ushort> opcodeField) : base(address, opcodeField, null) {
    }

    /// <summary>
    /// Gets or sets current corresponding call instruction.
    /// </summary>
    public CfgInstruction? CurrentCorrespondingCallInstruction { get; set; }

    /// <summary>
    /// The can cause context restore.
    /// </summary>
    public override bool CanCauseContextRestore => true;

    /// <summary>
    /// Executes .
    /// </summary>
    /// <param name="helper">The helper.</param>
    public override void Execute(InstructionExecutionHelper helper) {
        helper.HandleInterruptRet(this);
    }

    /// <summary>
    /// Converts to instruction ast.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The result of the operation.</returns>
    public override InstructionNode ToInstructionAst(AstBuilder builder) {
        return new InstructionNode(InstructionOperation.IRET);
    }
}