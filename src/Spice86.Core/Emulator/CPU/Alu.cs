﻿namespace Spice86.Core.Emulator.CPU;

using Spice86.Core.Emulator.CPU.Exceptions;

/// <summary>
/// Arithmetic-logic unit
/// </summary>
public class Alu {
    /// <summary>
    /// Shifting this by the number we want to test gives 1 if number of bit is even and 0 if odd.<br/>
    /// Hardcoded numbers:<br/>
    /// 0 -> 0000: even -> 1<br/>
    /// 1 -> 0001: 1 bit so odd -> 0<br/>
    /// 2 -> 0010: 1 bit so odd -> 0<br/>
    /// 3 -> 0011: 2 bit so even -> 1<br/>
    /// 4 -> 0100: 1 bit so odd -> 0<br/>
    /// 5 -> 0101: even -> 1<br/>
    /// 6 -> 0110: even -> 1<br/>
    /// 7 -> 0111: odd -> 0<br/>
    /// 8 -> 1000: odd -> 0<br/>
    /// 9 -> 1001: even -> 1<br/>
    /// A -> 1010: even -> 1<br/>
    /// B -> 1011: odd -> 0<br/>
    /// C -> 1100: even -> 1<br/>
    /// D -> 1101: odd -> 0<br/>
    /// E -> 1110: odd -> 0<br/>
    /// F -> 1111: even -> 1<br/>
    /// => lookup table is 1001011001101001
    /// </summary>
    private const uint FourBitParityTable = 0b1001011001101001;

    private const uint BeforeMsbMask32 = 0x40000000;

    private const ushort BeforeMsbMask16 = 0x4000;

    private const byte BeforeMsbMask8 = 0x40;

    private const uint MsbMask32 = 0x80000000;

    private const ushort MsbMask16 = 0x8000;

    private const byte MsbMask8 = 0x80;

    private const int ShiftCountMask = 0x1F;

    private readonly ICpuState _state;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="state">The state of the CPU.</param>
    public Alu(ICpuState state) {
        _state = state;
    }

    public uint Adc32(uint value1, uint value2) {
        return Add32(value1, value2, true);
    }

    public ushort Adc16(ushort value1, ushort value2) {
        return Add16(value1, value2, true);
    }

    public byte Adc8(byte value1, byte value2) {
        return Add8(value1, value2, true);
    }

    public uint Add32(uint value1, uint value2) {
        return Add32(value1, value2, false);
    }

    public uint Add32(uint value1, uint value2, bool useCarry) {
        int carry = useCarry && _state.CarryFlag ? 1 : 0;
        uint res = (uint)(value1 + value2 + carry);
        UpdateFlags32(res);
        uint carryBits = CarryBitsAdd(value1, value2, res);
        uint overflowBits = OverflowBitsAdd(value1, value2, res);
        _state.CarryFlag = (carryBits >> 31 & 1) == 1;
        _state.AuxiliaryFlag = (carryBits >> 3 & 1) == 1;
        _state.OverflowFlag = (overflowBits >> 31 & 1) == 1;
        return res;
    }

    public ushort Add16(ushort value1, ushort value2, bool useCarry) {
        int carry = useCarry && _state.CarryFlag ? 1 : 0;
        ushort res = (ushort)(value1 + value2 + carry);
        UpdateFlags16(res);
        uint carryBits = CarryBitsAdd(value1, value2, res);
        uint overflowBits = OverflowBitsAdd(value1, value2, res);
        _state.CarryFlag = (carryBits >> 15 & 1) == 1;
        _state.AuxiliaryFlag = (carryBits >> 3 & 1) == 1;
        _state.OverflowFlag = (overflowBits >> 15 & 1) == 1;
        return res;
    }

    public ushort Add16(ushort value1, ushort value2) {
        return Add16(value1, value2, false);
    }

