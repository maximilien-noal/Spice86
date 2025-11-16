namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast.Operations;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value;

/// <summary>
/// Represents the UnaryOperationNode class.
/// </summary>
public class UnaryOperationNode(DataType dataType, UnaryOperation unaryOperation, ValueNode value) : ValueNode(dataType) {
    /// <summary>
    /// Gets or sets the UnaryOperation.
    /// </summary>
    public UnaryOperation UnaryOperation { get; } = unaryOperation;
    /// <summary>
    /// Gets or sets the Value.
    /// </summary>
    public ValueNode Value { get; } = value;

    /// <summary>
    /// T method.
    /// </summary>
    public override T Accept<T>(IAstVisitor<T> astVisitor) {
        return astVisitor.VisitUnaryOperationNode(this);
    }
}