namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

/// <summary>
/// Represents set rmo.
/// </summary>
[SetRmcc("state.OverflowFlag", "o")]
public partial class SetRmo;

/// <summary>
/// Represents set rmno.
/// </summary>
[SetRmcc("!state.OverflowFlag", "no")]
public partial class SetRmno;

/// <summary>
/// Represents set rmb.
/// </summary>
[SetRmcc("state.CarryFlag", "b")]
public partial class SetRmb;

/// <summary>
/// Represents set rmae.
/// </summary>
[SetRmcc("!state.CarryFlag", "ae")]
public partial class SetRmae;

/// <summary>
/// Represents set rme.
/// </summary>
[SetRmcc("state.ZeroFlag", "e")]
public partial class SetRme;

/// <summary>
/// Represents set rmne.
/// </summary>
[SetRmcc("!state.ZeroFlag", "ne")]
public partial class SetRmne;

/// <summary>
/// Represents set rmbe.
/// </summary>
[SetRmcc("state.CarryFlag || state.ZeroFlag", "be")]
public partial class SetRmbe;

/// <summary>
/// Represents set rma.
/// </summary>
[SetRmcc("!state.CarryFlag && !state.ZeroFlag", "a")]
public partial class SetRma;

/// <summary>
/// Represents set rms.
/// </summary>
[SetRmcc("state.SignFlag", "s")]
public partial class SetRms;

/// <summary>
/// Represents set rmns.
/// </summary>
[SetRmcc("!state.SignFlag", "ns")]
public partial class SetRmns;

/// <summary>
/// Represents set rmp.
/// </summary>
[SetRmcc("state.ParityFlag", "p")]
public partial class SetRmp;

/// <summary>
/// Represents set rmnp.
/// </summary>
[SetRmcc("!state.ParityFlag", "np")]
public partial class SetRmnp;

/// <summary>
/// Represents set rml.
/// </summary>
[SetRmcc("state.SignFlag != state.OverflowFlag", "l")]
public partial class SetRml;

/// <summary>
/// Represents set rmge.
/// </summary>
[SetRmcc("state.SignFlag == state.OverflowFlag", "ge")]
public partial class SetRmge;

/// <summary>
/// Represents set rmle.
/// </summary>
[SetRmcc("state.ZeroFlag || state.SignFlag != state.OverflowFlag", "le")]
public partial class SetRmle;

/// <summary>
/// Represents set rmg.
/// </summary>
[SetRmcc("!state.ZeroFlag && state.SignFlag == state.OverflowFlag", "g")]
public partial class SetRmg;