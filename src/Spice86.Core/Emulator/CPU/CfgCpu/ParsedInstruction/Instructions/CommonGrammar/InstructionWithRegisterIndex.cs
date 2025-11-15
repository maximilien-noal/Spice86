namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.CommonGrammar;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.Interfaces;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents instruction with register index.
/// </summary>
public abstract class InstructionWithRegisterIndex : CfgInstruction, IInstructionWithRegisterIndex {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="opcodeField">The opcode field.</param>
    /// <param name="prefixes">The prefixes.</param>
    /// <param name="registerIndex">The register index.</param>
    /// <param name="maxSuccessorsCount">The max successors count.</param>
    protected InstructionWithRegisterIndex(SegmentedAddress address,
        InstructionField<ushort> opcodeField,
        List<InstructionPrefix> prefixes,
        int registerIndex, int? maxSuccessorsCount) :
        base(address, opcodeField, prefixes, maxSuccessorsCount) {
        RegisterIndex = registerIndex;
    }
    /// <summary>
    /// Gets register index.
    /// </summary>
    public int RegisterIndex { get; }
}