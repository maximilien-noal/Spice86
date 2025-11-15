namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.CommonGrammar;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.Interfaces;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.ModRm;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Shared.Emulator.Memory;

using System.Numerics;

/// <summary>
/// Represents instruction with mod rm and value field.
/// </summary>
public abstract class InstructionWithModRmAndValueField<T> : InstructionWithModRm, IInstructionWithValueField<T> where T : INumberBase<T> {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="opcodeField">The opcode field.</param>
    /// <param name="prefixes">The prefixes.</param>
    /// <param name="modRmContext">The mod rm context.</param>
    /// <param name="valueField">The value field.</param>
    /// <param name="maxSuccessorsCount">The max successors count.</param>
    protected InstructionWithModRmAndValueField(SegmentedAddress address, InstructionField<ushort> opcodeField, List<InstructionPrefix> prefixes,
        ModRmContext modRmContext, InstructionField<T> valueField, int? maxSuccessorsCount) : base(address, opcodeField, prefixes, modRmContext, maxSuccessorsCount) {
        ValueField = valueField;
        AddField(ValueField);
    }

    /// <summary>
    /// Gets value field.
    /// </summary>
    public InstructionField<T> ValueField { get; }
}