namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.SpecificParsers;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.ModRm;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents grp 2 parser.
/// </summary>
public class Grp2Parser : BaseGrpOperationParser {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="instructionParser">The instruction parser.</param>
    public Grp2Parser(BaseInstructionParser instructionParser) : base(instructionParser) {
    }

    /// <summary>
    /// Parses .
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="modRmContext">The mod rm context.</param>
    /// <param name="groupIndex">The group index.</param>
    /// <returns>The result of the operation.</returns>
    protected override CfgInstruction Parse(ParsingContext context, ModRmContext modRmContext, int groupIndex) {
        ushort opcode = context.OpcodeField.Value;
        bool useImm = !BitIsTrue(opcode, 4);
        BitWidth bitWidth = GetBitWidth(context.OpcodeField, context.HasOperandSize32);
        if (useImm) {
            return GetOperationRmImmFactory(groupIndex).Parse(context, modRmContext, bitWidth);
        }
        bool useCl = BitIsTrue(opcode, 1);
        if (useCl) {
            return GetOperationClFactory(groupIndex).Parse(context, modRmContext, bitWidth);
        }

        return GetOperationOneFactory(groupIndex).Parse(context, modRmContext, bitWidth);
    }

    /// <summary>
    /// Gets operation one factory.
    /// </summary>
    /// <param name="groupIndex">The group index.</param>
    /// <returns>The result of the operation.</returns>
    protected BaseOperationModRmFactory GetOperationOneFactory(int groupIndex) {
        return groupIndex switch {
            0 => new Grp2RolOneRmOperationFactory(this),
            1 => new Grp2RorOneRmOperationFactory(this),
            2 => new Grp2RclOneRmOperationFactory(this),
            3 => new Grp2RcrOneRmOperationFactory(this),
            4 => new Grp2ShlOneRmOperationFactory(this),
            5 => new Grp2ShrOneRmOperationFactory(this),
            7 => new Grp2SarOneRmOperationFactory(this),
            _ => throw new InvalidGroupIndexException(_state, groupIndex)
        };
    }
    /// <summary>
    /// Gets operation cl factory.
    /// </summary>
    /// <param name="groupIndex">The group index.</param>
    /// <returns>The result of the operation.</returns>
    protected BaseOperationModRmFactory GetOperationClFactory(int groupIndex) {
        return groupIndex switch {
            0 => new Grp2RolClRmOperationFactory(this),
            1 => new Grp2RorClRmOperationFactory(this),
            2 => new Grp2RclClRmOperationFactory(this),
            3 => new Grp2RcrClRmOperationFactory(this),
            4 => new Grp2ShlClRmOperationFactory(this),
            5 => new Grp2ShrClRmOperationFactory(this),
            7 => new Grp2SarClRmOperationFactory(this),
            _ => throw new InvalidGroupIndexException(_state, groupIndex)
        };
    }

    /// <summary>
    /// Gets operation rm imm factory.
    /// </summary>
    /// <param name="groupIndex">The group index.</param>
    /// <returns>The result of the operation.</returns>
    protected BaseOperationModRmFactory GetOperationRmImmFactory(int groupIndex) {
        return groupIndex switch {
            0 => new Grp2RolRmImmOperationFactory(this),
            1 => new Grp2RorRmImmOperationFactory(this),
            2 => new Grp2RclRmImmOperationFactory(this),
            3 => new Grp2RcrRmImmOperationFactory(this),
            4 => new Grp2ShlRmImmOperationFactory(this),
            5 => new Grp2ShrRmImmOperationFactory(this),
            7 => new Grp2SarRmImmOperationFactory(this),
            _ => throw new InvalidGroupIndexException(_state, groupIndex)
        };
    }
}

