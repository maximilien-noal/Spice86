namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;
/// <summary>
/// Represents imul imm 8 rm 16.
/// </summary>
[ImulImmRm(Size: 16, RmSignedType: "short", RmUnsignedType: "ushort", ImmSignedType: "sbyte", ResSignedType: "int")]
public partial class ImulImm8Rm16;
/// <summary>
/// Represents imul imm 8 rm 32.
/// </summary>
[ImulImmRm(Size: 32, RmSignedType: "int", RmUnsignedType: "uint", ImmSignedType: "sbyte", ResSignedType: "long")]
public partial class ImulImm8Rm32;
/// <summary>
/// Represents imul imm rm 16.
/// </summary>
[ImulImmRm(Size: 16, RmSignedType: "short", RmUnsignedType: "ushort", ImmSignedType: "short", ResSignedType: "int")]
public partial class ImulImmRm16;
/// <summary>
/// Represents imul imm rm 32.
/// </summary>
[ImulImmRm(Size: 32, RmSignedType: "int", RmUnsignedType: "uint", ImmSignedType: "int", ResSignedType: "long")]
public partial class ImulImmRm32;
/// <summary>
/// Represents imul rm 16.
/// </summary>
[ImulRm(Size: 16, RmSignedType: "short", RmUnsignedType: "ushort", ResSignedType: "int")]
public partial class ImulRm16;
/// <summary>
/// Represents imul rm 32.
/// </summary>
[ImulRm(Size: 32, RmSignedType: "int", RmUnsignedType: "uint", ResSignedType: "long")]
public partial class ImulRm32;