namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

/// <summary>
/// Represents inc reg 16.
/// </summary>
[IncDecReg("Inc", 16)]
public partial class IncReg16;

/// <summary>
/// Represents inc reg 32.
/// </summary>
[IncDecReg("Inc", 32)]
public partial class IncReg32;

/// <summary>
/// Represents dec reg 16.
/// </summary>
[IncDecReg("Dec", 16)]
public partial class DecReg16;

/// <summary>
/// Represents dec reg 32.
/// </summary>
[IncDecReg("Dec", 32)]
public partial class DecReg32;