    public byte Add8(byte value1, byte value2, bool useCarry) {
        int carry = useCarry && _state.CarryFlag ? 1 : 0;
        byte res = (byte)(value1 + value2 + carry);
        UpdateFlags8(res);
        uint carryBits = CarryBitsAdd(value1, value2, res);
        uint overflowBits = OverflowBitsAdd(value1, value2, res);
        _state.CarryFlag = (carryBits >> 7 & 1) == 1;
        _state.AuxiliaryFlag = (carryBits >> 3 & 1) == 1;
        _state.OverflowFlag = (overflowBits >> 7 & 1) == 1;
        return res;
    }

    public byte Add8(byte value1, byte value2) {
        return Add8(value1, value2, false);
    }

    public uint And32(uint value1, uint value2) {
        uint res = value1 & value2;
        UpdateFlags32(res);
        _state.CarryFlag = false;
        _state.OverflowFlag = false;
        return res;
    }

    public ushort And16(ushort value1, ushort value2) {
        ushort res = (ushort)(value1 & value2);
        UpdateFlags16(res);
        _state.CarryFlag = false;
        _state.OverflowFlag = false;
        return res;
    }

    public byte And8(byte value1, byte value2) {
        byte res = (byte)(value1 & value2);
        UpdateFlags8(res);
        _state.CarryFlag = false;
        _state.OverflowFlag = false;
        return res;
    }

    public uint Dec32(uint value1) {
        bool carry = _state.CarryFlag;
        uint res = Sub32(value1, 1, false);
        _state.CarryFlag = carry;
        return res;
    }

    public ushort Dec16(ushort value1) {
        bool carry = _state.CarryFlag;
        ushort res = Sub16(value1, 1, false);
        _state.CarryFlag = carry;
        return res;
    }

    public byte Dec8(byte value1) {
        bool carry = _state.CarryFlag;
        byte res = Sub8(value1, 1, false);
        _state.CarryFlag = carry;
        return res;
    }

    public uint Div32(ulong value1, uint value2) {
        if (value2 == 0) {
            throw new CpuDivisionErrorException($"Division by zero");
        }

        ulong res = value1 / value2;
        if (res > uint.MaxValue) {
            throw new CpuDivisionErrorException($"Division result out of range: {res}");
        }

        return (uint)res;
    }

    public ushort Div16(uint value1, ushort value2) {
        if (value2 == 0) {
            throw new CpuDivisionErrorException($"Division by zero");
        }

        uint res = value1 / value2;
        if (res > ushort.MaxValue) {
            throw new CpuDivisionErrorException($"Division result out of range: {res}");
        }

        return (ushort)res;
    }

    public byte Div8(ushort value1, byte value2) {
        if (value2 == 0) {
            throw new CpuDivisionErrorException($"Division by zero");
        }

        uint res = (uint)(value1 / value2);
        if (res > byte.MaxValue) {
            throw new CpuDivisionErrorException($"Division result out of range: {res}");
        }

        return (byte)res;
    }
    
    public int Idiv32(long value1, int value2) {
        if (value2 == 0) {
            throw new CpuDivisionErrorException($"Division by zero");
        }

        long res = value1 / value2;
        unchecked {
            if (res is > 0x7FFFFFFF or < (int)0x80000000) {
                throw new CpuDivisionErrorException($"Division result out of range: {res}");
            }
        }

        return (int)res;
    }

    public short Idiv16(int value1, short value2) {
        if (value2 == 0) {
            throw new CpuDivisionErrorException($"Division by zero");
        }

        int res = value1 / value2;
        unchecked {
            if (res is > 0x7FFF or < (short)0x8000) {
                throw new CpuDivisionErrorException($"Division result out of range: {res}");
            }
        }

        return (short)res;
    }

