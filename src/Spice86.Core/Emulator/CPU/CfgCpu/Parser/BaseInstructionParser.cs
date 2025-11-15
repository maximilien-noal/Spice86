namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.CPU.CfgCpu.Parser.FieldReader;
using Spice86.Core.Emulator.CPU.Exceptions;
using Spice86.Core.Emulator.CPU.Registers;
using Spice86.Core.Emulator.Errors;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents base instruction parser.
/// </summary>
public class BaseInstructionParser {
    /// <summary>
    /// The reg index mask.
    /// </summary>
    protected const int RegIndexMask = 0b111;
    /// <summary>
    /// The word mask.
    /// </summary>
    protected const int WordMask = 0b1000;
    /// <summary>
    /// For some instructions, lsb == 0 when 8bit and 1 when 16/32
    /// </summary>
    protected const byte SizeMask = 0b1;
    /// <summary>
    /// The _instruction reader.
    /// </summary>
    protected readonly InstructionReader _instructionReader;
    /// <summary>
    /// The _instruction prefix parser.
    /// </summary>
    protected readonly InstructionPrefixParser _instructionPrefixParser;
    /// <summary>
    /// The _mod rm parser.
    /// </summary>
    protected readonly ModRmParser _modRmParser;
    /// <summary>
    /// The _state.
    /// </summary>
    protected readonly State _state;

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="instructionReader">The instruction reader.</param>
    /// <param name="state">The state.</param>
    protected BaseInstructionParser(InstructionReader instructionReader, State state) {
        _instructionReader = instructionReader;
        _instructionPrefixParser = new(_instructionReader);
        _modRmParser = new(_instructionReader, state);
        _state = state;
    }

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="other">The other.</param>
    protected BaseInstructionParser(BaseInstructionParser other) {
        _instructionReader = other._instructionReader;
        _instructionPrefixParser = other._instructionPrefixParser;
        _modRmParser = other._modRmParser;
        _state = other._state;
    }

    /// <summary>
    /// Performs the segment from prefixes or ds operation.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>The result of the operation.</returns>
    protected static int SegmentFromPrefixesOrDs(ParsingContext context) {
        return context.SegmentOverrideFromPrefixes ?? (int)SegmentRegisterIndex.DsIndex;
    }

    /// <summary>
    /// Calculates compute reg index.
    /// </summary>
    /// <param name="opcodeField">The opcode field.</param>
    /// <returns>The result of the operation.</returns>
    protected int ComputeRegIndex(InstructionField<ushort> opcodeField) {
        return opcodeField.Value & RegIndexMask;
    }

    /// <summary>
    /// Determines whether it has operand size 8.
    /// </summary>
    /// <param name="opcode">The opcode.</param>
    /// <returns><c>true</c> if the condition is met; otherwise, <c>false</c>.</returns>
    protected bool HasOperandSize8(ushort opcode) {
        return (opcode & SizeMask) == 0;
    }
    /// <summary>
    /// Gets bit width.
    /// </summary>
    /// <param name="opcodeField">The opcode field.</param>
    /// <param name="is32">The is 32.</param>
    /// <returns>The result of the operation.</returns>
    protected BitWidth GetBitWidth(InstructionField<ushort> opcodeField, bool is32) {
        return HasOperandSize8(opcodeField.Value) ? BitWidth.BYTE_8 : is32 ? BitWidth.DWORD_32 : BitWidth.WORD_16;
    }

    /// <summary>
    /// Gets bit width.
    /// </summary>
    /// <param name="is8">The is 8.</param>
    /// <param name="is32">The is 32.</param>
    /// <returns>The result of the operation.</returns>
    protected BitWidth GetBitWidth(bool is8, bool is32) {
        return is8 ? BitWidth.BYTE_8 : is32 ? BitWidth.DWORD_32 : BitWidth.WORD_16;
    }

    /// <summary>
    /// Gets segment register override or ds.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>The result of the operation.</returns>
    protected int GetSegmentRegisterOverrideOrDs(ParsingContext context) {
        return context.SegmentOverrideFromPrefixes ?? (int)SegmentRegisterIndex.DsIndex;
    }

    /// <summary>
    /// Performs the bit is true operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="bitIndex">The bit index.</param>
    /// <returns>A boolean value indicating the result.</returns>
    protected bool BitIsTrue(uint value, int bitIndex) {
        return ((value >> bitIndex) & 1) == 1;
    }

    /// <summary>
    /// Creates unsupported bit width exception.
    /// </summary>
    /// <param name="bitWidth">The bit width.</param>
    /// <returns>The result of the operation.</returns>
    protected static UnsupportedBitWidthException CreateUnsupportedBitWidthException(BitWidth bitWidth) {
        return new UnsupportedBitWidthException(bitWidth);
    }
}