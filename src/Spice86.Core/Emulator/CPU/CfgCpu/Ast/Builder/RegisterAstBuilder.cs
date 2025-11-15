namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast.Builder;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value;
using Spice86.Core.Emulator.CPU.Registers;

/// <summary>
/// Represents register ast builder.
/// </summary>
public class RegisterAstBuilder {
    /// <summary>
    /// Performs the reg 8 operation.
    /// </summary>
    /// <param name="registerIndex">The register index.</param>
    /// <returns>The result of the operation.</returns>
    public ValueNode Reg8(RegisterIndex registerIndex) {
        return Reg(DataType.UINT8, registerIndex);
    }

    /// <summary>
    /// Performs the reg 16 operation.
    /// </summary>
    /// <param name="registerIndex">The register index.</param>
    /// <returns>The result of the operation.</returns>
    public ValueNode Reg16(RegisterIndex registerIndex) {
        return Reg(DataType.UINT16, registerIndex);
    }

    /// <summary>
    /// Performs the s reg operation.
    /// </summary>
    /// <param name="registerIndex">The register index.</param>
    /// <returns>The result of the operation.</returns>
    public ValueNode SReg(SegmentRegisterIndex registerIndex) {
        return SReg((int)registerIndex);
    }

    /// <summary>
    /// Performs the s reg operation.
    /// </summary>
    /// <param name="segmentRegisterIndex">The segment register index.</param>
    /// <returns>The result of the operation.</returns>
    public ValueNode SReg(int segmentRegisterIndex) {
        return new SegmentRegisterNode(segmentRegisterIndex);
    }

    /// <summary>
    /// Performs the reg 32 operation.
    /// </summary>
    /// <param name="registerIndex">The register index.</param>
    /// <returns>The result of the operation.</returns>
    public ValueNode Reg32(RegisterIndex registerIndex) {
        return Reg(DataType.UINT32, registerIndex);
    }

    /// <summary>
    /// Performs the accumulator operation.
    /// </summary>
    /// <param name="dataType">The data type.</param>
    /// <returns>The result of the operation.</returns>
    public ValueNode Accumulator(DataType dataType) {
        return new RegisterNode(dataType, (int)RegisterIndex.AxIndex);
    }

    /// <summary>
    /// Performs the reg operation.
    /// </summary>
    /// <param name="dataType">The data type.</param>
    /// <param name="registerIndex">The register index.</param>
    /// <returns>The result of the operation.</returns>
    public ValueNode Reg(DataType dataType, RegisterIndex registerIndex) {
        return Reg(dataType, (int)registerIndex);
    }

    /// <summary>
    /// Performs the reg operation.
    /// </summary>
    /// <param name="dataType">The data type.</param>
    /// <param name="registerIndex">The register index.</param>
    /// <returns>The result of the operation.</returns>
    public ValueNode Reg(DataType dataType, int registerIndex) {
        return new RegisterNode(dataType, registerIndex);
    }
}