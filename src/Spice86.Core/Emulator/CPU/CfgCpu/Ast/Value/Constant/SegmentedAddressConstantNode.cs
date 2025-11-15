namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value.Constant;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents segmented address constant node.
/// </summary>
public class SegmentedAddressConstantNode(SegmentedAddress value) : ValueNode(DataType.UINT32) {
    /// <summary>
    /// Gets value.
    /// </summary>
    public SegmentedAddress Value { get; } = value;

    public override T Accept<T>(IAstVisitor<T> astVisitor) {
        return astVisitor.VisitSegmentedAddressConstantNode(this);
    }
}