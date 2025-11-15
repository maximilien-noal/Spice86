namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

// ADC
/// <summary>
/// Represents adc reg rm 8.
/// </summary>
[OpRegRm("Adc", 8)]
public partial class AdcRegRm8;

/// <summary>
/// Represents adc reg rm 16.
/// </summary>
[OpRegRm("Adc", 16)]
public partial class AdcRegRm16;

/// <summary>
/// Represents adc reg rm 32.
/// </summary>
[OpRegRm("Adc", 32)]
public partial class AdcRegRm32;

// ADD
/// <summary>
/// Represents add reg rm 8.
/// </summary>
[OpRegRm("Add", 8)]
public partial class AddRegRm8;

/// <summary>
/// Represents add reg rm 16.
/// </summary>
[OpRegRm("Add", 16)]
public partial class AddRegRm16;

/// <summary>
/// Represents add reg rm 32.
/// </summary>
[OpRegRm("Add", 32)]
public partial class AddRegRm32;

// AND
/// <summary>
/// Represents and reg rm 8.
/// </summary>
[OpRegRm("And", 8)]
public partial class AndRegRm8;

/// <summary>
/// Represents and reg rm 16.
/// </summary>
[OpRegRm("And", 16)]
public partial class AndRegRm16;

/// <summary>
/// Represents and reg rm 32.
/// </summary>
[OpRegRm("And", 32)]
public partial class AndRegRm32;

// CMP (Sub without assigment)
/// <summary>
/// Represents cmp reg rm 8.
/// </summary>
[OpRegRm("Sub", 8, false, "cmp")]
public partial class CmpRegRm8;

/// <summary>
/// Represents cmp reg rm 16.
/// </summary>
[OpRegRm("Sub", 16, false, "cmp")]
public partial class CmpRegRm16;

/// <summary>
/// Represents cmp reg rm 32.
/// </summary>
[OpRegRm("Sub", 32, false, "cmp")]
public partial class CmpRegRm32;

// OR
/// <summary>
/// Represents or reg rm 8.
/// </summary>
[OpRegRm("Or", 8)]
public partial class OrRegRm8;

/// <summary>
/// Represents or reg rm 16.
/// </summary>
[OpRegRm("Or", 16)]
public partial class OrRegRm16;

/// <summary>
/// Represents or reg rm 32.
/// </summary>
[OpRegRm("Or", 32)]
public partial class OrRegRm32;

// SBB
/// <summary>
/// Represents sbb reg rm 8.
/// </summary>
[OpRegRm("Sbb", 8)]
public partial class SbbRegRm8;

/// <summary>
/// Represents sbb reg rm 16.
/// </summary>
[OpRegRm("Sbb", 16)]
public partial class SbbRegRm16;

/// <summary>
/// Represents sbb reg rm 32.
/// </summary>
[OpRegRm("Sbb", 32)]
public partial class SbbRegRm32;

// SUB
/// <summary>
/// Represents sub reg rm 8.
/// </summary>
[OpRegRm("Sub", 8)]
public partial class SubRegRm8;

/// <summary>
/// Represents sub reg rm 16.
/// </summary>
[OpRegRm("Sub", 16)]
public partial class SubRegRm16;

/// <summary>
/// Represents sub reg rm 32.
/// </summary>
[OpRegRm("Sub", 32)]
public partial class SubRegRm32;

// XOR
/// <summary>
/// Represents xor reg rm 8.
/// </summary>
[OpRegRm("Xor", 8)]
public partial class XorRegRm8;

/// <summary>
/// Represents xor reg rm 16.
/// </summary>
[OpRegRm("Xor", 16)]
public partial class XorRegRm16;

/// <summary>
/// Represents xor reg rm 32.
/// </summary>
[OpRegRm("Xor", 32)]
public partial class XorRegRm32;