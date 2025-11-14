namespace Spice86.Core.Emulator.OperatingSystem.Structures;

using Spice86.Core.Emulator.Memory.ReaderWriter;
using Spice86.Core.Emulator.ReverseEngineer.DataStructure;
using Spice86.Shared.Emulator.Memory;

using System.Diagnostics;

/// <summary>
/// Represents the DOS EXEC Parameter Block used for INT 21h function 4Bh.
/// This structure is passed when loading or executing a program.
/// </summary>
[DebuggerDisplay("EnvSegment={EnvironmentSegment}, CmdLine={CommandLineAddress}, Fcb1={Fcb1Address}, Fcb2={Fcb2Address}")]
public sealed class DosExecParameterBlock : MemoryBasedDataStructure {
    /// <summary>
    /// Size of the EXEC parameter block in bytes.
    /// </summary>
    public const ushort Size = 14;

    /// <summary>
    /// Initializes a new instance of the <see cref="DosExecParameterBlock"/> class.
    /// </summary>
    /// <param name="byteReaderWriter">Where data is read and written.</param>
    /// <param name="baseAddress">The base address of the structure in memory.</param>
    public DosExecParameterBlock(IByteReaderWriter byteReaderWriter, uint baseAddress) 
        : base(byteReaderWriter, baseAddress) {
    }

    /// <summary>
    /// Gets or sets the segment address of the environment block to be passed to the child process.
    /// If 0, the child inherits the parent's environment.
    /// </summary>
    public ushort EnvironmentSegment { 
        get => UInt16[0x00]; 
        set => UInt16[0x00] = value; 
    }

    /// <summary>
    /// Gets or sets the segmented address of the command line to be passed to the child process.
    /// Format: segment:offset pointing to a command tail structure (length byte + command string + CR).
    /// </summary>
    public SegmentedAddress CommandLineAddress {
        get => new SegmentedAddress(UInt16[0x04], UInt16[0x02]);
        set {
            UInt16[0x02] = value.Offset;
            UInt16[0x04] = value.Segment;
        }
    }

    /// <summary>
    /// Gets or sets the segmented address of the first FCB (File Control Block) to be passed to the child.
    /// Points to a pre-filled FCB at offset 0x5C in the child's PSP.
    /// </summary>
    public SegmentedAddress Fcb1Address {
        get => new SegmentedAddress(UInt16[0x08], UInt16[0x06]);
        set {
            UInt16[0x06] = value.Offset;
            UInt16[0x08] = value.Segment;
        }
    }

    /// <summary>
    /// Gets or sets the segmented address of the second FCB to be passed to the child.
    /// Points to a pre-filled FCB at offset 0x6C in the child's PSP.
    /// </summary>
    public SegmentedAddress Fcb2Address {
        get => new SegmentedAddress(UInt16[0x0C], UInt16[0x0A]);
        set {
            UInt16[0x0A] = value.Offset;
            UInt16[0x0C] = value.Segment;
        }
    }
}

/// <summary>
/// Represents the DOS LOAD Parameter Block used for INT 21h function 4Bh with AL=0x03.
/// This structure is used when loading a program without executing it.
/// </summary>
[DebuggerDisplay("LoadSegment={LoadSegment}, RelocFactor={RelocationFactor}")]
public sealed class DosLoadParameterBlock : MemoryBasedDataStructure {
    /// <summary>
    /// Size of the LOAD parameter block in bytes.
    /// </summary>
    public const ushort Size = 4;

    /// <summary>
    /// Initializes a new instance of the <see cref="DosLoadParameterBlock"/> class.
    /// </summary>
    /// <param name="byteReaderWriter">Where data is read and written.</param>
    /// <param name="baseAddress">The base address of the structure in memory.</param>
    public DosLoadParameterBlock(IByteReaderWriter byteReaderWriter, uint baseAddress) 
        : base(byteReaderWriter, baseAddress) {
    }

    /// <summary>
    /// Gets or sets the segment address where the program should be loaded.
    /// </summary>
    public ushort LoadSegment { 
        get => UInt16[0x00]; 
        set => UInt16[0x00] = value; 
    }

    /// <summary>
    /// Gets or sets the relocation factor to use when applying fixups.
    /// This value is added to each relocation entry in the EXE file.
    /// </summary>
    public ushort RelocationFactor { 
        get => UInt16[0x02]; 
        set => UInt16[0x02] = value; 
    }
}

/// <summary>
/// Represents the DOS OVERLAY Parameter Block used for INT 21h function 4Bh with AL=0x01.
/// This structure is used when loading a program as an overlay.
/// </summary>
[DebuggerDisplay("LoadSegment={LoadSegment}, RelocFactor={RelocationFactor}")]
public sealed class DosOverlayParameterBlock : MemoryBasedDataStructure {
    /// <summary>
    /// Size of the OVERLAY parameter block in bytes.
    /// </summary>
    public const ushort Size = 4;

    /// <summary>
    /// Initializes a new instance of the <see cref="DosOverlayParameterBlock"/> class.
    /// </summary>
    /// <param name="byteReaderWriter">Where data is read and written.</param>
    /// <param name="baseAddress">The base address of the structure in memory.</param>
    public DosOverlayParameterBlock(IByteReaderWriter byteReaderWriter, uint baseAddress) 
        : base(byteReaderWriter, baseAddress) {
    }

    /// <summary>
    /// Gets or sets the segment address where the overlay should be loaded.
    /// </summary>
    public ushort LoadSegment { 
        get => UInt16[0x00]; 
        set => UInt16[0x00] = value; 
    }

    /// <summary>
    /// Gets or sets the relocation factor to use when applying fixups.
    /// </summary>
    public ushort RelocationFactor { 
        get => UInt16[0x02]; 
        set => UInt16[0x02] = value; 
    }
}
