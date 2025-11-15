namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.CommonGrammar;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.Interfaces;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents instruction with segment register index.
/// </summary>
public abstract class InstructionWithSegmentRegisterIndex : CfgInstruction, IInstructionWithSegmentRegisterIndex {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="opcodeField">The opcode field.</param>
    /// <param name="prefixes">The prefixes.</param>
    /// <param name="segmentRegisterIndex">The segment register index.</param>
    /// <param name="maxSuccessorsCount">The max successors count.</param>
    protected InstructionWithSegmentRegisterIndex(
        SegmentedAddress address,
        InstructionField<ushort> opcodeField,
        List<InstructionPrefix> prefixes,
        int segmentRegisterIndex,
        int? maxSuccessorsCount) : base(address, opcodeField, prefixes, maxSuccessorsCount) {
        SegmentRegisterIndex = segmentRegisterIndex;
    }

    /// <summary>
    /// Gets segment register index.
    /// </summary>
    public int SegmentRegisterIndex { get; }
}