/// <summary>
/// Represents grp 2 rol one rm operation factory.
/// </summary>
[OperationModRmFactory("Grp2RolOneRm")]
public partial class Grp2RolOneRmOperationFactory;
/// <summary>
/// Represents grp 2 ror one rm operation factory.
/// </summary>
[OperationModRmFactory("Grp2RorOneRm")]
public partial class Grp2RorOneRmOperationFactory;
/// <summary>
/// Represents grp 2 rcl one rm operation factory.
/// </summary>
[OperationModRmFactory("Grp2RclOneRm")]
public partial class Grp2RclOneRmOperationFactory;
/// <summary>
/// Represents grp 2 rcr one rm operation factory.
/// </summary>
[OperationModRmFactory("Grp2RcrOneRm")]
public partial class Grp2RcrOneRmOperationFactory;
/// <summary>
/// Represents grp 2 shl one rm operation factory.
/// </summary>
[OperationModRmFactory("Grp2ShlOneRm")]
public partial class Grp2ShlOneRmOperationFactory;
/// <summary>
/// Represents grp 2 shr one rm operation factory.
/// </summary>
[OperationModRmFactory("Grp2ShrOneRm")]
public partial class Grp2ShrOneRmOperationFactory;
/// <summary>
/// Represents grp 2 sar one rm operation factory.
/// </summary>
[OperationModRmFactory("Grp2SarOneRm")]
public partial class Grp2SarOneRmOperationFactory;

/// <summary>
/// Represents grp 2 rol cl rm operation factory.
/// </summary>
[OperationModRmFactory("Grp2RolClRm")]
public partial class Grp2RolClRmOperationFactory;
/// <summary>
/// Represents grp 2 ror cl rm operation factory.
/// </summary>
[OperationModRmFactory("Grp2RorClRm")]
public partial class Grp2RorClRmOperationFactory;
/// <summary>
/// Represents grp 2 rcl cl rm operation factory.
/// </summary>
[OperationModRmFactory("Grp2RclClRm")]
public partial class Grp2RclClRmOperationFactory;
/// <summary>
/// Represents grp 2 rcr cl rm operation factory.
/// </summary>
[OperationModRmFactory("Grp2RcrClRm")]
public partial class Grp2RcrClRmOperationFactory;
/// <summary>
/// Represents grp 2 shl cl rm operation factory.
/// </summary>
[OperationModRmFactory("Grp2ShlClRm")]
public partial class Grp2ShlClRmOperationFactory;
/// <summary>
/// Represents grp 2 shr cl rm operation factory.
/// </summary>
[OperationModRmFactory("Grp2ShrClRm")]
public partial class Grp2ShrClRmOperationFactory;
/// <summary>
/// Represents grp 2 sar cl rm operation factory.
/// </summary>
[OperationModRmFactory("Grp2SarClRm")]
public partial class Grp2SarClRmOperationFactory;

/// <summary>
/// Represents grp 2 rol rm imm operation factory.
/// </summary>
[OperationModRmImmFactory("Grp2RolRmImm", true)]
public partial class Grp2RolRmImmOperationFactory;
/// <summary>
/// Represents grp 2 ror rm imm operation factory.
/// </summary>
[OperationModRmImmFactory("Grp2RorRmImm", true)]
public partial class Grp2RorRmImmOperationFactory;
/// <summary>
/// Represents grp 2 rcl rm imm operation factory.
/// </summary>
[OperationModRmImmFactory("Grp2RclRmImm", true)]
public partial class Grp2RclRmImmOperationFactory;
/// <summary>
/// Represents grp 2 rcr rm imm operation factory.
/// </summary>
[OperationModRmImmFactory("Grp2RcrRmImm", true)]
public partial class Grp2RcrRmImmOperationFactory;
/// <summary>
/// Represents grp 2 shl rm imm operation factory.
/// </summary>
[OperationModRmImmFactory("Grp2ShlRmImm", true)]
public partial class Grp2ShlRmImmOperationFactory;
/// <summary>
/// Represents grp 2 shr rm imm operation factory.
/// </summary>
[OperationModRmImmFactory("Grp2ShrRmImm", true)]
public partial class Grp2ShrRmImmOperationFactory;
/// <summary>
/// Represents grp 2 sar rm imm operation factory.
/// </summary>
[OperationModRmImmFactory("Grp2SarRmImm", true)]
public partial class Grp2SarRmImmOperationFactory;