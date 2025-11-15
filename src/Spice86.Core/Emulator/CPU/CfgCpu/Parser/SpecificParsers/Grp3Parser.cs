namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.SpecificParsers;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.ModRm;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents grp 3 parser.
/// </summary>
public class Grp3Parser : BaseGrpOperationParser {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="instructionParser">The instruction parser.</param>
    public Grp3Parser(BaseInstructionParser instructionParser) : base(instructionParser) {
    }

    /// <summary>
    /// Parses .
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="modRmContext">The mod rm context.</param>
    /// <param name="groupIndex">The group index.</param>
    /// <returns>The result of the operation.</returns>
    protected override CfgInstruction Parse(ParsingContext context, ModRmContext modRmContext, int groupIndex) {
        BitWidth bitWidth = GetBitWidth(context.OpcodeField, context.HasOperandSize32);
        IInstructionWithModRmFactory operationFactory = GetOperationParser(groupIndex);
        return operationFactory.Parse(context, modRmContext, bitWidth);
    }

    private IInstructionWithModRmFactory GetOperationParser(int groupIndex) {
        return groupIndex switch {
            0 => new Grp3TestInstructionWithModRmFactory(this),
            2 => new Grp3NotRmOperationFactory(this),
            3 => new Grp3NegRmOperationFactory(this),
            4 => new Grp3MulRmAccOperationFactory(this),
            5 => new Grp3ImulRmAccOperationFactory(this),
            6 => new Grp3DivRmAccOperationFactory(this),
            7 => new Grp3IdivRmAccOperationFactory(this),
            _ => throw new InvalidGroupIndexException(_state, groupIndex)
        };
    }
}

/// <summary>
/// Represents grp 3 test instruction with mod rm factory.
/// </summary>
public class Grp3TestInstructionWithModRmFactory : BaseInstructionParser, IInstructionWithModRmFactory {
    /// <summary>
    /// Performs the grp 3 test instruction with mod rm factory operation.
    /// </summary>
    /// <param name="other">The other.</param>
    public Grp3TestInstructionWithModRmFactory(BaseInstructionParser other) : base(other) {
    }

    /// <summary>
    /// Parses .
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="modRmContext">The mod rm context.</param>
    /// <param name="bitWidth">The bit width.</param>
    /// <returns>The result of the operation.</returns>
    public CfgInstruction Parse(ParsingContext context, ModRmContext modRmContext, BitWidth bitWidth) {
        return bitWidth switch {
            BitWidth.BYTE_8 => new Grp3TestRmImm8(context.Address, context.OpcodeField, context.Prefixes, modRmContext,
                _instructionReader.UInt8.NextField(false)),
            BitWidth.WORD_16 => new Grp3TestRmImm16(context.Address, context.OpcodeField, context.Prefixes, modRmContext,
                _instructionReader.UInt16.NextField(false)),
            BitWidth.DWORD_32 => new Grp3TestRmImm32(context.Address, context.OpcodeField, context.Prefixes, modRmContext,
                _instructionReader.UInt32.NextField(false)),
            _ => throw CreateUnsupportedBitWidthException(bitWidth)
        };
    }
}

/// <summary>
/// Represents grp 3 not rm operation factory.
/// </summary>
[OperationModRmFactory("Grp3NotRm")]
public partial class Grp3NotRmOperationFactory;

/// <summary>
/// Represents grp 3 neg rm operation factory.
/// </summary>
[OperationModRmFactory("Grp3NegRm")]
public partial class Grp3NegRmOperationFactory;

/// <summary>
/// Represents grp 3 mul rm acc operation factory.
/// </summary>
[OperationModRmFactory("Grp3MulRmAcc")]
public partial class Grp3MulRmAccOperationFactory;

/// <summary>
/// Represents grp 3 imul rm acc operation factory.
/// </summary>
[OperationModRmFactory("Grp3ImulRmAcc")]
public partial class Grp3ImulRmAccOperationFactory;

/// <summary>
/// Represents grp 3 div rm acc operation factory.
/// </summary>
[OperationModRmFactory("Grp3DivRmAcc")]
public partial class Grp3DivRmAccOperationFactory;

/// <summary>
/// Represents grp 3 idiv rm acc operation factory.
/// </summary>
[OperationModRmFactory("Grp3IdivRmAcc")]
public partial class Grp3IdivRmAccOperationFactory;