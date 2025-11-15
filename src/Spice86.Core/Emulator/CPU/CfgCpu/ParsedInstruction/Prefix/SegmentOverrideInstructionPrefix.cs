namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;

using Spice86.Core.Emulator.CPU.Registers;

/// <summary>
/// Prefix representing that the instruction segmented address register will be overridden by segmentRegisterIndex
/// </summary>
public class SegmentOverrideInstructionPrefix : InstructionPrefix {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="prefixField">The prefix field.</param>
    /// <param name="segmentRegisterIndex">The segment register index.</param>
    public SegmentOverrideInstructionPrefix(InstructionField<byte> prefixField,
        SegmentRegisterIndex segmentRegisterIndex) : base(prefixField) {
        SegmentRegisterIndex = segmentRegisterIndex;
    }

    /// <summary>
    /// SegmentRegisterIndex defined in this prefix
    /// </summary>
    public SegmentRegisterIndex SegmentRegisterIndex { get; }

    /// <summary>
    /// value of the SegmentRegisterIndex
    /// </summary>
    public int SegmentRegisterIndexValue { get => (int)SegmentRegisterIndex; }
}