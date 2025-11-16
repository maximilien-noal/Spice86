namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast.Builder;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value;
using Spice86.Core.Emulator.CPU.Registers;

/// <summary>
/// Represents the RegisterAstBuilder class.
/// </summary>
public class RegisterAstBuilder {
    /// <summary>
    /// Reg8 method.
    /// </summary>
    public ValueNode Reg8(RegisterIndex registerIndex) {
        return Reg(DataType.UINT8, registerIndex);
    }

    /// <summary>
    /// Reg16 method.
    /// </summary>
    public ValueNode Reg16(RegisterIndex registerIndex) {
        return Reg(DataType.UINT16, registerIndex);
    }

    /// <summary>
    /// SReg method.
    /// </summary>
    public ValueNode SReg(SegmentRegisterIndex registerIndex) {
        return SReg((int)registerIndex);
    }

    /// <summary>
    /// SReg method.
    /// </summary>
    public ValueNode SReg(int segmentRegisterIndex) {
        return new SegmentRegisterNode(segmentRegisterIndex);
    }

    /// <summary>
    /// Reg32 method.
    /// </summary>
    public ValueNode Reg32(RegisterIndex registerIndex) {
        return Reg(DataType.UINT32, registerIndex);
    }

    /// <summary>
    /// Accumulator method.
    /// </summary>
    public ValueNode Accumulator(DataType dataType) {
        return new RegisterNode(dataType, (int)RegisterIndex.AxIndex);
    }

    /// <summary>
    /// Reg method.
    /// </summary>
    public ValueNode Reg(DataType dataType, RegisterIndex registerIndex) {
        return Reg(dataType, (int)registerIndex);
    }

    /// <summary>
    /// Reg method.
    /// </summary>
    public ValueNode Reg(DataType dataType, int registerIndex) {
        return new RegisterNode(dataType, registerIndex);
    }
}