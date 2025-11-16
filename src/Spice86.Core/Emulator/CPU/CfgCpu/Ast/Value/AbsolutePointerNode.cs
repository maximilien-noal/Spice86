namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast;

/// <summary>
/// Represents the AbsolutePointerNode class.
/// </summary>
public class AbsolutePointerNode(DataType dataType, ValueNode absoluteAddress) : ValueNode(dataType) {
    /// <summary>
    /// Gets or sets the AbsoluteAddress.
    /// </summary>
    public ValueNode AbsoluteAddress { get; } = absoluteAddress;
    /// <summary>
    /// T method.
    /// </summary>
    public override T Accept<T>(IAstVisitor<T> astVisitor) {
        return astVisitor.VisitAbsolutePointerNode(this);
    }
}