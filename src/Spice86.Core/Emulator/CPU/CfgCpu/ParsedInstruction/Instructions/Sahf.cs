namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Builder;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Instruction;
using Spice86.Core.Emulator.CPU.CfgCpu.InstructionExecutor;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents sahf.
/// </summary>
public class Sahf : CfgInstruction {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="opcodeField">The opcode field.</param>
    public Sahf(SegmentedAddress address, InstructionField<ushort> opcodeField) : base(address, opcodeField, 1) {
    }

    /// <summary>
    /// Executes .
    /// </summary>
    /// <param name="helper">The helper.</param>
    public override void Execute(InstructionExecutionHelper helper) {
        // EFLAGS(SF:ZF:0:AF:0:PF:1:CF) := AH;
        helper.State.SignFlag = (helper.State.AH & Flags.Sign) == Flags.Sign;
        helper.State.ZeroFlag = (helper.State.AH & Flags.Zero) == Flags.Zero;
        helper.State.AuxiliaryFlag = (helper.State.AH & Flags.Auxiliary) == Flags.Auxiliary;
        helper.State.ParityFlag = (helper.State.AH & Flags.Parity) == Flags.Parity;
        helper.State.CarryFlag = (helper.State.AH & Flags.Carry) == Flags.Carry;
        helper.MoveIpAndSetNextNode(this);
    }

    /// <summary>
    /// Converts to instruction ast.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The result of the operation.</returns>
    public override InstructionNode ToInstructionAst(AstBuilder builder) {
        return new InstructionNode(InstructionOperation.SAHF);
    }
}