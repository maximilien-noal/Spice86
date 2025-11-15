namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.SpecificParsers;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Shared.Emulator.Memory;

using System.Xml;

/// <summary>
/// Parser for instructions that only have the opcode.
/// The opcode has a reg index to indicate on which register to perform the operation.
/// Operation is performed on 16 or 32 bits operands depending on the operand size prefix.
/// </summary>
public abstract class OperationRegIndexParser : BaseInstructionParser {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="other">The other.</param>
    public OperationRegIndexParser(BaseInstructionParser other) : base(other) {
    }

    /// <summary>
    /// Parses .
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>The result of the operation.</returns>
    public CfgInstruction Parse(ParsingContext context) {
        int regIndex = ComputeRegIndex(context.OpcodeField);
        BitWidth bitWidth = GetBitWidth(false, context.HasOperandSize32);
        return Parse(context, regIndex, bitWidth);
    }
    protected abstract CfgInstruction Parse(ParsingContext context, int regIndex, BitWidth bitWidth);
}

/// <summary>
/// Represents inc reg index parser.
/// </summary>
[OperationRegIndexParser("IncReg", false)]
public partial class IncRegIndexParser;
/// <summary>
/// Represents dec reg index parser.
/// </summary>
[OperationRegIndexParser("DecReg", false)]
public partial class DecRegIndexParser;
/// <summary>
/// Represents push reg index parser.
/// </summary>
[OperationRegIndexParser("PushReg", false)]
public partial class PushRegIndexParser;
/// <summary>
/// Represents pop reg index parser.
/// </summary>
[OperationRegIndexParser("PopReg", false)]
public partial class PopRegIndexParser;

/// <summary>
/// Represents xchg reg acc parser.
/// </summary>
[OperationRegIndexParser("XchgRegAcc", false)]
public partial class XchgRegAccParser;