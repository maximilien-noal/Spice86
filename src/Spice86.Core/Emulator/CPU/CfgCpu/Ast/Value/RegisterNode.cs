namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast;

/// <summary>
/// Represents the RegisterNode class.
/// </summary>
public class RegisterNode(DataType dataType, int registerIndex) : ValueNode(dataType) {
    /// <summary>
    /// Gets or sets the RegisterIndex.
    /// </summary>
    public int RegisterIndex { get; } = registerIndex;
    /// <summary>
    /// T method.
    /// </summary>
    public override T Accept<T>(IAstVisitor<T> astVisitor) {
        return astVisitor.VisitRegisterNode(this);
    }
}