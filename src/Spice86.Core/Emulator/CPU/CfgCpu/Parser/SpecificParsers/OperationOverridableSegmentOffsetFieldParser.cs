namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.SpecificParsers;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents operation overridable segment offset field parser.
/// </summary>
public abstract class OperationOverridableSegmentOffsetFieldParser : BaseInstructionParser {
    public OperationOverridableSegmentOffsetFieldParser(BaseInstructionParser other) : base(other) {
    }
    /// <summary>
    /// Parses .
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>The result of the operation.</returns>
    public CfgInstruction Parse(ParsingContext context) {
        BitWidth bitWidth = GetBitWidth(context.OpcodeField, context.HasOperandSize32);
        return Parse(context, bitWidth, SegmentFromPrefixesOrDs(context), _instructionReader.UInt16.NextField(false));
    }
    protected abstract CfgInstruction Parse(ParsingContext context, BitWidth bitWidth, int segmentRegisterIndex, InstructionField<ushort> offsetField);

}
/// <summary>
/// Represents mov moffs acc parser.
/// </summary>
[OperationOverridableSegmentOffsetFieldParser("MovMoffsAcc")]
public partial class MovMoffsAccParser;

/// <summary>
/// Represents mov acc moffs parser.
/// </summary>
[OperationOverridableSegmentOffsetFieldParser("MovAccMoffs")]
public partial class MovAccMoffsParser;