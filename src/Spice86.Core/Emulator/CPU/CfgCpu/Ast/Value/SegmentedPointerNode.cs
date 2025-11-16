namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast;

/// <summary>
/// Represents the SegmentedPointerNode class.
/// </summary>
public class SegmentedPointerNode(DataType dataType, ValueNode segment, ValueNode offset) : ValueNode(dataType) {
    /// <summary>
    /// Gets or sets the Segment.
    /// </summary>
    public ValueNode Segment { get; } = segment;
    /// <summary>
    /// Gets or sets the Offset.
    /// </summary>
    public ValueNode Offset { get; } = offset;

    /// <summary>
    /// T method.
    /// </summary>
    public override T Accept<T>(IAstVisitor<T> astVisitor) {
        return astVisitor.VisitSegmentedPointer(this);
    }
}