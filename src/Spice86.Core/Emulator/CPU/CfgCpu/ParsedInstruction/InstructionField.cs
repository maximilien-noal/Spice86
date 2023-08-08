namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;

using System.Collections.Generic;

/// <summary>
/// Represents a field of an instruction.
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public class InstructionField<T> : IFieldWithValue {
    /// <summary>
    /// Index of the field in the enclosing instruction
    /// </summary>
    public int IndexInInstruction { get; }

    public InstructionField(int indexInInstruction, int length, uint physicalAddress, T value,
        IList<byte?> discriminatorValue) {
        IndexInInstruction = indexInInstruction;
        Length = length;
        PhysicalAddress = physicalAddress;
        Value = value;
        DiscriminatorValue = discriminatorValue;
        UseValue = true;
    }

    /// <summary>
    /// Length of this field
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Physical address of the field in memory
    /// </summary>
    public uint PhysicalAddress { get; }

    /// <summary>
    /// Value of the field at creation time. Meaningless if UseValue is false
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Differs to value for fields which do not represent something that changes CPU logic.
    /// Opcode would have value and valueForDiscriminator with the same value.
    /// For example in MOV AX, 1234:
    ///  - 1234 would be a InstructionField<ushort>
    ///  - value would be 1234
    ///  - ValueForDiscriminator would be [null, null]
    /// </summary>
    public IList<byte?> DiscriminatorValue { get; }

    /// <inheritdoc/>
    public bool UseValue { get; set; }


    /// <summary>
    /// Compares the positions and the value of this field with those of another field.
    /// </summary>
    /// <param name="other"></param>
    /// <returns>True if position and value is equals to the other field</returns>
    public bool IsValueAndPositionEquals(IFieldWithValue other) {
        if (other is InstructionField<T> otherT) {
            return this.PhysicalAddress == otherT.PhysicalAddress && this.Length == otherT.Length &&
                   this.IndexInInstruction == otherT.IndexInInstruction &&
                   EqualityComparer<T>.Default.Equals(this.Value, otherT.Value);
        }

        return false;
    }
}