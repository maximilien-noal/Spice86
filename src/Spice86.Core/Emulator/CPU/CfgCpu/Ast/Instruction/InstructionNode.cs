namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast.Instruction;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value;

/// <summary>
/// Represents the InstructionNode class.
/// </summary>
public class InstructionNode(RepPrefix? repPrefix, InstructionOperation operation, params ValueNode[] parameters) : IVisitableAstNode {
    public InstructionNode(InstructionOperation operation, params ValueNode[] parameters) : this(null, operation, parameters) {
    }
    public RepPrefix? RepPrefix { get; } = repPrefix;
    /// <summary>
    /// Gets or sets the Operation.
    /// </summary>
    public InstructionOperation Operation { get; } = operation;
    /// <summary>
    /// Gets or sets the Parameters.
    /// </summary>
    public IReadOnlyList<ValueNode> Parameters { get; } = parameters;

    /// <summary>
    /// T method.
    /// </summary>
    public virtual T Accept<T>(IAstVisitor<T> astVisitor) {
        return astVisitor.VisitInstructionNode(this);
    }
}