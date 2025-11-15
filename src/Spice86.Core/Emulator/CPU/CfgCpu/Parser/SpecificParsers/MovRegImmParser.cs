namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.SpecificParsers;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents mov reg imm parser.
/// </summary>
public class MovRegImmParser : BaseInstructionParser {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="other">The other.</param>
    public MovRegImmParser(BaseInstructionParser other) : base(other) {
    }

    /// <summary>
    /// Parses mov reg imm.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>The result of the operation.</returns>
    public CfgInstruction ParseMovRegImm(ParsingContext context) {
        int regIndex = ComputeRegIndex(context.OpcodeField);
        bool is8 = (context.OpcodeField.Value & WordMask) == 0;
        BitWidth bitWidth = GetBitWidth(is8, context.HasOperandSize32);
        return bitWidth switch {
            BitWidth.BYTE_8 => new MovRegImm8(context.Address, context.OpcodeField, context.Prefixes, _instructionReader.UInt8.NextField(false), regIndex),
            BitWidth.WORD_16 => new MovRegImm16(context.Address, context.OpcodeField, context.Prefixes, _instructionReader.UInt16.NextField(false), regIndex),
            BitWidth.DWORD_32 => new MovRegImm32(context.Address, context.OpcodeField, context.Prefixes,
                _instructionReader.UInt32.NextField(false), regIndex),
            _ => throw CreateUnsupportedBitWidthException(bitWidth)
        };
    }
}