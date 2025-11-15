namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast;

/// <summary>
/// Represents absolute pointer node.
/// </summary>
public class AbsolutePointerNode(DataType dataType, ValueNode absoluteAddress) : ValueNode(dataType) {
    /// <summary>
    /// Gets absolute address.
    /// </summary>
    public ValueNode AbsoluteAddress { get; } = absoluteAddress;
    public override T Accept<T>(IAstVisitor<T> astVisitor) {
        return astVisitor.VisitAbsolutePointerNode(this);
    }
}