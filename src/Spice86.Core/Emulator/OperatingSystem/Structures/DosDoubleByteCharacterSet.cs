namespace Spice86.Core.Emulator.OperatingSystem.Structures;

using Spice86.Core.Emulator.Memory.ReaderWriter;
using Spice86.Core.Emulator.ReverseEngineer.DataStructure;

/// <summary>
/// Represents the DOS Double Byte Character Set (DBCS) lead-byte table.
/// The DBCS table is used for multi-byte character encodings like Japanese, Chinese, and Korean.
/// </summary>
/// <remarks>
/// In DOSBox, this is allocated with DOS_GetMemory(12) and initialized as an empty table.
/// An empty DBCS table (value 0) indicates no DBCS ranges are defined, meaning single-byte
/// character encoding is used (standard ASCII/extended ASCII).
/// </remarks>
public class DosDoubleByteCharacterSet : MemoryBasedDataStructure {
    /// <summary>
    /// Size of the DBCS table in bytes.
    /// </summary>
    public const int DbcsTableSize = 12; // Size in bytes (matches DOS_GetMemory(12) in pages where page=16 bytes)

    /// <summary>
    /// Initializes a new instance of the <see cref="DosDoubleByteCharacterSet"/> class.
    /// </summary>
    /// <param name="byteReaderWriter">The memory reader/writer interface.</param>
    /// <param name="baseAddress">The base address of the DBCS table in memory.</param>
    public DosDoubleByteCharacterSet(IByteReaderWriter byteReaderWriter, uint baseAddress)
        : base(byteReaderWriter, baseAddress) {
        // Initialize with empty table (all zeros)
        // mem_writed(RealToPhysical(dos.tables.dbcs),0); in DOSBox
        UInt32[0x00] = 0;
    }

    /// <summary>
    /// Gets the DBCS lead-byte table value.
    /// A value of 0 indicates an empty table (no DBCS ranges defined).
    /// </summary>
    public uint DbcsLeadByteTable {
        get => UInt32[0x00];
    }
}
