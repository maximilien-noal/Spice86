namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

/// <summary>
/// Represents cmpxchg rm 8.
/// </summary>
[CmpxchgRm(8, "AL")]
public partial class CmpxchgRm8;

/// <summary>
/// Represents cmpxchg rm 16.
/// </summary>
[CmpxchgRm(16, "AX")]
public partial class CmpxchgRm16;

/// <summary>
/// Represents cmpxchg rm 32.
/// </summary>
[CmpxchgRm(32, "EAX")]
public partial class CmpxchgRm32;