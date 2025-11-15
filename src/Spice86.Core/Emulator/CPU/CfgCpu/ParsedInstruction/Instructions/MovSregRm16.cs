namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Builder;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Instruction;
using Spice86.Core.Emulator.CPU.CfgCpu.InstructionExecutor;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.ModRm;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Core.Emulator.CPU.Exceptions;
using Spice86.Core.Emulator.CPU.Registers;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents mov sreg rm 16.
/// </summary>
public class MovSregRm16 : InstructionWithModRm {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="opcodeField">The opcode field.</param>
    /// <param name="prefixes">The prefixes.</param>
    /// <param name="modRmContext">The mod rm context.</param>
    public MovSregRm16(SegmentedAddress address,
        InstructionField<ushort> opcodeField,
        List<InstructionPrefix> prefixes,
        ModRmContext modRmContext) : base(address, opcodeField, prefixes, modRmContext, 1) {
        if (modRmContext.RegisterIndex == (uint)SegmentRegisterIndex.CsIndex) {
            throw new CpuInvalidOpcodeException("Attempted to write to CS register with MOV instruction");
        }
    }

    /// <summary>
    /// Executes .
    /// </summary>
    /// <param name="helper">The helper.</param>
    public override void Execute(InstructionExecutionHelper helper) {
        helper.ModRm.RefreshWithNewModRmContext(ModRmContext);
        helper.State.SegmentRegisters.UInt16[ModRmContext.RegisterIndex] = helper.ModRm.RM16;
        if (ModRmContext.RegisterIndex == (uint)SegmentRegisterIndex.SsIndex) {
            helper.State.InterruptShadowing = true;
        }
        helper.MoveIpAndSetNextNode(this);
    }

    /// <summary>
    /// Converts to instruction ast.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The result of the operation.</returns>
    public override InstructionNode ToInstructionAst(AstBuilder builder) {
        return new InstructionNode(InstructionOperation.MOV, builder.Register.SReg(ModRmContext.RegisterIndex), builder.ModRm.RmToNode(DataType.UINT16, ModRmContext));
    }
}