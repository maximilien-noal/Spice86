namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast.Builder;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Instruction;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.Interfaces;

/// <summary>
/// Represents ast builder.
/// </summary>
public class AstBuilder {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public AstBuilder() {
        InstructionField = new(Constant, Pointer);
        ModRm = new(Register, InstructionField, Pointer);
    }

    /// <summary>
    /// Gets register.
    /// </summary>
    public RegisterAstBuilder Register { get; } = new();
    /// <summary>
    /// Gets pointer.
    /// </summary>
    public PointerAstBuilder Pointer { get; } = new();
    /// <summary>
    /// Gets constant.
    /// </summary>
    public ConstantAstBuilder Constant { get; } = new();
    /// <summary>
    /// Gets instruction field.
    /// </summary>
    public InstructionFieldAstBuilder InstructionField { get; }
    /// <summary>
    /// Gets mod rm.
    /// </summary>
    public ModRmAstBuilder ModRm { get; }

    /// <summary>
    /// Performs the s type operation.
    /// </summary>
    /// <param name="size">The size.</param>
    /// <returns>The result of the operation.</returns>
    public DataType SType(int size) {
        return Type(size, true);
    }
    /// <summary>
    /// Performs the u type operation.
    /// </summary>
    /// <param name="size">The size.</param>
    /// <returns>The result of the operation.</returns>
    public DataType UType(int size) {
        return Type(size, false);
    }

    private DataType Type(int size, bool isSigned) {
        return size switch {
            8 => isSigned ? DataType.INT8 : DataType.UINT8,
            16 => isSigned ? DataType.INT16 : DataType.UINT16,
            32 => isSigned ? DataType.INT32 : DataType.UINT32,
            _ => throw new ArgumentOutOfRangeException(nameof(size), size, "value not handled")
        };
    }

    /// <summary>
    /// Adds ress type.
    /// </summary>
    /// <param name="instruction">The instruction.</param>
    /// <returns>The result of the operation.</returns>
    public DataType AddressType(CfgInstruction instruction) {
        return instruction.AddressSize32Prefix == null ? DataType.UINT16 : DataType.UINT32;
    }

    /// <summary>
    /// Performs the rep operation.
    /// </summary>
    /// <param name="instruction">The instruction.</param>
    public RepPrefix? Rep(StringInstruction instruction) {
        if (instruction.RepPrefix is null) {
            return null;
        }
        if (!instruction.ChangesFlags) {
            return RepPrefix.REP;
        }
        if (instruction.RepPrefix.ContinueZeroFlagValue) {
            return RepPrefix.REPE;
        }
        return RepPrefix.REPNE;
    }

}