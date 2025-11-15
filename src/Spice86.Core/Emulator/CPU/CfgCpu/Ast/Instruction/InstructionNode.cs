namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast.Instruction;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value;

/// <summary>
/// Represents instruction node.
/// </summary>
public class InstructionNode(RepPrefix? repPrefix, InstructionOperation operation, params ValueNode[] parameters) : IVisitableAstNode {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="parameters">The parameters.</param>
    public InstructionNode(InstructionOperation operation, params ValueNode[] parameters) : this(null, operation, parameters) {
    }
    /// <summary>
    /// Gets rep prefix.
    /// </summary>
    public RepPrefix? RepPrefix { get; } = repPrefix;
    /// <summary>
    /// Gets operation.
    /// </summary>
    public InstructionOperation Operation { get; } = operation;
    /// <summary>
    /// Gets parameters.
    /// </summary>
    public IReadOnlyList<ValueNode> Parameters { get; } = parameters;

    public virtual T Accept<T>(IAstVisitor<T> astVisitor) {
        return astVisitor.VisitInstructionNode(this);
    }
}