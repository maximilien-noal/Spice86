namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.CommonGrammar;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.Interfaces;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents instruction with segment register index and offset field.
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
    /// Gets segment register index.
    /// </summary>
    public int SegmentRegisterIndex { get; }
    /// <summary>
    /// Gets offset field.
    /// </summary>
    public InstructionField<T> OffsetField { get; }
}