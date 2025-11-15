namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.SpecificParsers;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.ModRm;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents base operation mod rm factory.
/// </summary>
public abstract class BaseOperationModRmFactory : BaseInstructionParser, IInstructionWithModRmFactory {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="other">The other.</param>
    protected BaseOperationModRmFactory(BaseInstructionParser other) : base(other) {
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
            BitWidth.BYTE_8 => BuildOperandSize8(context, modRmContext),
            BitWidth.WORD_16 => BuildOperandSize16(context, modRmContext),
            BitWidth.DWORD_32 => BuildOperandSize32(context, modRmContext),
            _ => throw CreateUnsupportedBitWidthException(bitWidth)
        };
    }

    protected abstract CfgInstruction BuildOperandSize8(ParsingContext context, ModRmContext modRmContext);
    protected abstract CfgInstruction BuildOperandSize16(ParsingContext context, ModRmContext modRmContext);
    protected abstract CfgInstruction BuildOperandSize32(ParsingContext context, ModRmContext modRmContext);
}