namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

// ADC
/// <summary>
/// Represents grp 1 adc 8.
/// </summary>
[Grp1("Adc", 8, "byte", Mnemonic: "ADC")]
public partial class Grp1Adc8;

/// <summary>
/// Represents grp 1 adc signed 16.
/// </summary>
[Grp1("Adc", 16, "sbyte", "(ushort)", Mnemonic: "ADC")]
public partial class Grp1AdcSigned16;

/// <summary>
/// Represents grp 1 adc signed 32.
/// </summary>
[Grp1("Adc", 32, "sbyte", "(uint)", Mnemonic: "ADC")]
public partial class Grp1AdcSigned32;

/// <summary>
/// Represents grp 1 adc unsigned 16.
/// </summary>
[Grp1("Adc", 16, "ushort", Mnemonic: "ADC")]
public partial class Grp1AdcUnsigned16;

/// <summary>
/// Represents grp 1 adc unsigned 32.
/// </summary>
[Grp1("Adc", 32, "uint", Mnemonic: "ADC")]
public partial class Grp1AdcUnsigned32;

// ADD
/// <summary>
/// Represents grp 1 add 8.
/// </summary>
[Grp1("Add", 8, "byte", Mnemonic: "ADD")]
public partial class Grp1Add8;

/// <summary>
/// Represents grp 1 add signed 16.
/// </summary>
[Grp1("Add", 16, "sbyte", "(ushort)", Mnemonic: "ADD")]
public partial class Grp1AddSigned16;

/// <summary>
/// Represents grp 1 add signed 32.
/// </summary>
[Grp1("Add", 32, "sbyte", "(uint)", Mnemonic: "ADD")]
public partial class Grp1AddSigned32;

/// <summary>
/// Represents grp 1 add unsigned 16.
/// </summary>
[Grp1("Add", 16, "ushort", Mnemonic: "ADD")]
public partial class Grp1AddUnsigned16;

/// <summary>
/// Represents grp 1 add unsigned 32.
/// </summary>
[Grp1("Add", 32, "uint", Mnemonic: "ADD")]
public partial class Grp1AddUnsigned32;

// AND
/// <summary>
/// Represents grp 1 and 8.
/// </summary>
[Grp1("And", 8, "byte", Mnemonic: "AND")]
public partial class Grp1And8;

/// <summary>
/// Represents grp 1 and signed 16.
/// </summary>
[Grp1("And", 16, "sbyte", "(ushort)", Mnemonic: "AND")]
public partial class Grp1AndSigned16;

/// <summary>
/// Represents grp 1 and signed 32.
/// </summary>
[Grp1("And", 32, "sbyte", "(uint)", Mnemonic: "AND")]
public partial class Grp1AndSigned32;

/// <summary>
/// Represents grp 1 and unsigned 16.
/// </summary>
[Grp1("And", 16, "ushort", Mnemonic: "AND")]
public partial class Grp1AndUnsigned16;

/// <summary>
/// Represents grp 1 and unsigned 32.
/// </summary>
[Grp1("And", 32, "uint", Mnemonic: "AND")]
public partial class Grp1AndUnsigned32;

// CMP (Sub without assigment)
/// <summary>
/// Represents grp 1 cmp 8.
/// </summary>
[Grp1("Sub", 8, "byte", "", false, Mnemonic: "CMP")]
public partial class Grp1Cmp8;

/// <summary>
/// Represents grp 1 cmp signed 16.
/// </summary>
[Grp1("Sub", 16, "sbyte", "(ushort)", false, Mnemonic: "CMP")]
public partial class Grp1CmpSigned16;

/// <summary>
/// Represents grp 1 cmp signed 32.
/// </summary>
[Grp1("Sub", 32, "sbyte", "(uint)", false, Mnemonic: "CMP")]
public partial class Grp1CmpSigned32;

/// <summary>
/// Represents grp 1 cmp unsigned 16.
/// </summary>
[Grp1("Sub", 16, "ushort", "", false, Mnemonic: "CMP")]
public partial class Grp1CmpUnsigned16;

/// <summary>
/// Represents grp 1 cmp unsigned 32.
/// </summary>
[Grp1("Sub", 32, "uint", "", false, Mnemonic: "CMP")]
public partial class Grp1CmpUnsigned32;

