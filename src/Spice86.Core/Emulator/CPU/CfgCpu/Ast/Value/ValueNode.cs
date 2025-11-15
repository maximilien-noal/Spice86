namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast;

/// <summary>
/// Represents value node.
/// </summary>
public abstract class ValueNode(DataType dataType) : IVisitableAstNode {

    /// <summary>
    /// Gets data type.
    /// </summary>
    public DataType DataType { get; } = dataType;

    public abstract T Accept<T>(IAstVisitor<T> astVisitor);
}