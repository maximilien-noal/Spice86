namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

/// <summary>
/// Represents bt rm 16.
/// </summary>
[BitTestRm(16, "BT", "None")]
public partial class BtRm16;

/// <summary>
/// Represents bt rm 32.
/// </summary>
[BitTestRm(32, "BT", "None")]
public partial class BtRm32;

/// <summary>
/// Represents bts rm 16.
/// </summary>
[BitTestRm(16, "BTS", "Set")]
public partial class BtsRm16;

/// <summary>
/// Represents bts rm 32.
/// </summary>
[BitTestRm(32, "BTS", "Set")]
public partial class BtsRm32;

/// <summary>
/// Represents btr rm 16.
/// </summary>
[BitTestRm(16, "BTR", "Reset")]
public partial class BtrRm16;

/// <summary>
/// Represents btr rm 32.
/// </summary>
[BitTestRm(32, "BTR", "Reset")]
public partial class BtrRm32;

/// <summary>
/// Represents btc rm 16.
/// </summary>
[BitTestRm(16, "BTC", "Toggle")]
public partial class BtcRm16;

/// <summary>
/// Represents btc rm 32.
/// </summary>
[BitTestRm(32, "BTC", "Toggle")]
public partial class BtcRm32;

/// <summary>
/// Represents bt rm imm 16.
/// </summary>
[BitTestRmImm(16, "BT", "None")]
public partial class BtRmImm16;

/// <summary>
/// Represents bt rm imm 32.
/// </summary>
[BitTestRmImm(32, "BT", "None")]
public partial class BtRmImm32;

/// <summary>
/// Represents bts rm imm 16.
/// </summary>
[BitTestRmImm(16, "BTS", "Set")]
public partial class BtsRmImm16;

/// <summary>
/// Represents bts rm imm 32.
/// </summary>
[BitTestRmImm(32, "BTS", "Set")]
public partial class BtsRmImm32;

/// <summary>
/// Represents btr rm imm 16.
/// </summary>
[BitTestRmImm(16, "BTR", "Reset")]
public partial class BtrRmImm16;

/// <summary>
/// Represents btr rm imm 32.
/// </summary>
[BitTestRmImm(32, "BTR", "Reset")]
public partial class BtrRmImm32;

/// <summary>
/// Represents btc rm imm 16.
/// </summary>
[BitTestRmImm(16, "BTC", "Toggle")]
public partial class BtcRmImm16;

/// <summary>
/// Represents btc rm imm 32.
/// </summary>
[BitTestRmImm(32, "BTC", "Toggle")]
public partial class BtcRmImm32;

/// <summary>
/// Represents bsf rm 16.
/// </summary>
[BsfRm(16, "BSF")]
public partial class BsfRm16;

/// <summary>
/// Represents bsf rm 32.
/// </summary>
[BsfRm(32, "BSF")]
public partial class BsfRm32;

/// <summary>
/// Represents bsr rm 16.
/// </summary>
[BsrRm(16, "BSR")]
public partial class BsrRm16;

/// <summary>
/// Represents bsr rm 32.
/// </summary>
[BsrRm(32, "BSR")]
public partial class BsrRm32;