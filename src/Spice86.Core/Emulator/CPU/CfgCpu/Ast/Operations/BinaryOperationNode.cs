namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast.Operations;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value;

/// <summary>
/// Represents binary operation node.
/// </summary>
public class BinaryOperationNode(DataType dataType, ValueNode left, BinaryOperation binaryOperation, ValueNode right) : ValueNode(dataType) {
    /// <summary>
    /// Gets left.
    /// </summary>
    public ValueNode Left { get; } = left;
    /// <summary>
    /// Gets binary operation.
    /// </summary>
    public BinaryOperation BinaryOperation { get; } = binaryOperation;
    /// <summary>
    /// Gets right.
    /// </summary>
    public ValueNode Right { get; } = right;

    public override T Accept<T>(IAstVisitor<T> astVisitor) {
        return astVisitor.VisitBinaryOperationNode(this);
    }
}