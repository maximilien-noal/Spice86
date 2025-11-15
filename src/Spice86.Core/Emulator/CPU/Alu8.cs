namespace Spice86.Core.Emulator.CPU;

using Spice86.Core.Emulator.CPU.Exceptions;

/// <summary>
/// Arithmetic Logic Unit code for 8bits operations.
/// </summary>
public class Alu8 : Alu<byte, sbyte, ushort, short> {
    private const byte BeforeMsbMask = 0x40;

    private const byte MsbMask = 0x80;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="state">The class representing the CPU registers, flags, and execution state.</param>
    public Alu8(State state) : base(state) {
    }

    /// <summary>
    /// Adds .
    /// </summary>
    /// <param name="value1">The value 1.</param>
    /// <param name="value2">The value 2.</param>
    /// <param name="useCarry">The use carry.</param>
    /// <returns>The result of the operation.</returns>
    public override byte Add(byte value1, byte value2, bool useCarry) {
        int carry = useCarry && _state.CarryFlag ? 1 : 0;
        byte res = (byte)(value1 + value2 + carry);
        UpdateFlags(res);
        uint carryBits = CarryBitsAdd(value1, value2, res);
        uint overflowBits = OverflowBitsAdd(value1, value2, res);
        _state.CarryFlag = (carryBits >> 7 & 1) == 1;
        _state.AuxiliaryFlag = (carryBits >> 3 & 1) == 1;
        _state.OverflowFlag = (overflowBits >> 7 & 1) == 1;
        return res;
    }

    /// <summary>
    /// Performs the and operation.
    /// </summary>
    /// <param name="value1">The value 1.</param>
    /// <param name="value2">The value 2.</param>
    /// <returns>The result of the operation.</returns>
    public override byte And(byte value1, byte value2) {
        byte res = (byte)(value1 & value2);
        UpdateFlags(res);
        _state.CarryFlag = false;
        _state.OverflowFlag = false;
        return res;
    }

    /// <summary>
    /// Performs the div operation.
    /// </summary>
    /// <param name="value1">The value 1.</param>
    /// <param name="value2">The value 2.</param>
    /// <returns>The result of the operation.</returns>
    public override byte Div(ushort value1, byte value2) {
        if (value2 == 0) {
            throw new CpuDivisionErrorException($"Division by zero");
        }

        uint res = (uint)(value1 / value2);
        if (res > byte.MaxValue) {
            throw new CpuDivisionErrorException($"Division result out of range: {res}");
        }

        return (byte)res;
    }

    /// <summary>
    /// Performs the idiv operation.
    /// </summary>
    /// <param name="value1">The value 1.</param>
    /// <param name="value2">The value 2.</param>
    /// <returns>The result of the operation.</returns>
    public override sbyte Idiv(short value1, sbyte value2) {
        if (value2 == 0) {
            throw new CpuDivisionErrorException($"Division by zero");
        }

        int res = value1 / value2;
        unchecked {
            if (res is > 0x7F or < ((sbyte)0x80)) {
                throw new CpuDivisionErrorException($"Division result out of range: {res}");
            }
        }

        return (sbyte)res;
    }

    /// <summary>
    /// Performs the imul operation.
    /// </summary>
    /// <param name="value1">The value 1.</param>
    /// <param name="value2">The value 2.</param>
    /// <returns>The result of the operation.</returns>
    public override short Imul(sbyte value1, sbyte value2) {
        int res = value1 * value2;
        bool doesNotFitInByte = res != (sbyte)res;
        _state.OverflowFlag = doesNotFitInByte;
        _state.CarryFlag = doesNotFitInByte;
        return (short)res;
    }


    /// <summary>
    /// Performs the mul operation.
    /// </summary>
    /// <param name="value1">The value 1.</param>
    /// <param name="value2">The value 2.</param>
    /// <returns>The result of the operation.</returns>
    public override ushort Mul(byte value1, byte value2) {
        ushort res = (ushort)(value1 * value2);
        bool upperHalfNonZero = (res & 0xFF00) != 0;
        _state.OverflowFlag = upperHalfNonZero;
        _state.CarryFlag = upperHalfNonZero;
        _state.ZeroFlag = (res & 0x00FF) == 0;
        return res;
    }

    /// <summary>
    /// Performs the or operation.
    /// </summary>
    /// <param name="value1">The value 1.</param>
    /// <param name="value2">The value 2.</param>
    /// <returns>The result of the operation.</returns>
    public override byte Or(byte value1, byte value2) {
        byte res = (byte)(value1 | value2);
        UpdateFlags(res);
        _state.CarryFlag = false;
        _state.OverflowFlag = false;
        // Undocumented 8086
        _state.AuxiliaryFlag = false;
        return res;
    }

    /// <summary>
    /// Performs the rcl operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="count">The count.</param>
    /// <returns>The result of the operation.</returns>
    public override byte Rcl(byte value, byte count) {
        count = (byte)((count & ShiftCountMask) % 9);
        if (count == 0) {
            return value;
        }

        bool oldCarry = _state.CarryFlag;
        int carry = value >> 8 - count & 0x1;
        byte res = (byte)(value << count);
        int mask = (1 << count - 1) - 1;
        res = (byte)(res | (value >> 9 - count & mask));
        if (oldCarry) {
            res = (byte)(res | 1 << count - 1);
        }

        _state.CarryFlag = carry != 0;
        bool msb = (res & MsbMask) != 0;
        _state.OverflowFlag = _state.CarryFlag ^ msb;
        return res;
    }

