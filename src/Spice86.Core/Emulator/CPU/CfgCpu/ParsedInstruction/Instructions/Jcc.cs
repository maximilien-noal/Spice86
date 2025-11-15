namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;


/// <summary>
/// Represents jo 8.
/// </summary>
[JccNearImm(8, "sbyte", "state.OverflowFlag", "jo")]
public partial class Jo8;

/// <summary>
/// Represents jno 8.
/// </summary>
[JccNearImm(8, "sbyte", "!state.OverflowFlag", "jno")]
public partial class Jno8;

/// <summary>
/// Represents jb 8.
/// </summary>
[JccNearImm(8, "sbyte", "state.CarryFlag", "jb")]
public partial class Jb8;

/// <summary>
/// Represents jae 8.
/// </summary>
[JccNearImm(8, "sbyte", "!state.CarryFlag", "jae")]
public partial class Jae8;

/// <summary>
/// Represents je 8.
/// </summary>
[JccNearImm(8, "sbyte", "state.ZeroFlag", "je")]
public partial class Je8;

/// <summary>
/// Represents jne 8.
/// </summary>
[JccNearImm(8, "sbyte", "!state.ZeroFlag", "jne")]
public partial class Jne8;

/// <summary>
/// Represents jbe 8.
/// </summary>
[JccNearImm(8, "sbyte", "state.CarryFlag || state.ZeroFlag", "jbe")]
public partial class Jbe8;

/// <summary>
/// Represents ja 8.
/// </summary>
[JccNearImm(8, "sbyte", "!state.CarryFlag && !state.ZeroFlag", "ja")]
public partial class Ja8;

/// <summary>
/// Represents js 8.
/// </summary>
[JccNearImm(8, "sbyte", "state.SignFlag", "js")]
public partial class Js8;

/// <summary>
/// Represents jns 8.
/// </summary>
[JccNearImm(8, "sbyte", "!state.SignFlag", "jns")]
public partial class Jns8;

/// <summary>
/// Represents jp 8.
/// </summary>
[JccNearImm(8, "sbyte", "state.ParityFlag", "jp")]
public partial class Jp8;

/// <summary>
/// Represents jnp 8.
/// </summary>
[JccNearImm(8, "sbyte", "!state.ParityFlag", "jnp")]
public partial class Jnp8;

/// <summary>
/// Represents jl 8.
/// </summary>
[JccNearImm(8, "sbyte", "state.SignFlag != state.OverflowFlag", "jl")]
public partial class Jl8;

/// <summary>
/// Represents jge 8.
/// </summary>
[JccNearImm(8, "sbyte", "state.SignFlag == state.OverflowFlag", "jge")]
public partial class Jge8;

/// <summary>
/// Represents jle 8.
/// </summary>
[JccNearImm(8, "sbyte", "state.ZeroFlag || state.SignFlag != state.OverflowFlag", "jle")]
public partial class Jle8;

/// <summary>
/// Represents jg 8.
/// </summary>
[JccNearImm(8, "sbyte", "!state.ZeroFlag && state.SignFlag == state.OverflowFlag", "jg")]
public partial class Jg8;

/// <summary>
/// Represents jo 16.
/// </summary>
[JccNearImm(16, "short", "state.OverflowFlag", "jo")]
public partial class Jo16;

/// <summary>
/// Represents jno 16.
/// </summary>
[JccNearImm(16, "short", "!state.OverflowFlag", "jno")]
public partial class Jno16;

/// <summary>
/// Represents jb 16.
/// </summary>
[JccNearImm(16, "short", "state.CarryFlag", "jb")]
public partial class Jb16;

/// <summary>
/// Represents jae 16.
/// </summary>
[JccNearImm(16, "short", "!state.CarryFlag", "jae")]
public partial class Jae16;

/// <summary>
/// Represents je 16.
/// </summary>
[JccNearImm(16, "short", "state.ZeroFlag", "je")]
public partial class Je16;

/// <summary>
/// Represents jne 16.
/// </summary>
[JccNearImm(16, "short", "!state.ZeroFlag", "jne")]
public partial class Jne16;

/// <summary>
/// Represents jbe 16.
/// </summary>
[JccNearImm(16, "short", "state.CarryFlag || state.ZeroFlag", "jbe")]
public partial class Jbe16;

/// <summary>
/// Represents ja 16.
/// </summary>
[JccNearImm(16, "short", "!state.CarryFlag && !state.ZeroFlag", "ja")]
public partial class Ja16;

/// <summary>
/// Represents js 16.
/// </summary>
[JccNearImm(16, "short", "state.SignFlag", "js")]
public partial class Js16;

