namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

/// <summary>
/// Represents leave 16.
/// </summary>
[Leave("SP", "BP", 16)]
public partial class Leave16;
/// <summary>
/// Represents leave 32.
/// </summary>
[Leave("ESP", "EBP", 32)]
public partial class Leave32;