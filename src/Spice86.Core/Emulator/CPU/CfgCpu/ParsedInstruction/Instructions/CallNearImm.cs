namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

/// <summary>
/// Represents call near imm 16.
/// </summary>
[CallNearImm("short", 16)]
public partial class CallNearImm16;

/// <summary>
/// Represents call near imm 32.
/// </summary>
[CallNearImm("int", 32)]
public partial class CallNearImm32;