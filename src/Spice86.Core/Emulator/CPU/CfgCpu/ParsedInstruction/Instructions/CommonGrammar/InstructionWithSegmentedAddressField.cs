namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.CommonGrammar;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents instruction with segmented address field.
/// </summary>
public abstract class InstructionWithSegmentedAddressField : CfgInstruction {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="opcodeField">The opcode field.</param>
    /// <param name="prefixes">The prefixes.</param>
    /// <param name="segmentedAddressField">The segmented address field.</param>
    /// <param name="maxSuccessorsCount">The max successors count.</param>
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
    /// Gets segmented address field.
    /// </summary>
    public InstructionField<SegmentedAddress> SegmentedAddressField { get; }
}