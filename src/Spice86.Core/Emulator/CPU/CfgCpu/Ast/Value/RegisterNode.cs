namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast;

/// <summary>
/// Represents register node.
/// </summary>
public class RegisterNode(DataType dataType, int registerIndex) : ValueNode(dataType) {
    /// <summary>
    /// Gets register index.
    /// </summary>
    public int RegisterIndex { get; } = registerIndex;
    public override T Accept<T>(IAstVisitor<T> astVisitor) {
        return astVisitor.VisitRegisterNode(this);
    }
}