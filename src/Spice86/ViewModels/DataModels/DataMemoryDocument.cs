namespace Spice86.ViewModels.DataModels;

using AvaloniaHex.Document;

using Spice86.Core.Emulator.Memory;

/// <inheritdoc cref="IBinaryDocument" />
public sealed class DataMemoryDocument : IBinaryDocument {
    private readonly IMemory _memory;
    private readonly uint _startAddress;
    private readonly uint _endAddress;

    public DataMemoryDocument(IMemory memory, uint startAddress, uint endAddress) {
        IsReadOnly = false;
        CanInsert = false;
        CanRemove = false;
        _startAddress = startAddress;
        _endAddress = endAddress;
        _memory = memory;
        ValidRanges = new MemoryReadOnlyBitRangeUnion(0, _endAddress - _startAddress);
    }

    /// <summary>
    /// The Action.
    /// </summary>
    public event Action<Exception>? MemoryReadInvalidOperation;

    /// <summary>
    /// The Length.
    /// </summary>
    public ulong Length => _endAddress - _startAddress;
    /// <summary>
    /// Gets or sets the IsReadOnly.
    /// </summary>
    public bool IsReadOnly { get; }
    /// <summary>
    /// Gets or sets the CanInsert.
    /// </summary>
    public bool CanInsert { get; }
    /// <summary>
    /// Gets or sets the CanRemove.
    /// </summary>
    public bool CanRemove { get; }
    /// <summary>
    /// Gets or sets the ValidRanges.
    /// </summary>
    public IReadOnlyBitRangeUnion ValidRanges { get; }

#pragma warning disable CS0067 // Binary document is write-through; UI listens to memory polling instead
    /// <summary>
    /// The EventHandler.
    /// </summary>
    public event EventHandler<BinaryDocumentChange>? Changed;
#pragma warning restore CS0067

    /// <summary>
    /// InsertBytes method.
    /// </summary>
    public void InsertBytes(ulong offset, ReadOnlySpan<byte> buffer) {
        throw new NotSupportedException();
    }

    /// <summary>
    /// ReadBytes method.
    /// </summary>
    public void ReadBytes(ulong offset, Span<byte> buffer) {
        if (buffer.Length == 0) {
            return;
        }
        try {
            Span<byte> memRange = _memory.ReadRam((uint)buffer.Length, (uint)(_startAddress + offset));
            memRange.CopyTo(buffer);
        } catch (Exception e) when (e is ArgumentException or InvalidOperationException) {
            MemoryReadInvalidOperation?.Invoke(e);
        }
    }

    /// <summary>
    /// RemoveBytes method.
    /// </summary>
    public void RemoveBytes(ulong offset, ulong length) {
        throw new NotSupportedException();
    }

    /// <summary>
    /// WriteBytes method.
    /// </summary>
    public void WriteBytes(ulong address, ReadOnlySpan<byte> buffer) {
        _memory.WriteRam(buffer.ToArray(), (uint)address);
    }

    /// <summary>
    /// Does nothing
    /// </summary>
    public void Flush() {
        //NOP
    }

    /// <summary>
    /// Does nothing
    /// </summary>
    public void Dispose() {
        //NOP
    }
}