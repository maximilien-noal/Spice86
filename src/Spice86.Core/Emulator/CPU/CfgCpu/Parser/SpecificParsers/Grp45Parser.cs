namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.SpecificParsers;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.ModRm;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents grp 45 parser.
/// </summary>
public class Grp45Parser : BaseGrpOperationParser {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="other">The other.</param>
    public Grp45Parser(BaseInstructionParser other) : base(other) {
    }

    /// <summary>
    /// Parses .
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="modRmContext">The mod rm context.</param>
    /// <param name="groupIndex">The group index.</param>
    /// <returns>The result of the operation.</returns>
    protected override CfgInstruction Parse(ParsingContext context, ModRmContext modRmContext, int groupIndex) {
        bool grp4 = context.OpcodeField.Value is 0xFE;
        if (grp4) {
            return groupIndex switch {
                0 => new Grp45RmInc8(context.Address, context.OpcodeField, context.Prefixes, modRmContext),
                1 => new Grp45RmDec8(context.Address, context.OpcodeField, context.Prefixes, modRmContext),
                // Callback, emulator specific instruction FE38 like in dosbox,
                // to allow interrupts to be overridden by the program
                7 => new Grp4Callback(context.Address, context.OpcodeField, context.Prefixes, modRmContext,
                    _instructionReader.UInt16.NextField(true)),
                _ => throw new InvalidGroupIndexException(_state, groupIndex)
            };
        }

        return groupIndex switch {
            0 => context.HasOperandSize32
                ? new Grp45RmInc32(context.Address, context.OpcodeField, context.Prefixes, modRmContext)
                : new Grp45RmInc16(context.Address, context.OpcodeField, context.Prefixes, modRmContext),
            1 => context.HasOperandSize32
                ? new Grp45RmDec32(context.Address, context.OpcodeField, context.Prefixes, modRmContext)
                : new Grp45RmDec16(context.Address, context.OpcodeField, context.Prefixes, modRmContext),
            2 => context.HasOperandSize32
                ? new Grp5RmCallNear32(context.Address, context.OpcodeField, context.Prefixes, modRmContext)
                : new Grp5RmCallNear16(context.Address, context.OpcodeField, context.Prefixes, modRmContext),
            3 => context.HasOperandSize32
                ? new Grp5RmCallFar32(context.Address, context.OpcodeField, context.Prefixes, _modRmParser.EnsureNotMode3(modRmContext))
                : new Grp5RmCallFar16(context.Address, context.OpcodeField, context.Prefixes, _modRmParser.EnsureNotMode3(modRmContext)),
            4 => new Grp5RmJumpNear(context.Address, context.OpcodeField, context.Prefixes, modRmContext),
            5 => new Grp5RmJumpFar(context.Address, context.OpcodeField, context.Prefixes, _modRmParser.EnsureNotMode3(modRmContext)),
            6 => context.HasOperandSize32
                ? new Grp5RmPush32(context.Address, context.OpcodeField, context.Prefixes, modRmContext)
                : new Grp5RmPush16(context.Address, context.OpcodeField, context.Prefixes, modRmContext),
            _ => throw new InvalidGroupIndexException(_state, groupIndex)
        };
    }
}