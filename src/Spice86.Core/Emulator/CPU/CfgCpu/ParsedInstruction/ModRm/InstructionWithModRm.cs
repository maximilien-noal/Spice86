namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.ModRm;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// The class.
/// </summary>
public abstract class InstructionWithModRm : CfgInstruction {
    public InstructionWithModRm(SegmentedAddress address, InstructionField<ushort> opcodeField, List<InstructionPrefix> prefixes, ModRmContext modRmContext, int? maxSuccessorsCount) : base(address, opcodeField, prefixes, maxSuccessorsCount) {
        ModRmContext = modRmContext;
        AddFields(ModRmContext.FieldsInOrder);
    }
    /// <summary>
    /// Gets or sets the ModRmContext.
    /// </summary>
    public ModRmContext ModRmContext { get; init; }
}