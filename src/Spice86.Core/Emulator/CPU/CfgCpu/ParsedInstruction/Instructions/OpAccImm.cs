namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

// ADC
/// <summary>
/// Represents adc acc imm 8.
/// </summary>
[OpAccImm("Adc", "AL", 8, "byte")]
public partial class AdcAccImm8;

/// <summary>
/// Represents adc acc imm 16.
/// </summary>
[OpAccImm("Adc", "AX", 16, "ushort")]
public partial class AdcAccImm16;

/// <summary>
/// Represents adc acc imm 32.
/// </summary>
[OpAccImm("Adc", "EAX", 32, "uint")]
public partial class AdcAccImm32;

// ADD
/// <summary>
/// Represents add acc imm 8.
/// </summary>
[OpAccImm("Add", "AL", 8, "byte")]
public partial class AddAccImm8;

/// <summary>
/// Represents add acc imm 16.
/// </summary>
[OpAccImm("Add", "AX", 16, "ushort")]
public partial class AddAccImm16;

/// <summary>
/// Represents add acc imm 32.
/// </summary>
[OpAccImm("Add", "EAX", 32, "uint")]
public partial class AddAccImm32;

// AND
/// <summary>
/// Represents and acc imm 8.
/// </summary>
[OpAccImm("And", "AL", 8, "byte")]
public partial class AndAccImm8;

/// <summary>
/// Represents and acc imm 16.
/// </summary>
[OpAccImm("And", "AX", 16, "ushort")]
public partial class AndAccImm16;

/// <summary>
/// Represents and acc imm 32.
/// </summary>
[OpAccImm("And", "EAX", 32, "uint")]
public partial class AndAccImm32;

// OR
/// <summary>
/// Represents or acc imm 8.
/// </summary>
[OpAccImm("Or", "AL", 8, "byte")]
public partial class OrAccImm8;

/// <summary>
/// Represents or acc imm 16.
/// </summary>
[OpAccImm("Or", "AX", 16, "ushort")]
public partial class OrAccImm16;

/// <summary>
/// Represents or acc imm 32.
/// </summary>
[OpAccImm("Or", "EAX", 32, "uint")]
public partial class OrAccImm32;

// SBB
/// <summary>
/// Represents sbb acc imm 8.
/// </summary>
[OpAccImm("Sbb", "AL", 8, "byte")]
public partial class SbbAccImm8;

/// <summary>
/// Represents sbb acc imm 16.
/// </summary>
[OpAccImm("Sbb", "AX", 16, "ushort")]
public partial class SbbAccImm16;

/// <summary>
/// Represents sbb acc imm 32.
/// </summary>
[OpAccImm("Sbb", "EAX", 32, "uint")]
public partial class SbbAccImm32;

// SUB
/// <summary>
/// Represents sub acc imm 8.
/// </summary>
[OpAccImm("Sub", "AL", 8, "byte")]
public partial class SubAccImm8;

/// <summary>
/// Represents sub acc imm 16.
/// </summary>
[OpAccImm("Sub", "AX", 16, "ushort")]
public partial class SubAccImm16;

/// <summary>
/// Represents sub acc imm 32.
/// </summary>
[OpAccImm("Sub", "EAX", 32, "uint")]
public partial class SubAccImm32;

// XOR
/// <summary>
/// Represents xor acc imm 8.
/// </summary>
[OpAccImm("Xor", "AL", 8, "byte")]
public partial class XorAccImm8;

/// <summary>
/// Represents xor acc imm 16.
/// </summary>
[OpAccImm("Xor", "AX", 16, "ushort")]
public partial class XorAccImm16;

/// <summary>
/// Represents xor acc imm 32.
/// </summary>
[OpAccImm("Xor", "EAX", 32, "uint")]
public partial class XorAccImm32;

// CMP (Sub without assigment)
/// <summary>
/// Represents cmp acc imm 8.
/// </summary>
[OpAccImm("Sub", "AL", 8, "byte", false, "cmp")]
public partial class CmpAccImm8;

/// <summary>
/// Represents cmp acc imm 16.
/// </summary>
[OpAccImm("Sub", "AX", 16, "ushort", false, "cmp")]
public partial class CmpAccImm16;

/// <summary>
/// Represents cmp acc imm 32.
/// </summary>
[OpAccImm("Sub", "EAX", 32, "uint", false, "cmp")]
public partial class CmpAccImm32;


// Test (Sub without assigment)
/// <summary>
/// Represents test acc imm 8.
/// </summary>
[OpAccImm("And", "AL", 8, "byte", false, "test")]
public partial class TestAccImm8;

/// <summary>
/// Represents test acc imm 16.
/// </summary>
[OpAccImm("And", "AX", 16, "ushort", false, "test")]
public partial class TestAccImm16;

/// <summary>
/// Represents test acc imm 32.
/// </summary>
[OpAccImm("And", "EAX", 32, "uint", false, "test")]
public partial class TestAccImm32;