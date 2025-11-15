namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.SpecificParsers;

/// <summary>
/// Represents shld imm 8 rm parser.
/// </summary>
[OperationModRmImmParser(Operation: "ShldImm8Rm", Has8: false, IsOnlyField8: true, IsUnsignedField: true)]
public partial class ShldImm8RmParser;

/// <summary>
/// Represents shrd imm 8 rm parser.
/// </summary>
[OperationModRmImmParser(Operation: "ShrdImm8Rm", Has8: false, IsOnlyField8: true, IsUnsignedField: true)]
public partial class ShrdImm8RmParser;

/// <summary>
/// Represents mov rm imm parser.
/// </summary>
[OperationModRmImmParser(Operation: "MovRmImm", Has8: true, IsOnlyField8: false, IsUnsignedField: true)]
public partial class MovRmImmParser;

/// <summary>
/// Represents imul imm rm parser.
/// </summary>
[OperationModRmImmParser(Operation: "ImulImmRm", Has8: false, IsOnlyField8: false, IsUnsignedField: false)]
public partial class ImulImmRmParser;
/// <summary>
/// Represents imul imm 8 rm parser.
/// </summary>
[OperationModRmImmParser(Operation: "ImulImm8Rm", Has8: false, IsOnlyField8: true, IsUnsignedField: false)]
public partial class ImulImm8RmParser;