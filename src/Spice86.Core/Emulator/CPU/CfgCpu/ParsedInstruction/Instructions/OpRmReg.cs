namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

// ADC
/// <summary>
/// Represents adc rm reg 8.
/// </summary>
[OpRmReg("Adc", 8)]
public partial class AdcRmReg8;

/// <summary>
/// Represents adc rm reg 16.
/// </summary>
[OpRmReg("Adc", 16)]
public partial class AdcRmReg16;

/// <summary>
/// Represents adc rm reg 32.
/// </summary>
[OpRmReg("Adc", 32)]
public partial class AdcRmReg32;

// ADD
/// <summary>
/// Represents add rm reg 8.
/// </summary>
[OpRmReg("Add", 8)]
public partial class AddRmReg8;

/// <summary>
/// Represents add rm reg 16.
/// </summary>
[OpRmReg("Add", 16)]
public partial class AddRmReg16;

/// <summary>
/// Represents add rm reg 32.
/// </summary>
[OpRmReg("Add", 32)]
public partial class AddRmReg32;

// AND
/// <summary>
/// Represents and rm reg 8.
/// </summary>
[OpRmReg("And", 8)]
public partial class AndRmReg8;

/// <summary>
/// Represents and rm reg 16.
/// </summary>
[OpRmReg("And", 16)]
public partial class AndRmReg16;

/// <summary>
/// Represents and rm reg 32.
/// </summary>
[OpRmReg("And", 32)]
public partial class AndRmReg32;

// OR
/// <summary>
/// Represents or rm reg 8.
/// </summary>
[OpRmReg("Or", 8)]
public partial class OrRmReg8;

/// <summary>
/// Represents or rm reg 16.
/// </summary>
[OpRmReg("Or", 16)]
public partial class OrRmReg16;

/// <summary>
/// Represents or rm reg 32.
/// </summary>
[OpRmReg("Or", 32)]
public partial class OrRmReg32;

// SBB
/// <summary>
/// Represents sbb rm reg 8.
/// </summary>
[OpRmReg("Sbb", 8)]
public partial class SbbRmReg8;

/// <summary>
/// Represents sbb rm reg 16.
/// </summary>
[OpRmReg("Sbb", 16)]
public partial class SbbRmReg16;

/// <summary>
/// Represents sbb rm reg 32.
/// </summary>
[OpRmReg("Sbb", 32)]
public partial class SbbRmReg32;

// SUB
/// <summary>
/// Represents sub rm reg 8.
/// </summary>
[OpRmReg("Sub", 8)]
public partial class SubRmReg8;

/// <summary>
/// Represents sub rm reg 16.
/// </summary>
[OpRmReg("Sub", 16)]
public partial class SubRmReg16;

/// <summary>
/// Represents sub rm reg 32.
/// </summary>
[OpRmReg("Sub", 32)]
public partial class SubRmReg32;

// XOR
/// <summary>
/// Represents xor rm reg 8.
/// </summary>
[OpRmReg("Xor", 8)]
public partial class XorRmReg8;

/// <summary>
/// Represents xor rm reg 16.
/// </summary>
[OpRmReg("Xor", 16)]
public partial class XorRmReg16;

/// <summary>
/// Represents xor rm reg 32.
/// </summary>
[OpRmReg("Xor", 32)]
public partial class XorRmReg32;


// CMP (Sub without assigment)
/// <summary>
/// Represents cmp rm reg 8.
/// </summary>
[OpRmReg("Sub", 8, false, "cmp")]
public partial class CmpRmReg8;

/// <summary>
/// Represents cmp rm reg 16.
/// </summary>
[OpRmReg("Sub", 16, false, "cmp")]
public partial class CmpRmReg16;

/// <summary>
/// Represents cmp rm reg 32.
/// </summary>
[OpRmReg("Sub", 32, false, "cmp")]
public partial class CmpRmReg32;

// TEST (And without assigment)
/// <summary>
/// Represents test rm reg 8.
/// </summary>
[OpRmReg("And", 8, false)]
public partial class TestRmReg8;

/// <summary>
/// Represents test rm reg 16.
/// </summary>
[OpRmReg("And", 16, false)]
public partial class TestRmReg16;

/// <summary>
/// Represents test rm reg 32.
/// </summary>
[OpRmReg("And", 32, false)]
public partial class TestRmReg32;