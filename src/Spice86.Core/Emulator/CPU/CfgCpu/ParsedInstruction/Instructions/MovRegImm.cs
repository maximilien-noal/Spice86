namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

/// <summary>
/// Represents mov reg imm 8.
/// </summary>
[MovRegImm(8, "8HighLow", "byte")]
public partial class MovRegImm8;

/// <summary>
/// Represents mov reg imm 16.
/// </summary>
[MovRegImm(16, "16", "ushort")]
public partial class MovRegImm16;

/// <summary>
/// Represents mov reg imm 32.
/// </summary>
[MovRegImm(32, "32", "uint")]
public partial class MovRegImm32;