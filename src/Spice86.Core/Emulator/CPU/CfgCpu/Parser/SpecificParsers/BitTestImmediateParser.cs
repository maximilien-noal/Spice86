namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.SpecificParsers;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.ModRm;

/// <summary>
/// Represents bit test immediate parser.
/// </summary>
public class BitTestImmediateParser(BaseInstructionParser other) : BaseGrpOperationParser(other) {
    /// <summary>
    /// Parses .
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="modRmContext">The mod rm context.</param>
    /// <param name="groupIndex">The group index.</param>
    /// <returns>The result of the operation.</returns>
    protected override CfgInstruction Parse(ParsingContext context, ModRmContext modRmContext, int groupIndex) {
        InstructionField<byte> immediate = _instructionReader.UInt8.NextField(false);
        return groupIndex switch {
            4 => context.HasOperandSize32
                ? new BtRmImm32(context.Address, context.OpcodeField, context.Prefixes, modRmContext, immediate)
                : new BtRmImm16(context.Address, context.OpcodeField, context.Prefixes, modRmContext, immediate),
            5 => context.HasOperandSize32
                ? new BtsRmImm32(context.Address, context.OpcodeField, context.Prefixes, modRmContext, immediate)
                : new BtsRmImm16(context.Address, context.OpcodeField, context.Prefixes, modRmContext, immediate),
            6 => context.HasOperandSize32
                ? new BtrRmImm32(context.Address, context.OpcodeField, context.Prefixes, modRmContext, immediate)
                : new BtrRmImm16(context.Address, context.OpcodeField, context.Prefixes, modRmContext, immediate),
            7 => context.HasOperandSize32
                ? new BtcRmImm32(context.Address, context.OpcodeField, context.Prefixes, modRmContext, immediate)
                : new BtcRmImm16(context.Address, context.OpcodeField, context.Prefixes, modRmContext, immediate),
            _ => throw new InvalidGroupIndexException(_state, groupIndex)
        };
    }
}