namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

/// <summary>
/// Represents out acc imm 8.
/// </summary>
[OutAccImm("AL", 8)]
public partial class OutAccImm8;

/// <summary>
/// Represents out acc imm 16.
/// </summary>
[OutAccImm("AX", 16)]
public partial class OutAccImm16;

/// <summary>
/// Represents out acc imm 32.
/// </summary>
[OutAccImm("EAX", 32)]
public partial class OutAccImm32;