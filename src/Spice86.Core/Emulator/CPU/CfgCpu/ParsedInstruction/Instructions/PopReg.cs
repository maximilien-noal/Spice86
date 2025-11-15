namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

/// <summary>
/// Represents pop reg 16.
/// </summary>
[PopReg(16, false)]
public partial class PopReg16;

/// <summary>
/// Represents pop reg 32.
/// </summary>
[PopReg(32, false)]
public partial class PopReg32;

/// <summary>
/// Represents pops reg.
/// </summary>
[PopReg(16, true)]
public partial class PopSReg;