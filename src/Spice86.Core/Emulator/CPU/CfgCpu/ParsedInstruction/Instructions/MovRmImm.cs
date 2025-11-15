namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

/// <summary>
/// Represents mov rm imm 8.
/// </summary>
[MovRmImm(8, "byte")]
public partial class MovRmImm8;

/// <summary>
/// Represents mov rm imm 16.
/// </summary>
[MovRmImm(16, "ushort")]
public partial class MovRmImm16;

/// <summary>
/// Represents mov rm imm 32.
/// </summary>
[MovRmImm(32, "uint")]
public partial class MovRmImm32;