/// <summary>
/// Represents jns 16.
/// </summary>
[JccNearImm(16, "short", "!state.SignFlag", "jns")]
public partial class Jns16;

/// <summary>
/// Represents jp 16.
/// </summary>
[JccNearImm(16, "short", "state.ParityFlag", "jp")]
public partial class Jp16;

/// <summary>
/// Represents jnp 16.
/// </summary>
[JccNearImm(16, "short", "!state.ParityFlag", "jnp")]
public partial class Jnp16;

/// <summary>
/// Represents jl 16.
/// </summary>
[JccNearImm(16, "short", "state.SignFlag != state.OverflowFlag", "jl")]
public partial class Jl16;

/// <summary>
/// Represents jge 16.
/// </summary>
[JccNearImm(16, "short", "state.SignFlag == state.OverflowFlag", "jge")]
public partial class Jge16;

/// <summary>
/// Represents jle 16.
/// </summary>
[JccNearImm(16, "short", "state.ZeroFlag || state.SignFlag != state.OverflowFlag", "jle")]
public partial class Jle16;

/// <summary>
/// Represents jg 16.
/// </summary>
[JccNearImm(16, "short", "!state.ZeroFlag && state.SignFlag == state.OverflowFlag", "jg")]
public partial class Jg16;

/// <summary>
/// Represents jo 32.
/// </summary>
[JccNearImm(32, "int", "state.OverflowFlag", "jo")]
public partial class Jo32;

/// <summary>
/// Represents jno 32.
/// </summary>
[JccNearImm(32, "int", "!state.OverflowFlag", "jno")]
public partial class Jno32;

/// <summary>
/// Represents jb 32.
/// </summary>
[JccNearImm(32, "int", "state.CarryFlag", "jb")]
public partial class Jb32;

/// <summary>
/// Represents jae 32.
/// </summary>
[JccNearImm(32, "int", "!state.CarryFlag", "jae")]
public partial class Jae32;

/// <summary>
/// Represents je 32.
/// </summary>
[JccNearImm(32, "int", "state.ZeroFlag", "je")]
public partial class Je32;

/// <summary>
/// Represents jne 32.
/// </summary>
[JccNearImm(32, "int", "!state.ZeroFlag", "jne")]
public partial class Jne32;

/// <summary>
/// Represents jbe 32.
/// </summary>
[JccNearImm(32, "int", "state.CarryFlag || state.ZeroFlag", "jbe")]
public partial class Jbe32;

/// <summary>
/// Represents ja 32.
/// </summary>
[JccNearImm(32, "int", "!state.CarryFlag && !state.ZeroFlag", "ja")]
public partial class Ja32;

/// <summary>
/// Represents js 32.
/// </summary>
[JccNearImm(32, "int", "state.SignFlag", "js")]
public partial class Js32;

/// <summary>
/// Represents jns 32.
/// </summary>
[JccNearImm(32, "int", "!state.SignFlag", "jns")]
public partial class Jns32;

/// <summary>
/// Represents jp 32.
/// </summary>
[JccNearImm(32, "int", "state.ParityFlag", "jp")]
public partial class Jp32;

/// <summary>
/// Represents jnp 32.
/// </summary>
[JccNearImm(32, "int", "!state.ParityFlag", "jnp")]
public partial class Jnp32;

/// <summary>
/// Represents jl 32.
/// </summary>
[JccNearImm(32, "int", "state.SignFlag != state.OverflowFlag", "jl")]
public partial class Jl32;

/// <summary>
/// Represents jge 32.
/// </summary>
[JccNearImm(32, "int", "state.SignFlag == state.OverflowFlag", "jge")]
public partial class Jge32;

/// <summary>
/// Represents jle 32.
/// </summary>
[JccNearImm(32, "int", "state.ZeroFlag || state.SignFlag != state.OverflowFlag", "jle")]
public partial class Jle32;

/// <summary>
/// Represents jg 32.
/// </summary>
[JccNearImm(32, "int", "!state.ZeroFlag && state.SignFlag == state.OverflowFlag", "jg")]
public partial class Jg32;
/// <summary>
/// Represents jcxz 16.
/// </summary>
[JccNearImm(8, "sbyte", "state.CX == 0", "jcxz")]
public partial class Jcxz16;
// JCXZ32 still uses an 8bit offset
/// <summary>
/// Represents jcxz 32.
/// </summary>
[JccNearImm(8, "sbyte", "state.ECX == 0", "jcxz")]
public partial class Jcxz32;