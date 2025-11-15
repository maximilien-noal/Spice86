namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.CommonGrammar;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.Interfaces;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Shared.Emulator.Memory;

using System.Numerics;

/// <summary>
/// Represents instruction with value field and register index.
/// </summary>
public abstract class InstructionWithValueFieldAndRegisterIndex<T> : InstructionWithValueField<T>, IInstructionWithRegisterIndex where T : INumberBase<T> {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="opcodeField">The opcode field.</param>
    /// <param name="prefixes">The prefixes.</param>
    /// <param name="valueField">The value field.</param>
    /// <param name="registerIndex">The register index.</param>
    /// <param name="maxSuccessorsCount">The max successors count.</param>
    public InstructionWithValueFieldAndRegisterIndex(SegmentedAddress address,
        InstructionField<ushort> opcodeField,
        List<InstructionPrefix> prefixes,
        InstructionField<T> valueField,
        int registerIndex,
        int? maxSuccessorsCount) :
        base(address, opcodeField, prefixes, valueField, maxSuccessorsCount) {
        RegisterIndex = registerIndex;
    }

    /// <summary>
    /// Gets register index.
    /// </summary>
    public int RegisterIndex { get; }
}