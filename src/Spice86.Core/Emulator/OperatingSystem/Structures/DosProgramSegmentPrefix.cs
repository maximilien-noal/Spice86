namespace Spice86.Core.Emulator.OperatingSystem.Structures;

using Spice86.Core.Emulator.Memory.ReaderWriter;
using Spice86.Core.Emulator.ReverseEngineer.DataStructure;
using Spice86.Core.Emulator.ReverseEngineer.DataStructure.Array;

using System.Diagnostics;

/// <summary>
/// Represents the Program Segment Prefix (PSP)
/// </summary>
[DebuggerDisplay("BaseAddress={BaseAddress}, Parent={ParentProgramSegmentPrefix}, EnvSegment={EnvironmentTableSegment}, NextSegment={NextSegment}, StackPointer={StackPointer}, Cmd={DosCommandTail.Command}")]
public sealed class DosProgramSegmentPrefix : MemoryBasedDataStructure {
    /// <summary>
    /// The ushort.
    /// </summary>
    public const ushort MaxLength = 0x80 + 128;

    public DosProgramSegmentPrefix(IByteReaderWriter byteReaderWriter, uint baseAddress) : base(byteReaderWriter, baseAddress) {
    }

    /// <summary>
    /// CP/M like exit point for INT 0x20. (machine code: 0xCD, 0x20). Old way to exit the program.
    /// </summary>
    public UInt8Array Exit => GetUInt8Array(0x0, 2);

    /// <summary>
    /// Segment of first byte beyond the end of the program image. Reserved.
    /// </summary>
    public ushort NextSegment { get => UInt16[0x2]; set => UInt16[0x2] = value; }

    /// <summary>
    /// Reserved
    /// </summary>
    public byte Reserved { get => UInt8[0x4]; set => UInt8[0x4] = value; }

    /// <summary>
    /// Far call to DOS INT 0x21 dispatcher. Obsolete.
    /// </summary>
    public byte FarCall { get => UInt8[0x5]; set => UInt8[0x5] = value; }

    /// <summary>
    /// Gets or sets the CpmServiceRequestAddress.
    /// </summary>
    public uint CpmServiceRequestAddress { get => UInt32[0x6]; set => UInt32[0x6] = value; }

    /// <summary>
    /// On exit, DOS copies this to the INT 0x22 vector.
    /// </summary>
    public uint TerminateAddress { get => UInt32[0x0A]; set => UInt32[0x0A] = value; }

    /// <summary>
    /// On exit, DOS copies this to the INT 0x23 vector.
    /// </summary>
    public uint BreakAddress { get => UInt32[0x0E]; set => UInt32[0x0E] = value; }

    /// <summary>
    /// On exit, DOS copies this to the INT 0x24 vector.
    /// </summary>
    public uint CriticalErrorAddress { get => UInt32[0x12]; set => UInt32[0x12] = value; }

    /// <summary>
    /// Segment of PSP of parent program.
    /// </summary>
    public ushort ParentProgramSegmentPrefix { get => UInt16[0x16]; set => UInt16[0x16] = value; }

    /// <summary>
    /// Files method.
    /// </summary>
    public UInt8Array Files => GetUInt8Array(0x18, 20);

    /// <summary>
    /// Gets or sets the EnvironmentTableSegment.
    /// </summary>
    public ushort EnvironmentTableSegment { get => UInt16[0x2C]; set => UInt16[0x2C] = value; }

    /// <summary>
    /// Gets or sets the StackPointer.
    /// </summary>
    public uint StackPointer { get => UInt32[0x2E]; set => UInt32[0x2E] = value; }

    /// <summary>
    /// Gets or sets the MaximumOpenFiles.
    /// </summary>
    public ushort MaximumOpenFiles { get => UInt16[0x32]; set => UInt16[0x32] = value; }

    /// <summary>
    /// Gets or sets the FileTableAddress.
    /// </summary>
    public uint FileTableAddress { get => UInt32[0x34]; set => UInt32[0x34] = value; }

    /// <summary>
    /// Gets or sets the PreviousPspAddress.
    /// </summary>
    public uint PreviousPspAddress { get => UInt32[0x38]; set => UInt32[0x38] = value; }

    /// <summary>
    /// Gets or sets the InterimFlag.
    /// </summary>
    public byte InterimFlag { get => UInt8[0x3C]; set => UInt8[0x3C] = value; }

    /// <summary>
    /// Gets or sets the TrueNameFlag.
    /// </summary>
    public byte TrueNameFlag { get => UInt8[0x3D]; set => UInt8[0x3D] = value; }

    /// <summary>
    /// Gets or sets the NNFlags.
    /// </summary>
    public ushort NNFlags { get => UInt16[0x3E]; set => UInt16[0x3E] = value; }

    /// <summary>
    /// Gets or sets the DosVersionMajor.
    /// </summary>
    public byte DosVersionMajor { get => UInt8[0x40]; set => UInt8[0x40] = value; }

    /// <summary>
    /// Gets or sets the DosVersionMinor.
    /// </summary>
    public byte DosVersionMinor { get => UInt8[0x41]; set => UInt8[0x41] = value; }

    /// <summary>
    /// Unused method.
    /// </summary>
    public UInt8Array Unused => GetUInt8Array(0x42, 14);

    /// <summary>
    /// Service method.
    /// </summary>
    public UInt8Array Service => GetUInt8Array(0x50, 3);

    /// <summary>
    /// Unused2 method.
    /// </summary>
    public UInt8Array Unused2 => GetUInt8Array(0x53, 9);

    /// <summary>
    /// FirstFileControlBlock method.
    /// </summary>
    public UInt8Array FirstFileControlBlock => GetUInt8Array(0x5C, 16);

    /// <summary>
    /// SecondFileControlBlock method.
    /// </summary>
    public UInt8Array SecondFileControlBlock => GetUInt8Array(0x6C, 16);

    /// <summary>
    /// Unused3 method.
    /// </summary>
    public UInt8Array Unused3 => GetUInt8Array(0x7C, 4);

    /// <summary>
    /// DosCommandTail method.
    /// </summary>
    public DosCommandTail DosCommandTail => new(ByteReaderWriter, BaseAddress + 0x80);
}