namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast;

/// <summary>
/// Represents segmented pointer node.
/// </summary>
public class SegmentedPointerNode(DataType dataType, ValueNode segment, ValueNode offset) : ValueNode(dataType) {
    /// <summary>
    /// Gets segment.
    /// </summary>
    public ValueNode Segment { get; } = segment;
    /// <summary>
    /// Gets or sets offset.
    /// </summary>
    public ValueNode Offset { get; } = offset;

    public override T Accept<T>(IAstVisitor<T> astVisitor) {
        return astVisitor.VisitSegmentedPointer(this);
    }
}