// OR
/// <summary>
/// Represents grp 1 or 8.
/// </summary>
[Grp1("Or", 8, "byte", Mnemonic: "OR")]
public partial class Grp1Or8;

/// <summary>
/// Represents grp 1 or signed 16.
/// </summary>
[Grp1("Or", 16, "sbyte", "(ushort)", Mnemonic: "OR")]
public partial class Grp1OrSigned16;

/// <summary>
/// Represents grp 1 or signed 32.
/// </summary>
[Grp1("Or", 32, "sbyte", "(uint)", Mnemonic: "OR")]
public partial class Grp1OrSigned32;

/// <summary>
/// Represents grp 1 or unsigned 16.
/// </summary>
[Grp1("Or", 16, "ushort", Mnemonic: "OR")]
public partial class Grp1OrUnsigned16;

/// <summary>
/// Represents grp 1 or unsigned 32.
/// </summary>
[Grp1("Or", 32, "uint", Mnemonic: "OR")]
public partial class Grp1OrUnsigned32;

// SBB
/// <summary>
/// Represents grp 1 sbb 8.
/// </summary>
[Grp1("Sbb", 8, "byte", Mnemonic: "SBB")]
public partial class Grp1Sbb8;

/// <summary>
/// Represents grp 1 sbb signed 16.
/// </summary>
[Grp1("Sbb", 16, "sbyte", "(ushort)", Mnemonic: "SBB")]
public partial class Grp1SbbSigned16;

/// <summary>
/// Represents grp 1 sbb signed 32.
/// </summary>
[Grp1("Sbb", 32, "sbyte", "(uint)", Mnemonic: "SBB")]
public partial class Grp1SbbSigned32;

/// <summary>
/// Represents grp 1 sbb unsigned 16.
/// </summary>
[Grp1("Sbb", 16, "ushort", Mnemonic: "SBB")]
public partial class Grp1SbbUnsigned16;

/// <summary>
/// Represents grp 1 sbb unsigned 32.
/// </summary>
[Grp1("Sbb", 32, "uint", Mnemonic: "SBB")]
public partial class Grp1SbbUnsigned32;

// SUB
/// <summary>
/// Represents grp 1 sub 8.
/// </summary>
[Grp1("Sub", 8, "byte", Mnemonic: "SUB")]
public partial class Grp1Sub8;

/// <summary>
/// Represents grp 1 sub signed 16.
/// </summary>
[Grp1("Sub", 16, "sbyte", "(ushort)", Mnemonic: "SUB")]
public partial class Grp1SubSigned16;

/// <summary>
/// Represents grp 1 sub signed 32.
/// </summary>
[Grp1("Sub", 32, "sbyte", "(uint)", Mnemonic: "SUB")]
public partial class Grp1SubSigned32;

/// <summary>
/// Represents grp 1 sub unsigned 16.
/// </summary>
[Grp1("Sub", 16, "ushort", Mnemonic: "SUB")]
public partial class Grp1SubUnsigned16;

/// <summary>
/// Represents grp 1 sub unsigned 32.
/// </summary>
[Grp1("Sub", 32, "uint", Mnemonic: "SUB")]
public partial class Grp1SubUnsigned32;

// XOR
/// <summary>
/// Represents grp 1 xor 8.
/// </summary>
[Grp1("Xor", 8, "byte", Mnemonic: "XOR")]
public partial class Grp1Xor8;

/// <summary>
/// Represents grp 1 xor signed 16.
/// </summary>
[Grp1("Xor", 16, "sbyte", "(ushort)", Mnemonic: "XOR")]
public partial class Grp1XorSigned16;

/// <summary>
/// Represents grp 1 xor signed 32.
/// </summary>
[Grp1("Xor", 32, "sbyte", "(uint)", Mnemonic: "XOR")]
public partial class Grp1XorSigned32;

/// <summary>
/// Represents grp 1 xor unsigned 16.
/// </summary>
[Grp1("Xor", 16, "ushort", Mnemonic: "XOR")]
public partial class Grp1XorUnsigned16;

/// <summary>
/// Represents grp 1 xor unsigned 32.
/// </summary>
[Grp1("Xor", 32, "uint", Mnemonic: "XOR")]
public partial class Grp1XorUnsigned32;