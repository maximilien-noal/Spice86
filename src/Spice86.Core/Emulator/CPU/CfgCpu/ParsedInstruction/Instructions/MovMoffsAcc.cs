namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

/// <summary>
/// Represents mov moffs acc 8.
/// </summary>
[MovMoffsAcc("AL", 8)]
public partial class MovMoffsAcc8;

/// <summary>
/// Represents mov moffs acc 16.
/// </summary>
[MovMoffsAcc("AX", 16)]
public partial class MovMoffsAcc16;

/// <summary>
/// Represents mov moffs acc 32.
/// </summary>
[MovMoffsAcc("EAX", 32)]
public partial class MovMoffsAcc32;