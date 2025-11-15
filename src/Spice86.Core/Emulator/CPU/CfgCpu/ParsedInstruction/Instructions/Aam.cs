namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Builder;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Instruction;
using Spice86.Core.Emulator.CPU.CfgCpu.InstructionExecutor;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.CommonGrammar;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Core.Emulator.CPU.Exceptions;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents aam.
/// </summary>
public class Aam : InstructionWithValueField<byte> {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="opcodeField">The opcode field.</param>
    /// <param name="valueField">The value field.</param>
    public Aam(SegmentedAddress address, InstructionField<ushort> opcodeField, InstructionField<byte> valueField) :
        base(address, opcodeField, new List<InstructionPrefix>(), valueField, 1) {
    }

    /// <summary>
    /// Executes .
    /// </summary>
    /// <param name="helper">The helper.</param>
    public override void Execute(InstructionExecutionHelper helper) {
        byte v2 = helper.InstructionFieldValueRetriever.GetFieldValue(ValueField);
        byte v1 = helper.State.AL;
        if (v2 == 0) {
            throw new CpuDivisionErrorException("Division by zero");
        }

        byte result = (byte)(v1 % v2);
        helper.State.AH = (byte)(v1 / v2);
        helper.State.AL = result;
        helper.Alu8.UpdateFlags(result);
        helper.MoveIpAndSetNextNode(this);
    }

    /// <summary>
    /// Converts to instruction ast.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The result of the operation.</returns>
    public override InstructionNode ToInstructionAst(AstBuilder builder) {
        return new InstructionNode(InstructionOperation.AAM);
    }
}