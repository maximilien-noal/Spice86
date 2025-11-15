namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.CommonGrammar;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.Interfaces;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Shared.Emulator.Memory;

using System.Numerics;

/// <summary>
/// Represents instruction with value field.
/// </summary>
public abstract class InstructionWithValueField<T> : CfgInstruction, IInstructionWithValueField<T> where T : INumberBase<T> {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="opcodeField">The opcode field.</param>
    /// <param name="prefixes">The prefixes.</param>
    /// <param name="valueField">The value field.</param>
    /// <param name="maxSuccessorsCount">The max successors count.</param>
    protected InstructionWithValueField(SegmentedAddress address,
        InstructionField<ushort> opcodeField,
        List<InstructionPrefix> prefixes,
        InstructionField<T> valueField,
        int? maxSuccessorsCount) :
        base(address, opcodeField, prefixes, maxSuccessorsCount) {
        ValueField = valueField;
        AddField(ValueField);
    }

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="opcodeField">The opcode field.</param>
    /// <param name="valueField">The value field.</param>
    /// <param name="maxSuccessorsCount">The max successors count.</param>
    protected InstructionWithValueField(
        SegmentedAddress address,
        InstructionField<ushort> opcodeField,
        InstructionField<T> valueField,
        int? maxSuccessorsCount) : this(address,
        opcodeField, new List<InstructionPrefix>(), valueField, maxSuccessorsCount) {
    }

    /// <summary>
    /// Gets value field.
    /// </summary>
    public InstructionField<T> ValueField { get; }
}