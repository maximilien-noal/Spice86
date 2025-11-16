namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.CommonGrammar;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.Interfaces;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// The class.
/// </summary>
public abstract class InstructionWithSegmentRegisterIndexAndOffsetField<T> : CfgInstruction, IInstructionWithSegmentRegisterIndex, IInstructionWithOffsetField<T> {
    protected InstructionWithSegmentRegisterIndexAndOffsetField(
        SegmentedAddress address,
        InstructionField<ushort> opcodeField,
        List<InstructionPrefix> prefixes,
        int segmentRegisterIndex,
        InstructionField<T> offsetField,
        int? maxSuccessorsCount) : base(address, opcodeField, prefixes, maxSuccessorsCount) {
        SegmentRegisterIndex = segmentRegisterIndex;
        OffsetField = offsetField;
        AddField(offsetField);
    }

    /// <summary>
    /// Gets or sets the SegmentRegisterIndex.
    /// </summary>
    public int SegmentRegisterIndex { get; }
    /// <summary>
    /// Gets or sets the OffsetField.
    /// </summary>
    public InstructionField<T> OffsetField { get; }
}