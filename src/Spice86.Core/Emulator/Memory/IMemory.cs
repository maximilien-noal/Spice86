namespace Spice86.Core.Emulator.Memory;

using Spice86.Core.Emulator.Memory.Indexable;
using Spice86.Core.Emulator.Memory.ReaderWriter;

/// <summary>
/// Represents the memory bus of the IBM PC.
/// </summary>
public interface IMemory : IIndexable, IByteReaderWriter {
    /// <summary>
    /// Underlying RAM for the memory bus
    /// </summary>
    public IMemoryDevice Ram { get; }

    /// <summary>
    /// Manages memory breakpoints
    /// </summary>
    MemoryBreakpoints MemoryBreakpoints { get; }

    /// <summary>
    /// Represents the optional 20th address line suppression feature for legacy 8086 programs.
    /// </summary>
    A20Gate A20Gate { get; }

    /// <summary>
    /// Gets a copy of the current memory state.
    /// </summary>
    byte[] RamCopy { get; }
    
    /// <summary>
    /// Returns a <see cref="Span{T}"/> that represents the specified range of memory.
    /// </summary>
    /// <param name="address">The starting address of the memory range.</param>
    /// <param name="length">The length of the memory range.</param>
    /// <returns>A <see cref="Span{T}"/> instance that represents the specified range of memory.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no memory device supports the specified memory range.</exception>
    public Span<byte> GetSpan(int address, int length);

    /// <summary>
    ///     Find the address of a value in memory.
    /// </summary>
    /// <param name="address">The address in memory to start the search from</param>
    /// <param name="len">The maximum amount of memory to search</param>
    /// <param name="value">The sequence of bytes to search for</param>
    /// <returns>The address of the first occurence of the specified sequence of bytes, or null if not found.</returns>
    uint? SearchValue(uint address, int len, IList<byte> value);
    
    /// <summary>
    ///     Allows write breakpoints to access the byte being written before it actually is.
    /// </summary>
    byte CurrentlyWritingByte { get; }

    /// <summary>
    ///     Allow a class to register for a certain memory range.
    /// </summary>
    /// <param name="baseAddress">The start of the frame</param>B
    /// <param name="size">The size of the window</param>
    /// <param name="memoryDevice">The memory device to use</param>
    public void RegisterMapping(uint baseAddress, uint size, IMemoryDevice memoryDevice);
}