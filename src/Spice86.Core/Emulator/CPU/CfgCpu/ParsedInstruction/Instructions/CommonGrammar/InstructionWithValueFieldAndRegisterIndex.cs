namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.CommonGrammar;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.Interfaces;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Shared.Emulator.Memory;

using System.Numerics;

/// <summary>
/// The class.
/// </summary>
public abstract class InstructionWithValueFieldAndRegisterIndex<T> : InstructionWithValueField<T>, IInstructionWithRegisterIndex where T : INumberBase<T> {
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
    /// Gets or sets the RegisterIndex.
    /// </summary>
    public int RegisterIndex { get; }
}