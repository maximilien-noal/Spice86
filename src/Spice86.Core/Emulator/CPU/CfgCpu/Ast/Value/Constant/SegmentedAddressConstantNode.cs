namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value.Constant;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents the SegmentedAddressConstantNode class.
/// </summary>
public class SegmentedAddressConstantNode(SegmentedAddress value) : ValueNode(DataType.UINT32) {
    /// <summary>
    /// Gets or sets the Value.
    /// </summary>
    public SegmentedAddress Value { get; } = value;

    /// <summary>
    /// T method.
    /// </summary>
    public override T Accept<T>(IAstVisitor<T> astVisitor) {
        return astVisitor.VisitSegmentedAddressConstantNode(this);
    }
}