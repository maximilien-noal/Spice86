namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

/// <summary>
/// Represents mov rm sign extend byte 16.
/// </summary>
[MovRmSignExtend(DestSize: 16, SourceSize: 8, SourceSignedType: "sbyte", DestUnsignedType: "ushort")]
public partial class MovRmSignExtendByte16;

/// <summary>
/// Represents mov rm sign extend byte 32.
/// </summary>
[MovRmSignExtend(DestSize: 32, SourceSize: 8, SourceSignedType: "sbyte", DestUnsignedType: "uint")]
public partial class MovRmSignExtendByte32;

/// <summary>
/// Represents mov rm sign extend word 32.
/// </summary>
[MovRmSignExtend(DestSize: 32, SourceSize: 16, SourceSignedType: "short", DestUnsignedType: "uint")]
public partial class MovRmSignExtendWord32;