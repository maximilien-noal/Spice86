namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value.Constant;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents the ConstantNode class.
/// </summary>
public class ConstantNode(DataType dataType, uint value) : ValueNode(dataType) {
    /// <summary>
    /// Gets or sets the Value.
    /// </summary>
    public uint Value { get; } = value;
    /// <summary>
    /// T method.
    /// </summary>
    public override T Accept<T>(IAstVisitor<T> astVisitor) {
        return astVisitor.VisitConstantNode(this);
    }
}