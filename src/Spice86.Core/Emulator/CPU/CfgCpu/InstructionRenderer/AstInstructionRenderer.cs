namespace Spice86.Core.Emulator.CPU.CfgCpu.InstructionRenderer;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Instruction;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Operations;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value.Constant;
using Spice86.Shared.Emulator.Memory;

using System.Globalization;
using System.Linq;

/// <summary>
/// Represents ast instruction renderer.
/// </summary>
public class AstInstructionRenderer : IAstVisitor<string> {
    private readonly RegisterRenderer _registerRenderer = new();

    /// <summary>
    /// Performs the visit segment register node operation.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>The result of the operation.</returns>
    public string VisitSegmentRegisterNode(SegmentRegisterNode node) {
        return _registerRenderer.ToStringSegmentRegister(node.RegisterIndex);
    }

    /// <summary>
    /// Performs the visit segmented pointer operation.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>The result of the operation.</returns>
    public string VisitSegmentedPointer(SegmentedPointerNode node) {
        string offset = node.Offset.Accept(this);
        string segment = node.Segment.Accept(this);

        return PointerDataTypeToString(node.DataType) + " " + segment + ":[" + offset + "]";
    }

    /// <summary>
    /// Performs the visit register node operation.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>The result of the operation.</returns>
    public string VisitRegisterNode(RegisterNode node) {
        return _registerRenderer.ToStringRegister(node.DataType.BitWidth, node.RegisterIndex);
    }

    /// <summary>
    /// Performs the visit absolute pointer node operation.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>The result of the operation.</returns>
    public string VisitAbsolutePointerNode(AbsolutePointerNode node) {
        string absoluteAddress = node.AbsoluteAddress.Accept(this);
        return PointerDataTypeToString(node.DataType) + " [" + absoluteAddress + "]";
    }

    /// <summary>
    /// Performs the visit constant node operation.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>The result of the operation.</returns>
    public string VisitConstantNode(ConstantNode node) {
        if (IsNegative(node)) {
            int valueSigned = SignExtend(node.Value, node.DataType.BitWidth);
            return valueSigned.ToString(CultureInfo.InvariantCulture);
        }
        uint value = node.Value;
        if (value < 10) {
            // render it as decimal as it is the same and it will save the 0x0
            return value.ToString(CultureInfo.InvariantCulture);
        }

        return node.DataType.BitWidth switch {
            BitWidth.BYTE_8 => $"0x{value:X2}",
            BitWidth.WORD_16 => $"0x{value:X4}",
            BitWidth.DWORD_32 => $"0x{value:X8}",
            _ => throw new InvalidOperationException($"Unsupported bit width {node.DataType.BitWidth}")
        };
    }

    private bool IsNegative(ConstantNode node) {
        if (node.DataType.Signed) {
            int value = SignExtend(node.Value, node.DataType.BitWidth);
            if (value < 0) {
                return true;
            }
        }
        return false;
    }

    private int SignExtend(uint value, BitWidth size) {
        return size switch {
            BitWidth.BYTE_8 => (sbyte)value,
            BitWidth.WORD_16 => (short)value,
            BitWidth.DWORD_32 => (int)value,
            _ => throw new InvalidOperationException($"Unsupported bit width {size}")
        };
    }

    /// <summary>
    /// Performs the visit segmented address constant node operation.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>The result of the operation.</returns>
    public string VisitSegmentedAddressConstantNode(SegmentedAddressConstantNode node) {
        return node.Value.ToString();
    }

    /// <summary>
    /// Performs the visit binary operation node operation.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>The result of the operation.</returns>
    public string VisitBinaryOperationNode(BinaryOperationNode node) {
        string left = node.Left.Accept(this);
        if (IsZero(node.Right) && node.BinaryOperation == BinaryOperation.PLUS) {
            return left;
        }
        string right = node.Right.Accept(this);
        if (IsNegative(node.Right) && node.BinaryOperation == BinaryOperation.PLUS) {
            return left + right;
        }
        return left + OperationToString(node.BinaryOperation) + right;
    }

    /// <summary>
    /// Performs the visit unary operation node operation.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>The result of the operation.</returns>
    public string VisitUnaryOperationNode(UnaryOperationNode node) {
        string value = node.Value.Accept(this);
        return OperationToString(node.UnaryOperation) + value;
    }

    private bool IsZero(ValueNode valueNode) {
        return valueNode is ConstantNode constantNode && constantNode.Value == 0;
    }

    private bool IsNegative(ValueNode valueNode) {
        return valueNode is ConstantNode constantNode && IsNegative(constantNode);
    }

    /// <summary>
    /// Performs the visit instruction node operation.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>The result of the operation.</returns>
    public string VisitInstructionNode(InstructionNode node) {
        RepPrefix? repPrefix = node.RepPrefix;
        string prefix = "";
        if (repPrefix != null) {
            prefix = Enum.GetName(repPrefix.Value)?.ToLower() + " ";
        }
        string mnemonic = prefix + Enum.GetName(node.Operation)?.ToLower().Replace("_", " ");
        if (node.Parameters.Count == 0) {
            return mnemonic;
        }

        return mnemonic + " " + string.Join(",", node.Parameters.Select(param => param.Accept(this)));
    }

    private string OperationToString(BinaryOperation binaryOperation) {
        return binaryOperation switch {
            BinaryOperation.PLUS => "+",
            BinaryOperation.MULTIPLY => "*",
            BinaryOperation.EQUAL => "==",
            BinaryOperation.NOT_EQUAL => "!=",
            BinaryOperation.ASSIGN => "=",
            _ => throw new InvalidOperationException($"Unsupported AST operation {binaryOperation}")
        };
    }

    private string OperationToString(UnaryOperation unaryOperation) {
        return unaryOperation switch {
            UnaryOperation.NOT => "!",
            _ => throw new InvalidOperationException($"Unsupported AST operation {unaryOperation}")
        };
    }

    private string PointerDataTypeToString(DataType dataType) {
        return dataType.BitWidth switch {
            BitWidth.BYTE_8 => "byte ptr",
            BitWidth.WORD_16 => "word ptr",
            BitWidth.DWORD_32 => "dword ptr",
            _ => throw new InvalidOperationException($"Unsupported bit width {dataType.BitWidth}")
        };
    }
}