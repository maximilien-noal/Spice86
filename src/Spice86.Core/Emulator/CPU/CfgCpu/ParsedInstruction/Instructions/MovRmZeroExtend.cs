namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

/// <summary>
/// Represents mov rm zero extend byte 16.
/// </summary>
[MovRmZeroExtend(DestSize: 16, SourceSize: 8)]
public partial class MovRmZeroExtendByte16;

/// <summary>
/// Represents mov rm zero extend byte 32.
/// </summary>
[MovRmZeroExtend(DestSize: 32, SourceSize: 8)]
public partial class MovRmZeroExtendByte32;

/// <summary>
/// Represents mov rm zero extend word 32.
/// </summary>
[MovRmZeroExtend(DestSize: 32, SourceSize: 16)]
public partial class MovRmZeroExtendWord32;