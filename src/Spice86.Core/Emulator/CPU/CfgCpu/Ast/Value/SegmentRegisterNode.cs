namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast;

/// <summary>
/// Represents the SegmentRegisterNode class.
/// </summary>
public class SegmentRegisterNode(int registerIndex) : RegisterNode(DataType.UINT16, registerIndex) {
    /// <summary>
    /// T method.
    /// </summary>
    public override T Accept<T>(IAstVisitor<T> astVisitor) {
        return astVisitor.VisitSegmentRegisterNode(this);
    }
}