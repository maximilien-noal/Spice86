using Spice86.Core.Emulator.Errors;
using Spice86.Core.Emulator.VM;

namespace Spice86.Core.Emulator.CPU.InstructionsImpl;

/// <summary>
/// The class.
/// </summary>
public abstract class Instructions16Or32 : Instructions {
    protected Instructions16Or32(Cpu cpu, Memory.IMemory memory, ModRM modRm)
        : base(cpu, memory, modRm) {
    }

    // Inc Reg
    /// <summary>
    /// void method.
    /// </summary>
    public abstract void IncReg(int regIndex);

    // Dec Reg
    /// <summary>
    /// void method.
    /// </summary>
    public abstract void DecReg(int regIndex);

    // Push Reg
    /// <summary>
    /// void method.
    /// </summary>
    public abstract void PushReg(int regIndex);

    // Pop Reg
    /// <summary>
    /// void method.
    /// </summary>
    public abstract void PopReg(int regIndex);

    // Pusha
    /// <summary>
    /// void method.
    /// </summary>
    public abstract void Pusha();

    // Popa
    /// <summary>
    /// void method.
    /// </summary>
    public abstract void Popa();

    // Push immediate value
    /// <summary>
    /// void method.
    /// </summary>
    public abstract void PushImm();

    // Push Sign extended 8bit immediate value
    /// <summary>
    /// void method.
    /// </summary>
    public abstract void PushImm8SignExtended();

    // IMUL R <- Rm x Imm8
    /// <summary>
    /// void method.
    /// </summary>
    public abstract void ImulRmImm8();

    // IMUL R <- Rm x Imm16 / 32
    /// <summary>
    /// void method.
    /// </summary>
    public abstract void ImulRmImm16Or32();

    /// <summary>
    /// void method.
    /// </summary>
    public abstract void ImulRmReg16Or32();

    /// <summary>
    /// void method.
    /// </summary>
    public abstract void MovRmSreg();
    /// <summary>
    /// void method.
    /// </summary>
    public abstract void Lea();

    /// <summary>
    /// ExtractLeaMemoryOffset16 method.
    /// </summary>
    public ushort ExtractLeaMemoryOffset16() {
        ModRM.Read();
        ushort? memoryOffset = ModRM.MemoryOffset;
        if (memoryOffset == null) {
            throw new InvalidVMOperationException(State,
                "Memory address was not read by Mod R/M but it is needed for LEA");
        }

        return (ushort)memoryOffset;
    }

    /// <summary>
    /// void method.
    /// </summary>
    public abstract void PopRm();

    /// <summary>
    /// void method.
    /// </summary>
    public abstract void XchgAcc(int regIndex);

    /// <summary>
    /// void method.
    /// </summary>
    public abstract void Cbw();
    /// <summary>
    /// void method.
    /// </summary>
    public abstract void Cwd();

    /// <summary>
    /// void method.
    /// </summary>
    public abstract void Pushf();

    /// <summary>
    /// void method.
    /// </summary>
    public abstract void Popf();

    /// <summary>
    /// void method.
    /// </summary>
    public abstract void Grp1(bool signExtendOp2);

    /// <summary>
    /// Grp5 method.
    /// </summary>
    public void Grp5() {
        ModRM.Read();
        uint groupIndex = ModRM.RegisterIndex;
        switch (groupIndex) {
            case 0:
                Grp45RmInc();
                break;
            case 1:
                Grp45RmDec();
                break;
            case 2:
                Grp5RmCallNear();
                break;
            case 3:
                Grp5RmCallFar();
                break;
            case 4:
                Grp5RmJumpNear();
                break;
            case 5:
                Grp5RmJumpFar();
                break;
            case 6:
                Grp5RmPush();
                break;
            default:
                throw new InvalidGroupIndexException(State, groupIndex);
        }
    }

    private void Grp5RmCallNear() {
        // NEAR CALL
        ushort callAddress = ModRM.GetRm16();
        Cpu.NearCallWithReturnIpNextInstruction(callAddress);
    }

    private void Grp5RmCallFar() {
        // FAR CALL
        uint? ipAddress = ModRM.MemoryAddress;
        if (ipAddress is null) {
            return;
        }

        (ushort cs, ushort ip) = Memory.SegmentedAddress16[ipAddress.Value];
        Cpu.FarCallWithReturnIpNextInstruction(cs, ip);
    }

    private void Grp5RmJumpNear() {
        ushort ip = ModRM.GetRm16();
        Cpu.JumpNear(ip);
    }

    private void Grp5RmJumpFar() {
        uint? ipAddress = ModRM.MemoryAddress;
        if (ipAddress is null) {
            return;
        }
        (ushort cs, ushort ip) = Memory.SegmentedAddress16[ipAddress.Value];
        Cpu.JumpFar(cs, ip);
    }

    protected abstract void Grp5RmPush();

    protected abstract ushort DoLxsAndReturnSegmentValue();

    protected uint ReadLxsMemoryAddress() {
        // Copy segmented address that is in memory (32bits) into DS/ES and the
        // specified register
        ModRM.Read();
        uint? memoryAddress = ModRM.MemoryAddress;
        if (memoryAddress == null) {
            throw new InvalidVMOperationException(State,
                "Memory address was not read by Mod R/M but it is needed for LES / LDS");
        }

        return (uint)memoryAddress;
    }

    /// <summary>
    /// Lds method.
    /// </summary>
    public void Lds() {
        State.DS = DoLxsAndReturnSegmentValue();
    }

    /// <summary>
    /// Les method.
    /// </summary>
    public void Les() {
        State.ES = DoLxsAndReturnSegmentValue();
    }

    /// <summary>
    /// Lfs method.
    /// </summary>
    public void Lfs() {
        State.FS = DoLxsAndReturnSegmentValue();
    }

    /// <summary>
    /// Lgs method.
    /// </summary>
    public void Lgs() {
        State.GS = DoLxsAndReturnSegmentValue();
    }

    /// <summary>
    /// https://xem.github.io/minix86/manual/intel-x86-and-64-manual-vol1/o_7281d5ea06a5b67a-159.html
    /// </summary>
    public abstract void Enter();

    /// <summary>
    /// void method.
    /// </summary>
    public abstract void Leave();

    /// <summary>
    /// void method.
    /// </summary>
    public abstract void Shld(Grp2CountSource countSource);

    /// <summary>
    /// void method.
    /// </summary>
    public abstract void MovzxByte();

    /// <summary>
    /// void method.
    /// </summary>
    public abstract void MovsxByte();
}