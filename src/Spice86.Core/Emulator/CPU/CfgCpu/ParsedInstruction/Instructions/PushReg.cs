namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

/// <summary>
/// Represents push reg 16.
/// </summary>
[PushReg(16, false)]
public partial class PushReg16;

/// <summary>
/// Represents push reg 32.
/// </summary>
[PushReg(32, false)]
public partial class PushReg32;

/// <summary>
/// Represents pushs reg.
/// </summary>
[PushReg(16, true)]
public partial class PushSReg;