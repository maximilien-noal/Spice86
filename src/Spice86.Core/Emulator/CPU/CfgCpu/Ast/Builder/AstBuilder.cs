namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast.Builder;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Instruction;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.Interfaces;

/// <summary>
/// Represents the AstBuilder class.
/// </summary>
public class AstBuilder {
    public AstBuilder() {
        InstructionField = new(Constant, Pointer);
        ModRm = new(Register, InstructionField, Pointer);
    }

    /// <summary>
    /// Register method.
    /// </summary>
    public RegisterAstBuilder Register { get; } = new();
    /// <summary>
    /// Pointer method.
    /// </summary>
    public PointerAstBuilder Pointer { get; } = new();
    /// <summary>
    /// Constant method.
    /// </summary>
    public ConstantAstBuilder Constant { get; } = new();
    /// <summary>
    /// Gets or sets the InstructionField.
    /// </summary>
    public InstructionFieldAstBuilder InstructionField { get; }
    /// <summary>
    /// Gets or sets the ModRm.
    /// </summary>
    public ModRmAstBuilder ModRm { get; }

    /// <summary>
    /// SType method.
    /// </summary>
    public DataType SType(int size) {
        return Type(size, true);
    }
    /// <summary>
    /// UType method.
    /// </summary>
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
    /// AddressType method.
    /// </summary>
    public DataType AddressType(CfgInstruction instruction) {
        return instruction.AddressSize32Prefix == null ? DataType.UINT16 : DataType.UINT32;
    }

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