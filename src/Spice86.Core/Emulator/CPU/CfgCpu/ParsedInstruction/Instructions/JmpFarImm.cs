namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Builder;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Instruction;
using Spice86.Core.Emulator.CPU.CfgCpu.InstructionExecutor;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.CommonGrammar;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.Interfaces;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents jmp far imm.
/// </summary>
public class JmpFarImm : InstructionWithSegmentedAddressField, IJumpInstruction {
    private readonly SegmentedAddress _targetAddress;

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="opcodeField">The opcode field.</param>
    /// <param name="prefixes">The prefixes.</param>
    /// <param name="segmentedAddressField">The segmented address field.</param>
    public JmpFarImm(
        SegmentedAddress address,
        InstructionField<ushort> opcodeField,
        List<InstructionPrefix> prefixes,
        InstructionField<SegmentedAddress> segmentedAddressField) :
        base(address, opcodeField, prefixes, segmentedAddressField, 1) {
        _targetAddress = SegmentedAddressField.Value;
    }

    /// <summary>
    /// Executes .
    /// </summary>
    /// <param name="helper">The helper.</param>
    public override void Execute(InstructionExecutionHelper helper) {
        helper.JumpFar(this, _targetAddress.Segment, _targetAddress.Offset);
    }

    /// <summary>
    /// Converts to instruction ast.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The result of the operation.</returns>
    public override InstructionNode ToInstructionAst(AstBuilder builder) {
        return new InstructionNode(InstructionOperation.JMP_FAR, builder.Constant.ToNode(_targetAddress));
    }
}