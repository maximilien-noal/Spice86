namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.FieldReader;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.Memory.Indexable;
using Spice86.Shared.Emulator.Memory;

using System.Collections.Immutable;
using System.Linq;

/// <summary>
/// The class.
/// </summary>
public abstract class InstructionFieldReader<T> {
    protected IIndexable Memory { get; }

    protected InstructionReaderAddressSource AddressSource { get; }


    protected InstructionFieldReader(IIndexable memory, InstructionReaderAddressSource addressSource) {
        Memory = memory;
        AddressSource = addressSource;
    }

    protected SegmentedAddress CurrentAddress => AddressSource.CurrentAddress;

    protected abstract int FieldSize();
    /// <summary>
    /// T method.
    /// </summary>
    public abstract T PeekValue();

    /// <summary>
    /// Advance method.
    /// </summary>
    public void Advance() {
        AddressSource.IndexInInstruction += FieldSize();
    }
    /// <summary>
    /// Recede method.
    /// </summary>
    public void Recede() {
        AddressSource.IndexInInstruction -= FieldSize();
    }

    /// <summary>
    /// Reads field at current address.
    /// </summary>
    /// <param name="finalValue">If true, a change in value in memory will lead to the feeder to create a new instruction with the new value</param>
    /// <returns></returns>
    public virtual InstructionField<T> PeekField(bool finalValue) {
        T value = PeekValue();
        ImmutableList<byte?> bytes = PeekData(FieldSize());
        return new InstructionField<T>(AddressSource.IndexInInstruction, FieldSize(),
            CurrentAddress.Linear, value, bytes, finalValue);
    }

    /// <summary>
    /// Reads field at current address and advance read address to next.
    /// </summary>
    /// <param name="finalValue">If true, a change in value in memory will lead to the feeder to create a new instruction with the new value</param>
    /// <returns></returns>
    public InstructionField<T> NextField(bool finalValue) {
        InstructionField<T> res = PeekField(finalValue);
        Advance();
        return res;
    }

    private ImmutableList<byte?> PeekData(int size) {
        byte[] data = Memory.GetData(CurrentAddress.Linear, (uint)size);
        return data.
            Select(b => (byte?)b).
            ToImmutableList();
    }
}