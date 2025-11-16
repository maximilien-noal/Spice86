namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Instruction;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Operations;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value.Constant;

/// <summary>
/// Defines the contract for IAstVisitor.
/// </summary>
public interface IAstVisitor<T> {
    /// <summary>
    /// VisitSegmentRegisterNode method.
    /// </summary>
    public T VisitSegmentRegisterNode(SegmentRegisterNode node);
    /// <summary>
    /// VisitSegmentedPointer method.
    /// </summary>
    public T VisitSegmentedPointer(SegmentedPointerNode node);
    /// <summary>
    /// VisitRegisterNode method.
    /// </summary>
    public T VisitRegisterNode(RegisterNode node);
    /// <summary>
    /// VisitAbsolutePointerNode method.
    /// </summary>
    public T VisitAbsolutePointerNode(AbsolutePointerNode node);
    /// <summary>
    /// VisitSegmentedAddressConstantNode method.
    /// </summary>
    public T VisitSegmentedAddressConstantNode(SegmentedAddressConstantNode node);
    /// <summary>
    /// VisitBinaryOperationNode method.
    /// </summary>
    public T VisitBinaryOperationNode(BinaryOperationNode node);
    /// <summary>
    /// VisitUnaryOperationNode method.
    /// </summary>
    public T VisitUnaryOperationNode(UnaryOperationNode node);
    /// <summary>
    /// VisitInstructionNode method.
    /// </summary>
    public T VisitInstructionNode(InstructionNode node);
    /// <summary>
    /// VisitConstantNode method.
    /// </summary>
    public T VisitConstantNode(ConstantNode node);
}