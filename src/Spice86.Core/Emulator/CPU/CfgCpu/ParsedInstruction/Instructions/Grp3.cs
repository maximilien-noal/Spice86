namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

// TEST
/// <summary>
/// Represents grp 3 test rm imm 8.
/// </summary>
[Grp3TestRmImm(8, "byte")]
public partial class Grp3TestRmImm8;

/// <summary>
/// Represents grp 3 test rm imm 16.
/// </summary>
[Grp3TestRmImm(16, "ushort")]
public partial class Grp3TestRmImm16;

/// <summary>
/// Represents grp 3 test rm imm 32.
/// </summary>
[Grp3TestRmImm(32, "uint")]
public partial class Grp3TestRmImm32;

// NOT
/// <summary>
/// Represents grp 3 not rm 8.
/// </summary>
[Grp3NotRm(8, "byte")]
public partial class Grp3NotRm8;

/// <summary>
/// Represents grp 3 not rm 16.
/// </summary>
[Grp3NotRm(16, "ushort")]
public partial class Grp3NotRm16;

/// <summary>
/// Represents grp 3 not rm 32.
/// </summary>
[Grp3NotRm(32, "uint")]
public partial class Grp3NotRm32;

// NEG
/// <summary>
/// Represents grp 3 neg rm 8.
/// </summary>
[Grp3NegRm(8, "byte")]
public partial class Grp3NegRm8;

/// <summary>
/// Represents grp 3 neg rm 16.
/// </summary>
[Grp3NegRm(16, "ushort")]
public partial class Grp3NegRm16;

/// <summary>
/// Represents grp 3 neg rm 32.
/// </summary>
[Grp3NegRm(32, "uint")]
public partial class Grp3NegRm32;

// MUL
/// <summary>
/// Represents grp 3 mul rm acc 8.
/// </summary>
[Grp3MulRmAcc(8, "byte", "byte", "ushort", "AL", "AH", "Mul")]
public partial class Grp3MulRmAcc8;

/// <summary>
/// Represents grp 3 mul rm acc 16.
/// </summary>
[Grp3MulRmAcc(16, "ushort", "ushort", "uint", "AX", "DX", "Mul")]
public partial class Grp3MulRmAcc16;

/// <summary>
/// Represents grp 3 mul rm acc 32.
/// </summary>
[Grp3MulRmAcc(32, "uint", "uint", "ulong", "EAX", "EDX", "Mul")]
public partial class Grp3MulRmAcc32;

// IMUL
/// <summary>
/// Represents grp 3 imul rm acc 8.
/// </summary>
[Grp3MulRmAcc(8, "byte", "sbyte", "short", "AL", "AH", "Imul")]
public partial class Grp3ImulRmAcc8;

/// <summary>
/// Represents grp 3 imul rm acc 16.
/// </summary>
[Grp3MulRmAcc(16, "ushort", "short", "int", "AX", "DX", "Imul")]
public partial class Grp3ImulRmAcc16;

/// <summary>
/// Represents grp 3 imul rm acc 32.
/// </summary>
[Grp3MulRmAcc(32, "uint", "int", "long", "EAX", "EDX", "Imul")]
public partial class Grp3ImulRmAcc32;

// DIV
/// <summary>
/// Represents grp 3 div rm acc 8.
/// </summary>
[Grp3DivRmAcc(8, "byte", "byte", "ushort", "ushort", true, "AL", "AH", "Div")]
public partial class Grp3DivRmAcc8;

/// <summary>
/// Represents grp 3 div rm acc 16.
/// </summary>
[Grp3DivRmAcc(16, "ushort", "ushort", "uint", "uint", false, "AX", "DX", "Div")]
public partial class Grp3DivRmAcc16;

/// <summary>
/// Represents grp 3 div rm acc 32.
/// </summary>
[Grp3DivRmAcc(32, "uint", "uint", "ulong", "ulong", false, "EAX", "EDX", "Div")]
public partial class Grp3DivRmAcc32;

// IDIV
/// <summary>
/// Represents grp 3 idiv rm acc 8.
/// </summary>
[Grp3DivRmAcc(8, "byte", "sbyte", "ushort", "short", true, "AL", "AH", "Idiv")]
public partial class Grp3IdivRmAcc8;

/// <summary>
/// Represents grp 3 idiv rm acc 16.
/// </summary>
[Grp3DivRmAcc(16, "ushort", "short", "uint", "int", false, "AX", "DX", "Idiv")]
public partial class Grp3IdivRmAcc16;

/// <summary>
/// Represents grp 3 idiv rm acc 32.
/// </summary>
[Grp3DivRmAcc(32, "uint", "int", "ulong", "long", false, "EAX", "EDX", "Idiv")]
public partial class Grp3IdivRmAcc32;