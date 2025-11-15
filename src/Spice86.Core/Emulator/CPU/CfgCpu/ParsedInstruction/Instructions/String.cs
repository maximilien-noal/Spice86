namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

/// <summary>
/// Represents movs 8.
/// </summary>
[Movs(8)]
public partial class Movs8;
/// <summary>
/// Represents movs 16.
/// </summary>
[Movs(16)]
public partial class Movs16;
/// <summary>
/// Represents movs 32.
/// </summary>
[Movs(32)]
public partial class Movs32;

/// <summary>
/// Represents ins dx 8.
/// </summary>
[InsDx(8)]
public partial class InsDx8;
/// <summary>
/// Represents ins dx 16.
/// </summary>
[InsDx(16)]
public partial class InsDx16;
/// <summary>
/// Represents ins dx 32.
/// </summary>
[InsDx(32)]
public partial class InsDx32;

/// <summary>
/// Represents outs dx 8.
/// </summary>
[OutsDx(8, "byte")]
public partial class OutsDx8;
/// <summary>
/// Represents outs dx 16.
/// </summary>
[OutsDx(16, "ushort")]
public partial class OutsDx16;
/// <summary>
/// Represents outs dx 32.
/// </summary>
[OutsDx(32, "uint")]
public partial class OutsDx32;

/// <summary>
/// Represents cmps 8.
/// </summary>
[Cmps(8)]
public partial class Cmps8;
/// <summary>
/// Represents cmps 16.
/// </summary>
[Cmps(16)]
public partial class Cmps16;
/// <summary>
/// Represents cmps 32.
/// </summary>
[Cmps(32)]
public partial class Cmps32;

/// <summary>
/// Represents stos 8.
/// </summary>
[Stos(8, "AL")]
public partial class Stos8;
/// <summary>
/// Represents stos 16.
/// </summary>
[Stos(16, "AX")]
public partial class Stos16;
/// <summary>
/// Represents stos 32.
/// </summary>
[Stos(32, "EAX")]
public partial class Stos32;

/// <summary>
/// Represents lods 8.
/// </summary>
[Lods(8, "AL")]
public partial class Lods8;
/// <summary>
/// Represents lods 16.
/// </summary>
[Lods(16, "AX")]
public partial class Lods16;
/// <summary>
/// Represents lods 32.
/// </summary>
[Lods(32, "EAX")]
public partial class Lods32;

/// <summary>
/// Represents scas 8.
/// </summary>
[Scas(8, "AL")]
public partial class Scas8;
/// <summary>
/// Represents scas 16.
/// </summary>
[Scas(16, "AX")]
public partial class Scas16;
/// <summary>
/// Represents scas 32.
/// </summary>
[Scas(32, "EAX")]
public partial class Scas32;