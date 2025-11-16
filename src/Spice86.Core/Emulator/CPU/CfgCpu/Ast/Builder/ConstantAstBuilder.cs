namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast.Builder;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value.Constant;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents the ConstantAstBuilder class.
/// </summary>
public class ConstantAstBuilder {
    /// <summary>
    /// ToNode method.
    /// </summary>
    public ValueNode ToNode(byte value) {
        return new ConstantNode(DataType.UINT8, value);
    }

    /// <summary>
    /// ToNode method.
    /// </summary>
    public ValueNode ToNode(ushort value) {
        return new ConstantNode(DataType.UINT16, value);
    }

    /// <summary>
    /// ToNode method.
    /// </summary>
    public ValueNode ToNode(uint value) {
        return new ConstantNode(DataType.UINT32, value);
    }

    /// <summary>
    /// ToNode method.
    /// </summary>
    public ValueNode ToNode(sbyte value) {
        return new ConstantNode(DataType.INT8, (byte)value);
    }

    /// <summary>
    /// ToNode method.
    /// </summary>
    public ValueNode ToNode(short value) {
        return new ConstantNode(DataType.INT16, (ushort)value);
    }

    /// <summary>
    /// ToNode method.
    /// </summary>
    public ValueNode ToNode(int value) {
        return new ConstantNode(DataType.INT32, (uint)value);
    }

    /// <summary>
    /// ToNode method.
    /// </summary>
    public ValueNode ToNode(SegmentedAddress segmentedAddress) {
        return new SegmentedAddressConstantNode(segmentedAddress);
    }
}