    public sbyte Idiv8(short value1, sbyte value2) {
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

    public long Imul32(int value1, int value2) {
        long res = (long)value1 * value2;
        bool doesNotFitInDWord = res != (int)res;
        _state.OverflowFlag = doesNotFitInDWord;
        _state.CarryFlag = doesNotFitInDWord;
        return res;
    }
    
    public int Imul16(short value1, short value2) {
        int res = value1 * value2;
        bool doesNotFitInWord = res != (short)res;
        _state.OverflowFlag = doesNotFitInWord;
        _state.CarryFlag = doesNotFitInWord;
        return res;
    }

    public short Imul8(sbyte value1, sbyte value2) {
        int res = value1 * value2;
        bool doesNotFitInByte = res != (sbyte)res;
        _state.OverflowFlag = doesNotFitInByte;
        _state.CarryFlag = doesNotFitInByte;
        return (short)res;
    }

    public uint Inc32(uint value) {
        // CF is not modified
        bool carry = _state.CarryFlag;
        uint res = Add32(value, 1, false);
        _state.CarryFlag = carry;
        return res;
    }

    public ushort Inc16(ushort value) {
        // CF is not modified
        bool carry = _state.CarryFlag;
        ushort res = Add16(value, 1, false);
        _state.CarryFlag = carry;
        return res;
    }

    public byte Inc8(byte value) {
        // CF is not modified
        bool carry = _state.CarryFlag;
        byte res = Add8(value, 1, false);
        _state.CarryFlag = carry;
        return res;
    }

    public ulong Mul32(uint value1, uint value2) {
        ulong res = (ulong)value1 * value2;
        bool upperHalfNonZero = (res & 0xFFFFFFFF00000000) != 0;
        _state.OverflowFlag = upperHalfNonZero;
        _state.CarryFlag = upperHalfNonZero;
        SetZeroFlag(res);
        SetParityFlag(res);
        SetSignFlag32((uint)res);
        return res;
    }

    public uint Mul16(ushort value1, ushort value2) {
        uint res = (uint) (value1 * value2);
        bool upperHalfNonZero = (res & 0xFFFF0000) != 0;
        _state.OverflowFlag = upperHalfNonZero;
        _state.CarryFlag = upperHalfNonZero;
        SetZeroFlag(res);
        SetParityFlag(res);
        SetSignFlag16((ushort)res);
        return res;
    }

    public ushort Mul8(byte value1, byte value2) {
        ushort res = (ushort)(value1 * value2);
        bool upperHalfNonZero = (res & 0xFF00) != 0;
        _state.OverflowFlag = upperHalfNonZero;
        _state.CarryFlag = upperHalfNonZero;
        SetZeroFlag(res);
        SetParityFlag(res);
        SetSignFlag8((byte)res);
        return res;
    }

    public uint Or32(uint value1, uint value2) {
        uint res = value1 | value2;
        UpdateFlags32(res);
        _state.CarryFlag = false;
        _state.OverflowFlag = false;
        return res;
    }

    public ushort Or16(ushort value1, ushort value2) {
        ushort res = (ushort)(value1 | value2);
        UpdateFlags16(res);
        _state.CarryFlag = false;
        _state.OverflowFlag = false;
        return res;
    }

    public byte Or8(byte value1, byte value2) {
        byte res = (byte)(value1 | value2);
        UpdateFlags8(res);
        _state.CarryFlag = false;
        _state.OverflowFlag = false;
        return res;
    }

    public uint Rcl32(uint value, byte count) {
        count = (byte) ((count & ShiftCountMask) % 33);
        if (count == 0) {
            return value;
        }

        uint carry = value >> 32 - count & 0x1;
        uint res = value << count;
        int mask = (1 << count - 1) - 1;
        res = (uint)(res | (value >> 33 - count & mask));
        if (_state.CarryFlag) {
            res = (uint)(res | 1 << count - 1);
        }

        _state.CarryFlag = carry != 0;
        bool msb = (res & MsbMask32) != 0;
        _state.OverflowFlag = msb ^ _state.CarryFlag;
        return res;
    }

    public ushort Rcl16(ushort value, byte count) {
        count = (byte) ((count & ShiftCountMask) % 17);
        if (count == 0) {
            return value;
        }

        int carry = value >> 16 - count & 0x1;
        ushort res = (ushort)(value << count);
        int mask = (1 << count - 1) - 1;
        res = (ushort)(res | (value >> 17 - count & mask));
        if (_state.CarryFlag) {
            res = (ushort)(res | 1 << count - 1);
        }

        _state.CarryFlag = carry != 0;
        bool msb = (res & MsbMask16) != 0;
        _state.OverflowFlag = msb ^ _state.CarryFlag;
        return res;
    }

    public byte Rcl8(byte value, byte count) {
        count = (byte) ((count & ShiftCountMask) % 9);
        if (count == 0) {
            return value;
        }

        int carry = value >> 8 - count & 0x1;
        byte res = (byte)(value << count);
        int mask = (1 << count - 1) - 1;
        res = (byte)(res | (value >> 9 - count & mask));
        if (_state.CarryFlag) {
            res = (byte)(res | 1 << count - 1);
        }

        _state.CarryFlag = carry != 0;
        bool msb = (res & MsbMask8) != 0;
        _state.OverflowFlag = msb ^ _state.CarryFlag;
        return res;
    }

    public uint Rcr32(uint value, int count) {
        count = (count & ShiftCountMask) % 33;
        if (count == 0) {
            return value;
        }

        uint carry = value >> count - 1 & 0x1;
        int mask = (1 << 32 - count) - 1;
        uint res = (uint) (value >> count & mask);
        res |= value << 33 - count;
        if (_state.CarryFlag) {
            res = (ushort)(res | 1 << 32 - count);
        }

        _state.CarryFlag = carry != 0;
        SetOverflowForRigthRotate32(res);
        return res;
    }

    public ushort Rcr16(ushort value, int count) {
        count = (count & ShiftCountMask) % 17;
        if (count == 0) {
            return value;
        }

        int carry = value >> count - 1 & 0x1;
        int mask = (1 << 16 - count) - 1;
        ushort res = (ushort)(value >> count & mask);
        res = (ushort)(res | value << 17 - count);
        if (_state.CarryFlag) {
            res = (ushort)(res | 1 << 16 - count);
        }

        _state.CarryFlag = carry != 0;
        SetOverflowForRigthRotate16(res);
        return res;
    }

    public byte Rcr8(byte value, int count) {
        count = (count & ShiftCountMask) % 9;
        if (count == 0) {
            return value;
        }

        int carry = value >> count - 1 & 0x1;
        int mask = (1 << 8 - count) - 1;
        byte res = (byte)(value >> count & mask);
        res = (byte)(res | value << 9 - count);
        if (_state.CarryFlag) {
            res = (byte)(res | 1 << 8 - count);
        }

        _state.CarryFlag = carry != 0;
        SetOverflowForRigthRotate8(res);
        return res;
    }

    public uint Rol32(uint value, byte count) {
        count = (byte) ((count & ShiftCountMask) % 32);
        if (count == 0) {
            return value;
        }

        uint carry = value >> 32 - count & 0x1;
        uint res = value << count;
        res |= value >> 32 - count;
        _state.CarryFlag = carry != 0;
        bool msb = (res & MsbMask32) != 0;
        _state.OverflowFlag = msb ^ _state.CarryFlag;
        return res;
    }

    public ushort Rol16(ushort value, byte count) {
        count = (byte) ((count & ShiftCountMask) % 16);
        if (count == 0) {
            return value;
        }

        int carry = value >> 16 - count & 0x1;
        ushort res = (ushort)(value << count);
        res = (ushort)(res | value >> 16 - count);
        _state.CarryFlag = carry != 0;
        bool msb = (res & MsbMask16) != 0;
        _state.OverflowFlag = msb ^ _state.CarryFlag;
        return res;
    }

    public byte Rol8(byte value, byte count) {
        count = (byte) ((count & ShiftCountMask) % 8);
        if (count == 0) {
            return value;
        }

        int carry = value >> 8 - count & 0x1;
        byte res = (byte)(value << count);
        res = (byte)(res | value >> 8 - count);
        _state.CarryFlag = carry != 0;
        bool msb = (res & MsbMask8) != 0;
        _state.OverflowFlag = msb ^ _state.CarryFlag;
        return res;
    }

    public uint Ror32(uint value, int count) {
        count = (count & ShiftCountMask) % 16;
        if (count == 0) {
            return value;
        }

        uint carry = value >> count - 1 & 0x1;
        int mask = (1 << 32 - count) - 1;
        uint res = (uint)(value >> count & mask);
        res |= value << 32 - count;
        _state.CarryFlag = carry != 0;
        SetOverflowForRigthRotate32(res);
        return res;
    }

    public ushort Ror16(ushort value, int count) {
        count = (count & ShiftCountMask) % 16;
        if (count == 0) {
            return value;
        }

        int carry = value >> count - 1 & 0x1;
        int mask = (1 << 16 - count) - 1;
        ushort res = (ushort)(value >> count & mask);
        res = (ushort)(res | value << 16 - count);
        _state.CarryFlag = carry != 0;
        SetOverflowForRigthRotate16(res);
        return res;
    }

    public byte Ror8(byte value, int count) {
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

    public uint Sar32(uint value, int count) {
        count &= ShiftCountMask;
        if (count == 0) {
            return value;
        }

        int res = (int)value;
        SetCarryFlagForRightShifts((uint)res, count);
        res >>= count;
        UpdateFlags32((uint)res);
        _state.OverflowFlag = false;
        return (uint)res;
    }

    public ushort Sar16(ushort value, int count) {
        count &= ShiftCountMask;
        if (count == 0) {
            return value;
        }

        short res = (short)value;
        SetCarryFlagForRightShifts((uint)res, count);
        res >>= count;
        UpdateFlags16((ushort)res);
        _state.OverflowFlag = false;
        return (ushort)res;
    }

    public byte Sar8(byte value, int count) {
        count &= ShiftCountMask;
        if (count == 0) {
            return value;
        }

        sbyte res = (sbyte)value;
        SetCarryFlagForRightShifts((uint)res, count);
        res >>= count;
        UpdateFlags8((byte)res);
        _state.OverflowFlag = false;
        return (byte)res;
    }

    public uint Sbb32(uint value1, uint value2) {
        return Sub32(value1, value2, true);
    }

    public ushort Sbb16(ushort value1, ushort value2) {
        return Sub16(value1, value2, true);
    }

    public byte Sbb8(byte value1, byte value2) {
        return Sub8(value1, value2, true);
    }

    public uint Shl32(uint value, int count) {
        count &= ShiftCountMask;
        if (count == 0) {
            return value;
        }

        uint msbBefore = (value << (count - 1)) & MsbMask32;
        _state.CarryFlag = msbBefore != 0;
        uint res = value << count;
        UpdateFlags32(res);
        uint msb = res & MsbMask32;
        _state.OverflowFlag = (msb ^ msbBefore) != 0;
        return res;
    }

    public ushort Shl16(ushort value, int count) {
        count &= ShiftCountMask;
        if (count == 0) {
            return value;
        }

        int msbBefore = value << count - 1 & MsbMask16;
        _state.CarryFlag = msbBefore != 0;
        ushort res = (ushort)(value << count);
        UpdateFlags16(res);
        ushort msb = (ushort) (res & MsbMask16);
        _state.OverflowFlag = (msb ^ msbBefore) != 0;
        return res;
    }

    public byte Shl8(byte value, int count) {
        count &= ShiftCountMask;
        if (count == 0) {
            return value;
        }

        int msbBefore = value << count - 1 & MsbMask8;
        _state.CarryFlag = msbBefore != 0;
        byte res = (byte)(value << count);
        UpdateFlags8(res);
        int msb = res & MsbMask8;
        _state.OverflowFlag = (msb ^ msbBefore) != 0;
        return res;
    }

    public uint Shr32(uint value, int count) {
        count &= ShiftCountMask;
        if (count == 0) {
            return value;
        }

        uint msb = value & MsbMask32;
        _state.OverflowFlag = msb != 0;
        SetCarryFlagForRightShifts(value, count);
        uint res = value >> count;
        UpdateFlags32(res);
        return res;
    }

    public ushort Shr16(ushort value, int count) {
        count &= ShiftCountMask;
        if (count == 0) {
            return value;
        }

        ushort msb = (ushort)(value & MsbMask16);
        _state.OverflowFlag = msb != 0;
        SetCarryFlagForRightShifts(value, count);
        ushort res = (ushort)(value >> count);
        UpdateFlags16(res);
        return res;
    }

    public byte Shr8(byte value, int count) {
        count &= ShiftCountMask;
        if (count == 0) {
            return value;
        }

        int msb = value & MsbMask8;
        _state.OverflowFlag = msb != 0;
        SetCarryFlagForRightShifts(value, count);
        byte res = (byte)(value >> count);
        UpdateFlags8(res);
        return res;
    }

    public uint Sub32(uint value1, uint value2) {
        return Sub32(value1, value2, false);
    }

    public uint Sub32(uint value1, uint value2, bool useCarry) {
        int carry = (useCarry && _state.CarryFlag) ? 1 : 0;
        uint res = (uint)(value1 - value2 - carry);
        UpdateFlags32(res);
        uint borrowBits = BorrowBitsSub(value1, value2, res);
        uint overflowBits = OverflowBitsSub(value1, value2, res);
        _state.CarryFlag = ((borrowBits >> 31) & 1) == 1;
        _state.AuxiliaryFlag = ((borrowBits >> 3) & 1) == 1;
        _state.OverflowFlag = ((overflowBits >> 31) & 1) == 1;
        return res;
    }

    public ushort Sub16(ushort value1, ushort value2) {
        return Sub16(value1, value2, false);
    }

    public ushort Sub16(ushort value1, ushort value2, bool useCarry) {
        int carry = useCarry && _state.CarryFlag ? 1 : 0;
        ushort res = (ushort)(value1 - value2 - carry);
        UpdateFlags16(res);
        uint borrowBits = BorrowBitsSub(value1, value2, res);
        uint overflowBits = OverflowBitsSub(value1, value2, res);
        _state.CarryFlag = (borrowBits >> 15 & 1) == 1;
        _state.AuxiliaryFlag = (borrowBits >> 3 & 1) == 1;
        _state.OverflowFlag = (overflowBits >> 15 & 1) == 1;
        return res;
    }

    public byte Sub8(byte value1, byte value2) {
        return Sub8(value1, value2, false);
    }

    public byte Sub8(byte value1, byte value2, bool useCarry) {
        int carry = useCarry && _state.CarryFlag ? 1 : 0;
        byte res = (byte)(value1 - value2 - carry);
        UpdateFlags8(res);
        uint borrowBits = BorrowBitsSub(value1, value2, res);
        uint overflowBits = OverflowBitsSub(value1, value2, res);
        _state.CarryFlag = (borrowBits >> 7 & 1) == 1;
        _state.AuxiliaryFlag = (borrowBits >> 3 & 1) == 1;
        _state.OverflowFlag = (overflowBits >> 7 & 1) == 1;
        return res;
    }

    public void UpdateFlags32(uint value) {
        SetZeroFlag(value);
        SetParityFlag(value);
        SetSignFlag32(value);
    }

    public void UpdateFlags16(ushort value) {
        SetZeroFlag(value);
        SetParityFlag(value);
        SetSignFlag16(value);
    }

    public void UpdateFlags8(byte value) {
        SetZeroFlag(value);
        SetParityFlag(value);
        SetSignFlag8(value);
    }

    public uint Xor32(uint value1, uint value2) {
        uint res = value1 ^ value2;
        UpdateFlags32(res);
        _state.CarryFlag = false;
        _state.OverflowFlag = false;
        return res;
    }

    public ushort Xor16(ushort value1, ushort value2) {
        ushort res = (ushort)(value1 ^ value2);
        UpdateFlags16(res);
        _state.CarryFlag = false;
        _state.OverflowFlag = false;
        return res;
    }

    public byte Xor8(byte value1, byte value2) {
        byte res = (byte)(value1 ^ value2);
        UpdateFlags8(res);
        _state.CarryFlag = false;
        _state.OverflowFlag = false;
        return res;
    }

    private static uint BorrowBitsSub(uint value1, uint value2, uint dst) {
        return value1 ^ value2 ^ dst ^ ((value1 ^ dst) & (value1 ^ value2));
    }

    private static uint CarryBitsAdd(uint value1, uint value2, uint dst) {
        return value1 ^ value2 ^ dst ^ ((value1 ^ dst) & ~(value1 ^ value2));
    }

    private static bool IsParity(byte value) {
        int low4 = value & 0xF;
        int high4 = value >> 4 & 0xF;
        return (FourBitParityTable >> low4 & 1) == (FourBitParityTable >> high4 & 1);
    }

    // from https://www.vogons.org/viewtopic.php?t=55377
    private static uint OverflowBitsAdd(uint value1, uint value2, uint dst) {
        return (value1 ^ dst) & ~(value1 ^ value2);
    }

    private static uint OverflowBitsSub(uint value1, uint value2, uint dst) {
        return (value1 ^ dst) & (value1 ^ value2);
    }

    private void SetCarryFlagForRightShifts(uint value, int count) {
        uint lastBit = value >> count - 1 & 0x1;
        _state.CarryFlag = lastBit == 1;
    }

    private void SetOverflowForRigthRotate32(uint res) {
        bool msb = (res & MsbMask32) != 0;
        bool beforeMsb = (res & BeforeMsbMask32) != 0;
        _state.OverflowFlag = msb ^ beforeMsb;
    }

    private void SetOverflowForRigthRotate16(ushort res) {
        bool msb = (res & MsbMask16) != 0;
        bool beforeMsb = (res & BeforeMsbMask16) != 0;
        _state.OverflowFlag = msb ^ beforeMsb;
    }

    private void SetOverflowForRigthRotate8(byte res) {
        bool msb = (res & MsbMask8) != 0;
        bool beforeMsb = (res & BeforeMsbMask8) != 0;
        _state.OverflowFlag = msb ^ beforeMsb;
    }

    private void SetParityFlag(ulong value) {
        _state.ParityFlag = IsParity((byte)value);
    }

    private void SetSignFlag32(uint value) {
        _state.SignFlag = (value & MsbMask32) != 0;
    }

    private void SetSignFlag16(ushort value) {
        _state.SignFlag = (value & MsbMask16) != 0;
    }

    private void SetSignFlag8(byte value) {
        _state.SignFlag = (value & MsbMask8) != 0;
    }

    private void SetZeroFlag(ulong value) {
        _state.ZeroFlag = value == 0;
    }

    public ushort Shld16(ushort destination, ushort source, byte count) {
        count &= ShiftCountMask;
        if (count == 0) {
            return destination;
        }

        if (count > 16) {
            // Undefined. We shift the source in again.
            return (ushort)(source << (count - 16));
        }

        ushort msbBefore = (ushort)(destination & MsbMask16);
        _state.CarryFlag = (destination >> (16 - count) & 1) != 0;
        ushort res = (ushort)((destination << count) | (source >> (16 - count)));
        UpdateFlags16(res);
        ushort msb = (ushort)(res & MsbMask16);
        _state.OverflowFlag = msb != msbBefore;
        return res;
    }

    public uint Shld32(uint destination, uint source, byte count) {
        count &= ShiftCountMask;
        if (count == 0) {
            return destination;
        }

        uint msbBefore = destination & MsbMask32;
        _state.CarryFlag = (destination >> (32 - count) & 1) != 0;
        uint res = (destination << count) | (source >> (32 - count));
        UpdateFlags32(res);
        uint msb = res & MsbMask32;
        _state.OverflowFlag = msb != msbBefore;
        return res;
    }
}
