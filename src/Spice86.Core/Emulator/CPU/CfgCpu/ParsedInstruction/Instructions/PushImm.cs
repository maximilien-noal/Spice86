namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

/// <summary>
/// Represents push imm 16.
/// </summary>
[PushImm(16, "ushort")]
public partial class PushImm16;
/// <summary>
/// Represents push imm 32.
/// </summary>
[PushImm(32, "uint")]
public partial class PushImm32;

/// <summary>
/// Represents push imm 8 sign extended 16.
/// </summary>
[PushImm8SignExtended(16, "short", "ushort")]
public partial class PushImm8SignExtended16;
/// <summary>
/// Represents push imm 8 sign extended 32.
/// </summary>
[PushImm8SignExtended(32, "int", "uint")]
public partial class PushImm8SignExtended32;