namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

using System.Numerics;

public abstract class JmpNearImm<T> : CfgInstruction where T : ISignedNumber<T> {
    public JmpNearImm(uint physicalAddress, InstructionField<byte> opcodeField, InstructionField<T> offsetField) :
        base(physicalAddress, opcodeField) {
        OffsetField = offsetField;
        FieldsInOrder.Add(OffsetField);
    }

    public InstructionField<T> OffsetField { get; }
}