namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast.Builder;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Value.Constant;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents the InstructionFieldAstBuilder class.
/// </summary>
public class InstructionFieldAstBuilder(ConstantAstBuilder constant, PointerAstBuilder pointer) {
    /// <summary>
    /// Gets or sets the Constant.
    /// </summary>
    public ConstantAstBuilder Constant { get; } = constant;
    /// <summary>
    /// Gets or sets the Pointer.
    /// </summary>
    public PointerAstBuilder Pointer { get; } = pointer;


    public ValueNode? ToNode(InstructionField<byte> field, bool nullIfZero = false) {
        return ToNode(ToType(field), field.Value, field.UseValue, field.PhysicalAddress, nullIfZero);
    }

    public ValueNode? ToNode(InstructionField<ushort> field, bool nullIfZero = false) {
        return ToNode(ToType(field), field.Value, field.UseValue, field.PhysicalAddress, nullIfZero);
    }

    public ValueNode? ToNode(InstructionField<uint> field, bool nullIfZero = false) {
        return ToNode(ToType(field), field.Value, field.UseValue, field.PhysicalAddress, nullIfZero);
    }

    public ValueNode? ToNode(InstructionField<sbyte> field, bool nullIfZero = false) {
        return ToNode(ToType(field), (uint)field.Value, field.UseValue, field.PhysicalAddress, nullIfZero);
    }

    public ValueNode? ToNode(InstructionField<short> field, bool nullIfZero = false) {
        return ToNode(ToType(field), (uint)field.Value, field.UseValue, field.PhysicalAddress, nullIfZero);
    }

    public ValueNode? ToNode(InstructionField<int> field, bool nullIfZero = false) {
        return ToNode(ToType(field), (uint)field.Value, field.UseValue, field.PhysicalAddress, nullIfZero);
    }

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
    /// ToNode method.
    /// </summary>
    public ValueNode ToNode(InstructionField<SegmentedAddress> field) {
        if (field.UseValue) {
            return Constant.ToNode(field.Value);
        }

        return Pointer.ToAbsolutePointer(DataType.UINT32, field.PhysicalAddress);
    }

    /// <summary>
    /// ToType method.
    /// </summary>
    public DataType ToType(InstructionField<byte> field) {
        return DataType.UINT8;
    }

    /// <summary>
    /// ToType method.
    /// </summary>
    public DataType ToType(InstructionField<ushort> field) {
        return DataType.UINT16;
    }

    /// <summary>
    /// ToType method.
    /// </summary>
    public DataType ToType(InstructionField<uint> field) {
        return DataType.UINT32;
    }

    /// <summary>
    /// ToType method.
    /// </summary>
    public DataType ToType(InstructionField<sbyte> field) {
        return DataType.INT8;
    }

    /// <summary>
    /// ToType method.
    /// </summary>
    public DataType ToType(InstructionField<short> field) {
        return DataType.INT16;
    }

    /// <summary>
    /// ToType method.
    /// </summary>
    public DataType ToType(InstructionField<int> field) {
        return DataType.INT32;
    }
}