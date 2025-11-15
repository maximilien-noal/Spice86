namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

/// <summary>
/// Represents mov acc moffs 8.
/// </summary>
[MovAccMoffs("AL", 8)]
public partial class MovAccMoffs8;

/// <summary>
/// Represents mov acc moffs 16.
/// </summary>
[MovAccMoffs("AX", 16)]
public partial class MovAccMoffs16;

/// <summary>
/// Represents mov acc moffs 32.
/// </summary>
[MovAccMoffs("EAX", 32)]
public partial class MovAccMoffs32;