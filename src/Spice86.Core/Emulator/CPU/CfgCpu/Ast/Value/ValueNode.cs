namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast;

/// <summary>
/// class method.
/// </summary>
public abstract class ValueNode(DataType dataType) : IVisitableAstNode {

    /// <summary>
    /// Gets or sets the DataType.
    /// </summary>
    public DataType DataType { get; } = dataType;

    /// <summary>
    /// T method.
    /// </summary>
    public abstract T Accept<T>(IAstVisitor<T> astVisitor);
}