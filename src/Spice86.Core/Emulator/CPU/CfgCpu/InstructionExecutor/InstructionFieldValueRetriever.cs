namespace Spice86.Core.Emulator.CPU.CfgCpu.InstructionExecutor;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.Memory.Indexable;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents instruction field value retriever.
/// </summary>
public class InstructionFieldValueRetriever {
    private IIndexable Memory { get; }

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="memory">The memory.</param>
    public InstructionFieldValueRetriever(IIndexable memory) {
        Memory = memory;
    }

    /// <summary>
    /// Gets field value.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <returns>The result of the operation.</returns>
    public byte GetFieldValue(InstructionField<byte> field) {
        if (field.UseValue) {
            return field.Value;
        }

        return Memory.UInt8[field.PhysicalAddress];
    }

    /// <summary>
    /// Gets field value.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <returns>The result of the operation.</returns>
    public ushort GetFieldValue(InstructionField<ushort> field) {
        if (field.UseValue) {
            return field.Value;
        }

        return Memory.UInt16[field.PhysicalAddress];
    }

    /// <summary>
    /// Gets field value.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <returns>The result of the operation.</returns>
    public uint GetFieldValue(InstructionField<uint> field) {
        if (field.UseValue) {
            return field.Value;
        }

        return Memory.UInt32[field.PhysicalAddress];
    }

    /// <summary>
    /// Gets field value.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <returns>The result of the operation.</returns>
    public sbyte GetFieldValue(InstructionField<sbyte> field) {
        if (field.UseValue) {
            return field.Value;
        }

        return Memory.Int8[field.PhysicalAddress];
    }

    /// <summary>
    /// Gets field value.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <returns>The result of the operation.</returns>
    public short GetFieldValue(InstructionField<short> field) {
        if (field.UseValue) {
            return field.Value;
        }

        return Memory.Int16[field.PhysicalAddress];
    }

    /// <summary>
    /// Gets field value.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <returns>The result of the operation.</returns>
    public int GetFieldValue(InstructionField<int> field) {
        if (field.UseValue) {
            return field.Value;
        }

        return Memory.Int32[field.PhysicalAddress];
    }

    /// <summary>
    /// Gets field value.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <returns>The result of the operation.</returns>
    public SegmentedAddress GetFieldValue(InstructionField<SegmentedAddress> field) {
        if (field.UseValue) {
            return field.Value;
        }

        return Memory.SegmentedAddress16[field.PhysicalAddress];
    }
}