namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

/// <summary>
/// Represents jmp near imm 8.
/// </summary>
[JmpNearImm("sbyte")]
public partial class JmpNearImm8;

/// <summary>
/// Represents jmp near imm 16.
/// </summary>
[JmpNearImm("short")]
public partial class JmpNearImm16;
/// <summary>
/// Represents jmp near imm 32.
/// </summary>
[JmpNearImm("int")]
public partial class JmpNearImm32;