namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

/// <summary>
/// Represents xchg reg acc 16.
/// </summary>
[XchgRegAcc(16, "AX")]
public partial class XchgRegAcc16;

/// <summary>
/// Represents xchg reg acc 32.
/// </summary>
[XchgRegAcc(32, "EAX")]
public partial class XchgRegAcc32;