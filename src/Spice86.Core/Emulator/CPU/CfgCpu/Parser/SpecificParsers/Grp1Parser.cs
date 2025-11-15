namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.SpecificParsers;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.ModRm;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents grp 1 parser.
/// </summary>
public class Grp1Parser : BaseGrpOperationParser {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="instructionParser">The instruction parser.</param>
    public Grp1Parser(BaseInstructionParser instructionParser) : base(instructionParser) {
    }

    /// <summary>
    /// Parses .
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="modRmContext">The mod rm context.</param>
    /// <param name="groupIndex">The group index.</param>
    /// <returns>The result of the operation.</returns>
    protected override CfgInstruction Parse(ParsingContext context, ModRmContext modRmContext, int groupIndex) {
        ushort opCode = context.OpcodeField.Value;
        bool signExtendOp2 = opCode is 0x83;

        BitWidth bitWidth = GetBitWidth(context.OpcodeField, context.HasOperandSize32);
        Grp1OperationParser operationParser = GetOperationParser(groupIndex);
        return operationParser.Parse(context, modRmContext, bitWidth, signExtendOp2);
    }

    private Grp1OperationParser GetOperationParser(int groupIndex) {
        return groupIndex switch {
            0 => new Grp1AddOperationParser(this),
            1 => new Grp1OrOperationParser(this),
            2 => new Grp1AdcOperationParser(this),
            3 => new Grp1SbbOperationParser(this),
            4 => new Grp1AndOperationParser(this),
            5 => new Grp1SubOperationParser(this),
            6 => new Grp1XorOperationParser(this),
            7 => new Grp1CmpOperationParser(this),
            _ => throw new InvalidGroupIndexException(_state, groupIndex)
        };
    }
}

/// <summary>
/// Internal base class for generated code that will instantiate one of the grp1 operation in 8,16,32 bits mode, signed / unsigned
/// </summary>
public abstract class Grp1OperationParser : BaseInstructionParser {
    /// <summary>
    /// Performs the grp 1 operation parser operation.
    /// </summary>
    /// <param name="other">The other.</param>
    protected Grp1OperationParser(BaseInstructionParser other) : base(other) {
    }

    /// <summary>
    /// Parses .
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="modRmContext">The mod rm context.</param>
    /// <param name="bitWidth">The bit width.</param>
    /// <param name="signExtendOp2">The sign extend op 2.</param>
    /// <returns>The result of the operation.</returns>
    public CfgInstruction Parse(ParsingContext context, ModRmContext modRmContext, BitWidth bitWidth, bool signExtendOp2) {
        return bitWidth switch {
            BitWidth.BYTE_8 => BuildOperandSize8(context, modRmContext,
                _instructionReader.UInt8.NextField(false)),
            BitWidth.WORD_16 => signExtendOp2
                ? BuildOperandSizeSigned16(context, modRmContext,
                    _instructionReader.Int8.NextField(false))
                : BuildOperandSizeUnsigned16(context, modRmContext,
                    _instructionReader.UInt16.NextField(false)),
            BitWidth.DWORD_32 => signExtendOp2
                ? BuildOperandSizeSigned32(context, modRmContext,
                    _instructionReader.Int8.NextField(false))
                : BuildOperandSizeUnsigned32(context, modRmContext,
                    _instructionReader.UInt32.NextField(false)),
            _ => throw CreateUnsupportedBitWidthException(bitWidth)
        };
    }


    protected abstract CfgInstruction BuildOperandSize8(ParsingContext context, ModRmContext modRmContext, InstructionField<byte> valueField);

    protected abstract CfgInstruction BuildOperandSizeSigned16(ParsingContext context, ModRmContext modRmContext,
        InstructionField<sbyte> valueField);

    protected abstract CfgInstruction BuildOperandSizeUnsigned16(ParsingContext context, ModRmContext modRmContext,
        InstructionField<ushort> valueField);

    protected abstract CfgInstruction BuildOperandSizeSigned32(ParsingContext context, ModRmContext modRmContext,
        InstructionField<sbyte> valueField);

    protected abstract CfgInstruction BuildOperandSizeUnsigned32(ParsingContext context, ModRmContext modRmContext,
        InstructionField<uint> valueField);
}

/// <summary>
/// Represents grp 1 add operation parser.
/// </summary>
[Grp1OperationParser("Add")]
public partial class Grp1AddOperationParser;

/// <summary>
/// Represents grp 1 or operation parser.
/// </summary>
[Grp1OperationParser("Or")]
public partial class Grp1OrOperationParser;

/// <summary>
/// Represents grp 1 adc operation parser.
/// </summary>
[Grp1OperationParser("Adc")]
public partial class Grp1AdcOperationParser;

/// <summary>
/// Represents grp 1 sbb operation parser.
/// </summary>
[Grp1OperationParser("Sbb")]
public partial class Grp1SbbOperationParser;

/// <summary>
/// Represents grp 1 and operation parser.
/// </summary>
[Grp1OperationParser("And")]
public partial class Grp1AndOperationParser;

/// <summary>
/// Represents grp 1 sub operation parser.
/// </summary>
[Grp1OperationParser("Sub")]
public partial class Grp1SubOperationParser;

/// <summary>
/// Represents grp 1 xor operation parser.
/// </summary>
[Grp1OperationParser("Xor")]
public partial class Grp1XorOperationParser;

/// <summary>
/// Represents grp 1 cmp operation parser.
/// </summary>
[Grp1OperationParser("Cmp")]
public partial class Grp1CmpOperationParser;