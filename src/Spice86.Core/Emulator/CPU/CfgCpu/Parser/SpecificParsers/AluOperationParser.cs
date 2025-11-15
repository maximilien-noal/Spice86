namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.SpecificParsers;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.ModRm;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Parses memory bytes into several variants of one Alu operation.
/// For one Alu operation, there are several instructions that can:
///  - perform the operation in 8, 16 or 32 bits
///  - perform the operation with ModRm writing to Reg or RegMem
///  - perform the operation with the accumulator (AL / AX / EAX) and an immediate value
/// Patterns in opcode:
/// xxxxx000 rm reg 8
/// xxxxx001 rm reg 16/32
/// xxxxx010 reg rm 8
/// xxxxx011 reg rm 16/32
/// xxxxx100 acc imm 8
/// xxxxx101 acc imm 16/32
/// </summary>

public abstract class AluOperationParser : BaseInstructionParser {
    private const byte ModRmMask = 0b100;
    private const byte RmRegDirectionMask = 0b10;

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="other">The other.</param>
    public AluOperationParser(BaseInstructionParser other) : base(other) {
    }

    /// <summary>
    /// Parses .
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>The result of the operation.</returns>
    public CfgInstruction Parse(ParsingContext context) {
        ushort opcode = context.OpcodeField.Value;
        bool hasModRm = (opcode & ModRmMask) == 0;
        BitWidth bitWidth = GetBitWidth(context.OpcodeField, context.HasOperandSize32);

        if (hasModRm) {
            ModRmContext modRmContext = _modRmParser.ParseNext(context);
            bool rmReg = (opcode & RmRegDirectionMask) == 0;
            if (rmReg) {
                return ParseRmReg(context, bitWidth, modRmContext);
            }
            return ParseRegRm(context, bitWidth, modRmContext);
        }
        return ParseAccImm(context, bitWidth);
    }

    protected abstract CfgInstruction ParseAccImm(ParsingContext context, BitWidth bitWidth);

    protected abstract CfgInstruction ParseRegRm(ParsingContext context, BitWidth bitWidth, ModRmContext modRmContext);

    protected abstract CfgInstruction ParseRmReg(ParsingContext context, BitWidth bitWidth, ModRmContext modRmContext);
}

/// <summary>
/// Represents add alu operation parser.
/// </summary>
[AluOperationParser("Add")]
public partial class AddAluOperationParser;
/// <summary>
/// Represents or alu operation parser.
/// </summary>
[AluOperationParser("Or")]
public partial class OrAluOperationParser;
/// <summary>
/// Represents adc alu operation parser.
/// </summary>
[AluOperationParser("Adc")]
public partial class AdcAluOperationParser;
/// <summary>
/// Represents sbb alu operation parser.
/// </summary>
[AluOperationParser("Sbb")]
public partial class SbbAluOperationParser;
/// <summary>
/// Represents and alu operation parser.
/// </summary>
[AluOperationParser("And")]
public partial class AndAluOperationParser;
/// <summary>
/// Represents sub alu operation parser.
/// </summary>
[AluOperationParser("Sub")]
public partial class SubAluOperationParser;
/// <summary>
/// Represents xor alu operation parser.
/// </summary>
[AluOperationParser("Xor")]
public partial class XorAluOperationParser;
/// <summary>
/// Represents cmp alu operation parser.
/// </summary>
[AluOperationParser("Cmp")]
public partial class CmpAluOperationParser;