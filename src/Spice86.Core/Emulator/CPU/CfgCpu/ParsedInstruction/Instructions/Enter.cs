namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

/// <summary>
/// Represents enter 16.
/// </summary>
[Enter("SP", "ushort", "BP", 16)]
public partial class Enter16;
/// <summary>
/// Represents enter 32.
/// </summary>
[Enter("ESP", "uint", "EBP", 32)]
public partial class Enter32;