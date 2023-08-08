namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;

using System.Numerics;

public abstract class MovRegImm<T> : CfgInstruction where T : IUnsignedNumber<T> {
    public MovRegImm(uint physicalAddress, InstructionField<byte> opcodeField, IList<InstructionPrefix> prefixes,
        InstructionField<T> valueField, int regIndex) :
        base(physicalAddress, opcodeField, prefixes) {
        ValueField = valueField;
        FieldsInOrder.Add(ValueField);
        RegIndex = regIndex;
    }

    public InstructionField<T> ValueField { get; }
    public int RegIndex { get; }
}