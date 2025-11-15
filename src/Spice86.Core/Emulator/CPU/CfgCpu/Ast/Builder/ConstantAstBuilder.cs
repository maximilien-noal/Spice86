namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast.Builder;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value.Constant;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents constant ast builder.
/// </summary>
public class ConstantAstBuilder {
    /// <summary>
    /// Converts to node.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the operation.</returns>
    public ValueNode ToNode(byte value) {
        return new ConstantNode(DataType.UINT8, value);
    }

    /// <summary>
    /// Converts to node.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the operation.</returns>
    public ValueNode ToNode(ushort value) {
        return new ConstantNode(DataType.UINT16, value);
    }

    /// <summary>
    /// Converts to node.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the operation.</returns>
    public ValueNode ToNode(uint value) {
        return new ConstantNode(DataType.UINT32, value);
    }

    /// <summary>
    /// Converts to node.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the operation.</returns>
    public ValueNode ToNode(sbyte value) {
        return new ConstantNode(DataType.INT8, (byte)value);
    }

    /// <summary>
    /// Converts to node.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the operation.</returns>
    public ValueNode ToNode(short value) {
        return new ConstantNode(DataType.INT16, (ushort)value);
    }

    /// <summary>
    /// Converts to node.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the operation.</returns>
    public ValueNode ToNode(int value) {
        return new ConstantNode(DataType.INT32, (uint)value);
    }

    /// <summary>
    /// Converts to node.
    /// </summary>
    /// <param name="segmentedAddress">The segmented address.</param>
    /// <returns>The result of the operation.</returns>
    public ValueNode ToNode(SegmentedAddress segmentedAddress) {
        return new SegmentedAddressConstantNode(segmentedAddress);
    }
}