namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast.Builder;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value.Constant;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents instruction field ast builder.
/// </summary>
public class InstructionFieldAstBuilder(ConstantAstBuilder constant, PointerAstBuilder pointer) {
    /// <summary>
    /// Gets constant.
    /// </summary>
    public ConstantAstBuilder Constant { get; } = constant;
    /// <summary>
    /// Gets pointer.
    /// </summary>
    public PointerAstBuilder Pointer { get; } = pointer;


    /// <summary>
    /// Converts to node.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <param name="nullIfZero">The null if zero.</param>
    public ValueNode? ToNode(InstructionField<byte> field, bool nullIfZero = false) {
        return ToNode(ToType(field), field.Value, field.UseValue, field.PhysicalAddress, nullIfZero);
    }

    /// <summary>
    /// Converts to node.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <param name="nullIfZero">The null if zero.</param>
    public ValueNode? ToNode(InstructionField<ushort> field, bool nullIfZero = false) {
        return ToNode(ToType(field), field.Value, field.UseValue, field.PhysicalAddress, nullIfZero);
    }

    /// <summary>
    /// Converts to node.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <param name="nullIfZero">The null if zero.</param>
    public ValueNode? ToNode(InstructionField<uint> field, bool nullIfZero = false) {
        return ToNode(ToType(field), field.Value, field.UseValue, field.PhysicalAddress, nullIfZero);
    }

    /// <summary>
    /// Converts to node.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <param name="nullIfZero">The null if zero.</param>
    public ValueNode? ToNode(InstructionField<sbyte> field, bool nullIfZero = false) {
        return ToNode(ToType(field), (uint)field.Value, field.UseValue, field.PhysicalAddress, nullIfZero);
    }

    /// <summary>
    /// Converts to node.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <param name="nullIfZero">The null if zero.</param>
    public ValueNode? ToNode(InstructionField<short> field, bool nullIfZero = false) {
        return ToNode(ToType(field), (uint)field.Value, field.UseValue, field.PhysicalAddress, nullIfZero);
    }

    /// <summary>
    /// Converts to node.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <param name="nullIfZero">The null if zero.</param>
    public ValueNode? ToNode(InstructionField<int> field, bool nullIfZero = false) {
        return ToNode(ToType(field), (uint)field.Value, field.UseValue, field.PhysicalAddress, nullIfZero);
    }

    /// <summary>
    /// Converts to node.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="value">The value.</param>
    /// <param name="useValue">The use value.</param>
    /// <param name="physicalAddress">The physical address.</param>
    /// <param name="nullIfZero">The null if zero.</param>
    public ValueNode? ToNode(DataType type, uint value, bool useValue, uint physicalAddress, bool nullIfZero) {
        if (useValue) {
            if (value == 0 && nullIfZero) {
                return null;
            }
            return new ConstantNode(type, value);
        }
        return Pointer.ToAbsolutePointer(type, physicalAddress);
    }

    /// <summary>
    /// Converts to node.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <returns>The result of the operation.</returns>
    public ValueNode ToNode(InstructionField<SegmentedAddress> field) {
        if (field.UseValue) {
            return Constant.ToNode(field.Value);
        }

        return Pointer.ToAbsolutePointer(DataType.UINT32, field.PhysicalAddress);
    }

    /// <summary>
    /// Converts to type.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <returns>The result of the operation.</returns>
    public DataType ToType(InstructionField<byte> field) {
        return DataType.UINT8;
    }

    /// <summary>
    /// Converts to type.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <returns>The result of the operation.</returns>
    public DataType ToType(InstructionField<ushort> field) {
        return DataType.UINT16;
    }

    /// <summary>
    /// Converts to type.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <returns>The result of the operation.</returns>
    public DataType ToType(InstructionField<uint> field) {
        return DataType.UINT32;
    }

    /// <summary>
    /// Converts to type.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <returns>The result of the operation.</returns>
    public DataType ToType(InstructionField<sbyte> field) {
        return DataType.INT8;
    }

    /// <summary>
    /// Converts to type.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <returns>The result of the operation.</returns>
    public DataType ToType(InstructionField<short> field) {
        return DataType.INT16;
    }

    /// <summary>
    /// Converts to type.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <returns>The result of the operation.</returns>
    public DataType ToType(InstructionField<int> field) {
        return DataType.INT32;
    }
}