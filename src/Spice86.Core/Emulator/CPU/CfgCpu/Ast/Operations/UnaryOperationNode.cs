namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast.Operations;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value;

/// <summary>
/// Represents unary operation node.
/// </summary>
public class UnaryOperationNode(DataType dataType, UnaryOperation unaryOperation, ValueNode value) : ValueNode(dataType) {
    /// <summary>
    /// Gets unary operation.
    /// </summary>
    public UnaryOperation UnaryOperation { get; } = unaryOperation;
    /// <summary>
    /// Gets value.
    /// </summary>
    public ValueNode Value { get; } = value;

    public override T Accept<T>(IAstVisitor<T> astVisitor) {
        return astVisitor.VisitUnaryOperationNode(this);
    }
}