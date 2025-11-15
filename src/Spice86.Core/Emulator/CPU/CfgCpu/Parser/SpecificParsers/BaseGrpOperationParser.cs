namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.SpecificParsers;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.ModRm;

/// <summary>
/// Represents base grp operation parser.
/// </summary>
public abstract class BaseGrpOperationParser : BaseInstructionParser {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="instructionParser">The instruction parser.</param>
    public BaseGrpOperationParser(BaseInstructionParser instructionParser) : base(instructionParser) {
    }

    /// <summary>
    /// Parses .
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>The result of the operation.</returns>
    public CfgInstruction Parse(ParsingContext context) {
        ModRmContext modRmContext = _modRmParser.ParseNext(context);
        int groupIndex = modRmContext.RegisterIndex;
        if (groupIndex > 7) {
            throw new InvalidGroupIndexException(_state, groupIndex);
        }

        return Parse(context, modRmContext, groupIndex);
    }

    protected abstract CfgInstruction Parse(ParsingContext context, ModRmContext modRmContext, int groupIndex);
}