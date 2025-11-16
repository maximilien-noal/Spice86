namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast.Operations;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value;

/// <summary>
/// Represents the BinaryOperationNode class.
/// </summary>
public class BinaryOperationNode(DataType dataType, ValueNode left, BinaryOperation binaryOperation, ValueNode right) : ValueNode(dataType) {
    /// <summary>
    /// Gets or sets the Left.
    /// </summary>
    public ValueNode Left { get; } = left;
    /// <summary>
    /// Gets or sets the BinaryOperation.
    /// </summary>
    public BinaryOperation BinaryOperation { get; } = binaryOperation;
    /// <summary>
    /// Gets or sets the Right.
    /// </summary>
    public ValueNode Right { get; } = right;

    /// <summary>
    /// T method.
    /// </summary>
    public override T Accept<T>(IAstVisitor<T> astVisitor) {
        return astVisitor.VisitBinaryOperationNode(this);
    }
}