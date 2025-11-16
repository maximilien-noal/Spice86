namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.CommonGrammar;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// The class.
/// </summary>
public abstract class InstructionWithSegmentedAddressField : CfgInstruction {
    public InstructionWithSegmentedAddressField(
        SegmentedAddress address,
        InstructionField<ushort> opcodeField,
        List<InstructionPrefix> prefixes,
        InstructionField<SegmentedAddress> segmentedAddressField,
        int? maxSuccessorsCount) :
        base(address, opcodeField, prefixes, maxSuccessorsCount) {
        SegmentedAddressField = segmentedAddressField;
        AddField(segmentedAddressField);
    }

    /// <summary>
    /// Gets or sets the SegmentedAddressField.
    /// </summary>
    public InstructionField<SegmentedAddress> SegmentedAddressField { get; }
}