namespace Spice86.Core.Emulator.OperatingSystem.Structures;

using Spice86.Core.Emulator.Memory.ReaderWriter;
using Spice86.Core.Emulator.ReverseEngineer.DataStructure;

/// <summary>
/// Represents a DOS Current Directory Structure (CDS) entry.
/// Each CDS entry contains information about a drive's current directory.
/// </summary>
/// <remarks>
/// The CDS structure in DOS contains the current directory path for each drive.
/// This is a simplified read-only implementation for compatibility.
/// </remarks>
public class CurrentDirectoryStructure : MemoryBasedDataStructure {
    /// <summary>
    /// Size of a single CDS entry in bytes.
    /// </summary>
    public const int CdsEntrySize = 0x58; // 88 bytes per entry in DOS 5.0+

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentDirectoryStructure"/> class.
    /// </summary>
    /// <param name="byteReaderWriter">The memory reader/writer interface.</param>
    /// <param name="baseAddress">The base address of the CDS structure in memory.</param>
    public CurrentDirectoryStructure(IByteReaderWriter byteReaderWriter, uint baseAddress)
        : base(byteReaderWriter, baseAddress) {
        // Initialize with "C:\" - this is what DOSBox does
        // 0x005c3a43 = 'C', ':', '\', 0x00 in little-endian
        UInt32[0x00] = 0x005c3a43;
    }

    /// <summary>
    /// Gets the current directory path as a 4-byte value.
    /// This represents "C:\" in the default configuration.
    /// </summary>
    public uint CurrentPath {
        get => UInt32[0x00];
    }
}