    /// <summary>
    /// Performs the rcr operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="count">The count.</param>
    /// <returns>The result of the operation.</returns>
    public override byte Rcr(byte value, int count) {
        count = (count & ShiftCountMask) % 9;
        if (count == 0) {
            return value;
        }

        bool oldCarry = _state.CarryFlag;
        int carry = value >> count - 1 & 0x1;
        int mask = (1 << 8 - count) - 1;
        byte res = (byte)(value >> count & mask);
        res = (byte)(res | value << 9 - count);
        if (oldCarry) {
            res = (byte)(res | 1 << 8 - count);
        }

        _state.CarryFlag = carry != 0;
        SetOverflowForRigthRotate8(res);
        return res;
    }

    /// <summary>
    /// Performs the rol operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="count">The count.</param>
    /// <returns>The result of the operation.</returns>
    public override byte Rol(byte value, byte count) {
        count = (byte)((count & ShiftCountMask) % 8);
        if (count == 0) {
            return value;
        }

        int carry = value >> 8 - count & 0x1;
        byte res = (byte)(value << count);
        res = (byte)(res | value >> 8 - count);
        _state.CarryFlag = carry != 0;
        bool msb = (res & MsbMask) != 0;
        bool lsb = (res & 0x01) != 0;
        _state.OverflowFlag = msb ^ lsb;
        return res;
    }

    /// <summary>
    /// Performs the ror operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="count">The count.</param>
    /// <returns>The result of the operation.</returns>
    public override byte Ror(byte value, int count) {
        count = (count & ShiftCountMask) % 8;
        if (count == 0) {
            return value;
        }

        int carry = value >> count - 1 & 0x1;
        int mask = (1 << 8 - count) - 1;
        byte res = (byte)(value >> count & mask);
        res = (byte)(res | value << 8 - count);
        _state.CarryFlag = carry != 0;
        SetOverflowForRigthRotate8(res);
        return res;
    }

    /// <summary>
    /// Performs the sar operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="count">The count.</param>
    /// <returns>The result of the operation.</returns>
    public override byte Sar(byte value, int count) {
        count &= ShiftCountMask;
        if (count == 0) {
            return value;
        }

        sbyte res = (sbyte)value;
        SetCarryFlagForRightShifts((uint)res, count);
        res >>= count;
        UpdateFlags((byte)res);
        _state.OverflowFlag = false;
        return (byte)res;
    }

    /// <summary>
    /// Performs the shl operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="count">The count.</param>
    /// <returns>The result of the operation.</returns>
    public override byte Shl(byte value, int count) {
        count &= ShiftCountMask;
        if (count == 0) {
            return value;
        }

        int msbBefore = value << count - 1 & MsbMask;
        _state.CarryFlag = msbBefore != 0;
        byte res = (byte)(value << count);
        UpdateFlags(res);
        _state.OverflowFlag = ((res ^ value) & MsbMask) != 0;
        return res;
    }

    /// <summary>
    /// Performs the shld operation.
    /// </summary>
    /// <param name="destination">The destination.</param>
    /// <param name="source">The source.</param>
    /// <param name="count">The count.</param>
    /// <returns>The result of the operation.</returns>
    public override byte Shld(byte destination, byte source, byte count) {
        throw new NotImplementedException("Shld is not available for 8bits operations");
    }

    /// <summary>
    /// Performs the shrd operation.
    /// </summary>
    /// <param name="destination">The destination.</param>
    /// <param name="source">The source.</param>
    /// <param name="count">The count.</param>
    /// <returns>The result of the operation.</returns>
    public override byte Shrd(byte destination, byte source, byte count) {
        throw new NotImplementedException("Shrd is not available for 8bits operations");
    }

    /// <summary>
    /// Performs the shr operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="count">The count.</param>
    /// <returns>The result of the operation.</returns>
    public override byte Shr(byte value, int count) {
        count &= ShiftCountMask;
        if (count == 0) {
            return value;
        }

        int msb = value & MsbMask;
        SetCarryFlagForRightShifts(value, count);
        byte res = (byte)(value >> count);
        UpdateFlags(res);
        _state.OverflowFlag = count == 1 && msb != 0;
        return res;
    }

    /// <summary>
    /// Performs the sub operation.
    /// </summary>
    /// <param name="value1">The value 1.</param>
    /// <param name="value2">The value 2.</param>
    /// <param name="useCarry">The use carry.</param>
    /// <returns>The result of the operation.</returns>
    public override byte Sub(byte value1, byte value2, bool useCarry) {
        int carry = useCarry && _state.CarryFlag ? 1 : 0;
        byte res = (byte)(value1 - value2 - carry);
        UpdateFlags(res);
        uint borrowBits = BorrowBitsSub(value1, value2, res);
        uint overflowBits = OverflowBitsSub(value1, value2, res);
        _state.CarryFlag = (borrowBits >> 7 & 1) == 1;
        _state.AuxiliaryFlag = (borrowBits >> 3 & 1) == 1;
        _state.OverflowFlag = (overflowBits >> 7 & 1) == 1;
        return res;
    }

    /// <summary>
    /// Performs the xor operation.
    /// </summary>
    /// <param name="value1">The value 1.</param>
    /// <param name="value2">The value 2.</param>
    /// <returns>The result of the operation.</returns>
    public override byte Xor(byte value1, byte value2) {
        byte res = (byte)(value1 ^ value2);
        UpdateFlags(res);
        _state.CarryFlag = false;
        _state.OverflowFlag = false;
        return res;
    }

    private void SetOverflowForRigthRotate8(byte res) {
        bool msb = (res & MsbMask) != 0;
        bool beforeMsb = (res & BeforeMsbMask) != 0;
        _state.OverflowFlag = msb ^ beforeMsb;
    }

    /// <summary>
    /// Sets sign flag.
    /// </summary>
    /// <param name="value">The value.</param>
    protected override void SetSignFlag(byte value) {
        _state.SignFlag = (value & MsbMask) != 0;
    }
}