namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast;

/// <summary>
/// Defines the contract for i visitable ast node.
/// </summary>
public interface IVisitableAstNode {
    public T Accept<T>(IAstVisitor<T> astVisitor);
}