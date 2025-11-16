namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast;

/// <summary>
/// Defines the contract for IVisitableAstNode.
/// </summary>
public interface IVisitableAstNode {
    /// <summary>
    /// Accept method.
    /// </summary>
    public T Accept<T>(IAstVisitor<T> astVisitor);
}