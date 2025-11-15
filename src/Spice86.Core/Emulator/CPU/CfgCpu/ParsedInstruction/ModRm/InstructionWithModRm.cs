namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.ModRm;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents instruction with mod rm.
/// </summary>
public abstract class InstructionWithModRm : CfgInstruction {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="opcodeField">The opcode field.</param>
    /// <param name="prefixes">The prefixes.</param>
    /// <param name="modRmContext">The mod rm context.</param>
    /// <param name="maxSuccessorsCount">The max successors count.</param>
    public InstructionWithModRm(SegmentedAddress address, InstructionField<ushort> opcodeField, List<InstructionPrefix> prefixes, ModRmContext modRmContext, int? maxSuccessorsCount) : base(address, opcodeField, prefixes, maxSuccessorsCount) {
        ModRmContext = modRmContext;
        AddFields(ModRmContext.FieldsInOrder);
    }
    /// <summary>
    /// Gets mod rm context.
    /// </summary>
    public ModRmContext ModRmContext { get; init; }
}