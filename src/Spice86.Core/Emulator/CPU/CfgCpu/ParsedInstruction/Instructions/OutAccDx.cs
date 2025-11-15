namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

/// <summary>
/// Represents out acc dx 8.
/// </summary>
[OutAccDx("AL", 8)]
public partial class OutAccDx8;

/// <summary>
/// Represents out acc dx 16.
/// </summary>
[OutAccDx("AX", 16)]
public partial class OutAccDx16;

/// <summary>
/// Represents out acc dx 32.
/// </summary>
[OutAccDx("EAX", 32)]
public partial class OutAccDx32;