namespace Spice86.Core.Emulator.OperatingSystem.Structures;

using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.Memory.ReaderWriter;
using Spice86.Shared.Emulator.Memory;
using Spice86.Shared.Utils;
using System;

/// <summary>
/// Centralizes global DOS memory structures
/// </summary>
public class DosTables {
    public const ushort DosPrivateTablesSegmentStart = 0xC800;
    public const ushort DosPrivateTablesSegmentEnd = 0xD000;

    public ushort CurrentMemorySegment { get; set; } = DosPrivateTablesSegmentStart;

    /// <summary>
    /// The current country information
    /// </summary>
    public CountryInfo CountryInfo { get; set; } = new();

    /// <summary>
    /// Gets the Current Directory Structure (CDS) for DOS drives.
    /// </summary>
    public CurrentDirectoryStructure? CurrentDirectoryStructure { get; private set; }

    /// <summary>
    /// Gets the Double Byte Character Set (DBCS) lead-byte table.
    /// </summary>
    public DosDoubleByteCharacterSet? DoubleByteCharacterSet { get; private set; }

    /// <summary>
    /// Initializes the DOS table structures in memory.
    /// </summary>
    /// <param name="memory">The memory interface to write structures to.</param>
    public void Initialize(IByteReaderWriter memory) {
        // Allocate CDS at fixed segment (DOS_CDS_SEG)
        uint cdsAddress = MemoryUtils.ToPhysicalAddress(MemoryMap.DosCdsSegment, 0);
        CurrentDirectoryStructure = new CurrentDirectoryStructure(memory, cdsAddress);

        // Allocate DBCS table - 12 bytes (less than 1 page, so allocate 1 page = 16 bytes)
        ushort dbcsSegment = GetDosPrivateTableWritableAddress(1);
        uint dbcsAddress = MemoryUtils.ToPhysicalAddress(dbcsSegment, 0);
        DoubleByteCharacterSet = new DosDoubleByteCharacterSet(memory, dbcsAddress);
    }

    public ushort GetDosPrivateTableWritableAddress(ushort pages) {
        if (pages + CurrentMemorySegment >= DosPrivateTablesSegmentEnd) {
            throw new InvalidOperationException("DOS: Not enough memory for internal tables!");
        }
        ushort page = CurrentMemorySegment;
        CurrentMemorySegment += pages;
        return page;
    }
}