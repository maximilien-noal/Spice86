namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

/// <summary>
/// Represents cmc.
/// </summary>
[FlagControl(FlagName: "CarryFlag", FlagValue: "!helper.State.CarryFlag", "CMC")]
public partial class Cmc;
/// <summary>
/// Represents clc.
/// </summary>
[FlagControl(FlagName: "CarryFlag", FlagValue: "false", "CLC")]
public partial class Clc;
/// <summary>
/// Represents stc.
/// </summary>
[FlagControl(FlagName: "CarryFlag", FlagValue: "true", "STC")]
public partial class Stc;
/// <summary>
/// Represents cli.
/// </summary>
[FlagControl(FlagName: "InterruptFlag", FlagValue: "false", "CLI")]
public partial class Cli;
/// <summary>
/// Represents sti.
/// </summary>
[FlagControl(FlagName: "InterruptFlag", FlagValue: "true", "STI")]
public partial class Sti;
/// <summary>
/// Represents cld.
/// </summary>
[FlagControl(FlagName: "DirectionFlag", FlagValue: "false", "CLD")]
public partial class Cld;
/// <summary>
/// Represents std.
/// </summary>
[FlagControl(FlagName: "DirectionFlag", FlagValue: "true", "STD")]
public partial class Std;