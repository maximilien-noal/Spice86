namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

/// <summary>
/// Represents loop 16.
/// </summary>
[Loop("CX", null, "loop")]
public partial class Loop16;
/// <summary>
/// Represents loop 32.
/// </summary>
[Loop("ECX", null, "loop")]
public partial class Loop32;

/// <summary>
/// Represents loopz 16.
/// </summary>
[Loop("CX", "helper.State.ZeroFlag", "loope")]
public partial class Loopz16;
/// <summary>
/// Represents loopz 32.
/// </summary>
[Loop("ECX", "helper.State.ZeroFlag", "loope")]
public partial class Loopz32;

/// <summary>
/// Represents loopnz 16.
/// </summary>
[Loop("CX", "!helper.State.ZeroFlag", "loopne")]
public partial class Loopnz16;
/// <summary>
/// Represents loopnz 32.
/// </summary>
[Loop("ECX", "!helper.State.ZeroFlag", "loopne")]
public partial class Loopnz32;