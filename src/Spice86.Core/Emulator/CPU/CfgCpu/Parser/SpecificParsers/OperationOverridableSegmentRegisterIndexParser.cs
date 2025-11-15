namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.SpecificParsers;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Parser for instructions that only have the opcode.
/// The opcode has a reg index to indicate on which register to perform the operation.
/// Operation is performed on 16 or 32 bits operands depending on the operand size prefix.
/// </summary>
public abstract class OperationOverridableSegmentRegisterIndexParser : BaseInstructionParser {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="other">The other.</param>
    public OperationOverridableSegmentRegisterIndexParser(BaseInstructionParser other) : base(other) {
    }

    /// <summary>
    /// Parses .
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>The result of the operation.</returns>
    public CfgInstruction Parse(ParsingContext context) {
        int segmentRegisterIndex = GetSegmentRegisterOverrideOrDs(context);
        BitWidth bitWidth = GetBitWidth(context.OpcodeField, context.HasOperandSize32);
        return Parse(context, segmentRegisterIndex, bitWidth);
    }
    protected abstract CfgInstruction Parse(ParsingContext context, int segmentRegisterIndex, BitWidth bitWidth);
}

/// <summary>
/// Represents movs parser.
/// </summary>
[OperationOverridableSegmentRegisterIndexParser("Movs")]
public partial class MovsParser;
/// <summary>
/// Represents cmps parser.
/// </summary>
[OperationOverridableSegmentRegisterIndexParser("Cmps")]
public partial class CmpsParser;
/// <summary>
/// Represents lods parser.
/// </summary>
[OperationOverridableSegmentRegisterIndexParser("Lods")]
public partial class LodsParser;
/// <summary>
/// Represents outs dx parser.
/// </summary>
[OperationOverridableSegmentRegisterIndexParser("OutsDx")]
public partial class OutsDxParser;