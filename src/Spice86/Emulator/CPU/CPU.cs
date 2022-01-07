﻿namespace Spice86.Emulator.Cpu;

using Serilog;

using Spice86.Emulator.Callback;
using Spice86.Emulator.Errors;
using Spice86.Emulator.Function;
using Spice86.Emulator.IOPorts;
using Spice86.Emulator.Machine;
using Spice86.Emulator.Memory;
using Spice86.Utils;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Implementation of a 8086 CPU. <br /> It has some 80186, 80286 and 80386 instructions as some
/// program use them. <br /> It also has some x87 FPU instructions to support telling the programs
/// that x87 is not supported :) <br /> Some docs that helped the implementation: <ul> <li>
/// Instructions decoding: http://rubbermallet.org/8086%20notes.pdf and
/// http://ref.x86asm.net/coder32.html </li><li> Instructions implementation details:
/// https://www.felixcloutier.com/x86/ </li><li> Pure 8086 instructions:
/// https://jbwyatt.com/253/emu/8086_instruction_set.html </li></ul>
/// TODO: Complete it !
/// </summary>
public class Cpu
{

    private static readonly ILogger _logger = Log.Logger.ForContext<Cpu>();

    // Extract regIndex from opcode
    private const int REG_INDEX_MASK = 0b111;
    private static readonly List<int> PREFIXES_OPCODES = new int[] { 0x26, 0x2E, 0x36, 0x3E, 0x64, 0x65, 0xF0, 0xF2, 0xF3 }.ToList();
    private static readonly List<int> STRING_OPCODES = new int[] { 0xA4, 0xA5, 0xA6, 0xA7, 0xAA, 0xAB, 0xAC, 0xAD, 0xAE, 0xAF, 0x6C, 0x6D, 0x6E, 0x6F }.ToList();

    private Machine _machine;
    private State _state;
    private Memory _memory;
    private ModRM _modRM;
    private Alu _alu;
    private Stack _stack;
    private CallbackHandler _callbackHandler;
    private IOPortDispatcher _ioPortDispatcher;
    private FunctionHandler _functionHandler;
    private FunctionHandler _functionHandlerInExternalInterrupt;
    private FunctionHandler _functionHandlerInUse;
    private StaticAddressesRecorder _staticAddressesRecorder;
    // Value used to read parts of the instruction.
    // CPU uses this internally and adjusts IP after instruction execution is done.
    private int _internalIp;
    private bool _running = true;
    // interrupt not generated by the code
    private int? _externalInterruptVectorNumber;
    // When true will crash if an interrupt targets code at 0000:0000
    private bool _errorOnUninitializedInterruptHandler;
    private bool? _forceLog;

    public Cpu(Machine machine, bool debugMode)
    {
        _machine = machine;
        _memory = machine.GetMemory();
        _state = new State();
        _alu = new Alu(_state);
        _stack = new Stack(_memory, _state);
        _functionHandler = new FunctionHandler(machine, debugMode);
        _functionHandlerInExternalInterrupt = new FunctionHandler(machine, debugMode);
        _functionHandlerInUse = _functionHandler;
        _staticAddressesRecorder = new StaticAddressesRecorder(_state, debugMode);
        _modRM = new ModRM(machine, this);
    }

    public State GetState() => _state;

    public Alu GetAlu() => _alu;
    public Stack GetStack() => _stack;

    public bool IsRunning => _running;

    public void SetRunning(bool running) => _running = running;

    public void SetCallbackHandler(CallbackHandler callbackHandler) => _callbackHandler = callbackHandler;

    public void SetIoPortDispatcher(IOPortDispatcher iOPortDispatcher) => _ioPortDispatcher = iOPortDispatcher;

    public FunctionHandler GetFunctionHandler() => _functionHandler;

    public FunctionHandler GetFunctionHandlerInExternalInterrupt() => _functionHandlerInExternalInterrupt;

    public FunctionHandler GetFunctionHandlerInUse() => _functionHandlerInUse;

    public StaticAddressesRecorder GetStaticAddresRecorder => _staticAddressesRecorder;

    public void SetForceLog(bool forceLog) => _forceLog = forceLog;

    public void ExternalInterrupt(int vectorNumber)
    {
        // hack: do not let the timer overwrite keyboard.
        if (_externalInterruptVectorNumber == null || _externalInterruptVectorNumber != 9)
        {
            _externalInterruptVectorNumber = vectorNumber;
        }
    }

    public void SetErrorOnUninitializedInterruptHandler(bool errorOnUninitializedInterruptHandler)
    {
        _errorOnUninitializedInterruptHandler = errorOnUninitializedInterruptHandler;
    }

    private void AddCurrentInstructionPrefix(Func<String> getLog)
    {
        // Optimization, do not calculate the log if it is not used
        if (IsLoggingEnabled())
        {
            _state.AddCurrentInstructionPrefix(getLog.Invoke());
        }
    }

    private void SetCurrentInstructionName(Func<String> getLog)
    {
        // Optimization, do not calculate the log if it is not used
        if (IsLoggingEnabled())
        {
            _state.SetCurrentInstructionName(getLog.Invoke());
        }
    }

    private bool IsLoggingEnabled()
    {
        if (_forceLog == null)
        {
            return _logger.IsEnabled(Serilog.Events.LogEventLevel.Debug);
        }
        return _forceLog.Value;
    }

    public void ExecuteNextInstruction()
    {
        _internalIp = _state.GetIP();
        _staticAddressesRecorder.Reset();
        var stateString = "";
        if (IsLoggingEnabled())
        {
            stateString = _state.ToString();
            _state.ResetCurrentInstructionPrefix();
            _state.SetCurrentInstructionName("");
        }
        int opcode = ProcessPrefixes();
        if (IsLoggingEnabled())
        {
            _logger.Debug("Before execution: {@OpCode} {@StateString} ", ConvertUtils.ToHex8(opcode),
                stateString);
        }
        if (_state.GetContinueZeroFlagValue() != null && IsStringOpcode(opcode))
        {
            // continueZeroFlag is either true or false if a rep prefix has been encountered
            ProcessRep(opcode);
        }
        else
        {
            // Todo: implement
            //ExecOpcode(opcode);
        }
        if (IsLoggingEnabled())
        {
            String instructionName = _state.GetCurrentInstructionNameWithPrefix();
            _logger.Debug("After execution of {@InstructionName} {@State}", instructionName, _state);
        }
        _state.ClearPrefixes();
        _staticAddressesRecorder.Commit();
        _state.IncCycles();
        HandleExternalInterrupt();
        _state.SetIP(_internalIp);
    }

    private void ProcessRep(int opcode)
    {
        // repeat while zero flag is false for REPNZ (last bit is 0)
        // or while zero flag is true for REPZ (last bit is 1)
        var continueZeroFlagValue = _state.GetContinueZeroFlagValue();
        // For some instructions, zero flag is not to be checked
        bool checkZeroFlag = IsStringOpUpdatingFlags(opcode);
        int cx = _state.GetCX();
        while (cx != 0)
        {
            // re-set the segment override that may have been cleared. No need to reset ip
            // as string instructions don't modify it and are only one byte.
            ProcessString(opcode);
            cx = cx - 1;

            if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Verbose))
            {
                _logger.Verbose("{@Rep} Loop, {@Cx}, {@If}, {@CheckZeroFlag}, {@ContinueZF}",
                _state.GetCurrentInstructionNameWithPrefix(),
                ConvertUtils.ToHex(cx), _state.GetZeroFlag(), checkZeroFlag, continueZeroFlagValue);
            }
            // Not all the string operations require checking the zero flag...
            if (checkZeroFlag && _state.GetZeroFlag() != continueZeroFlagValue)
            {
                break;
            }
        }
        _state.SetCX(cx);
    }

    private int GetMemoryAddressOverridableDsSi() => _modRM.GetAddress(SegmentRegisters.DsIndex, _state.GetSI());

    private int GetMemoryAddressEsDi() => MemoryUtils.ToPhysicalAddress(_state.GetES(), _state.GetDI());

    private void ProcessString(int opcode)
    {
        int diff = (_state.GetDirectionFlag() ? -1 : 1) << (opcode & 1);

        if (opcode == 0xA4)
        {
            SetCurrentInstructionName(() => "MOVSB");
            int value = _memory.GetUint8(GetMemoryAddressOverridableDsSi());
            _memory.SetUint8(GetMemoryAddressEsDi(), (byte)value);
            _state.SetSI(_state.GetSI() + diff);
            _state.SetDI(_state.GetDI() + diff);
        }
        if (opcode == 0xA5)
        {
            SetCurrentInstructionName(() => "MOVSW");
            int value = _memory.GetUint16(GetMemoryAddressOverridableDsSi());
            _memory.SetUint16(GetMemoryAddressEsDi(), (ushort)value);
            _state.SetSI(_state.GetSI() + diff);
            _state.SetDI(_state.GetDI() + diff);
        }
        if (opcode == 0xA6)
        {
            SetCurrentInstructionName(() => "CMPSB");
            int value = _memory.GetUint8(GetMemoryAddressOverridableDsSi());
            _alu.Sub8(value, _memory.GetUint8(GetMemoryAddressEsDi()));
            _state.SetSI(_state.GetSI() + diff);
            _state.SetDI(_state.GetDI() + diff);
        }
        if (opcode == 0xA7)
        {
            SetCurrentInstructionName(() => "CMPSW");
            int value = _memory.GetUint16(GetMemoryAddressOverridableDsSi());
            _alu.Sub16(value, _memory.GetUint16(GetMemoryAddressEsDi()));
            _state.SetSI(_state.GetSI() + diff);
            _state.SetDI(_state.GetDI() + diff);
        }
        if (opcode == 0xAA)
        {
            SetCurrentInstructionName(() => "STOSB");
            _memory.SetUint8(GetMemoryAddressEsDi(), (byte)_state.GetAL());
            _state.SetDI(_state.GetDI() + diff);
        }
        if (opcode == 0xAB)
        {
            SetCurrentInstructionName(() => "STOSW");
            _memory.SetUint16(GetMemoryAddressEsDi(), (ushort)_state.GetAX());
            _state.SetDI(_state.GetDI() + diff);
        }
        if (opcode == 0xAC)
        {
            SetCurrentInstructionName(() => "LODSB");
            int value = _memory.GetUint8(GetMemoryAddressOverridableDsSi());
            _state.SetAL(value);
            _state.SetSI(_state.GetSI() + diff);
        }
        if (opcode == 0xAD)
        {
            SetCurrentInstructionName(() => "LODSW");
            int value = _memory.GetUint16(GetMemoryAddressOverridableDsSi());
            _state.SetAX(value);
            _state.SetSI(_state.GetSI() + diff);
        }
        if (opcode == 0xAE)
        {
            SetCurrentInstructionName(() => "SCASB");
            _alu.Sub8(_state.GetAL(), _memory.GetUint8(GetMemoryAddressEsDi()));
            _state.SetDI(_state.GetDI() + diff);
        }
        if (opcode == 0xAF)
        {
            SetCurrentInstructionName(() => "SCASW");
            _alu.Sub16(_state.GetAX(), _memory.GetUint16(GetMemoryAddressEsDi()));
            _state.SetDI(_state.GetDI() + diff);
        }
        if (opcode == 0x6C)
        {
            SetCurrentInstructionName(() => "INSB");
            int port = _state.GetDX();
            int value = Inb(port);
            _memory.SetUint8(GetMemoryAddressEsDi(), (byte)value);
            _state.SetSI(_state.GetSI() + diff);
        }
        if (opcode == 0x6D)
        {
            SetCurrentInstructionName(() => "INSW");
            int port = _state.GetDX();
            int value = Inw(port);
            _memory.SetUint16(GetMemoryAddressEsDi(), (ushort)value);
            _state.SetSI(_state.GetSI() + diff);
        }
        if (opcode == 0x6E)
        {
            SetCurrentInstructionName(() => "OUTSB");
            int port = _state.GetDX();
            int value = _memory.GetUint8(GetMemoryAddressOverridableDsSi());
            Outb(port, value);
            _state.SetSI(_state.GetSI() + diff);
        }
        if (opcode == 0x6F)
        {
            SetCurrentInstructionName(() => "OUTSW");
            int port = _state.GetDX();
            int value = _memory.GetUint16(GetMemoryAddressOverridableDsSi());
            Outw(port, value);
            _state.SetSI(_state.GetSI() + diff);
        }
        HandleInvalidOpcode(opcode);
    }

    private void Outb(int port, int val)
    {
        if (_ioPortDispatcher != null)
        {
            _ioPortDispatcher.Outb((ushort)port, (byte)val);
        }
    }

    private void Outw(int port, int val)
    {
        if (_ioPortDispatcher != null)
        {
            _ioPortDispatcher.Outw((ushort)port, (ushort)val);
        }
    }

    private byte Inb(int port)
    {
        if (_ioPortDispatcher != null)
        {
            return (byte)_ioPortDispatcher.Inb((ushort)port);
        }
        return 0;
    }

    private ushort Inw(int port)
    {
        if (_ioPortDispatcher != null)
        {
            return (ushort)_ioPortDispatcher.Inw((ushort)port);
        }
        return 0;
    }

    private void HandleInvalidOpcodeBecausePrefix(int opcode) => throw new InvalidOpCodeException(_machine, opcode, true);

    private void HandleInvalidOpcode(int opcode) => throw new InvalidOpCodeException(_machine, opcode, false);

    private void Interrupt(int? vectorNumber, bool external)
    {
        if (vectorNumber == null) { return; }
        int targetIP = _memory.GetUint16(4 * vectorNumber.Value);
        int targetCS = _memory.GetUint16(4 * vectorNumber.Value + 2);
        if (_errorOnUninitializedInterruptHandler && targetCS == 0 && targetIP == 0)
        {
            throw new UnhandledOperationException(_machine,
                $"Int was called but vector was not initialized for vectorNumber={ConvertUtils.ToHex(vectorNumber.Value)}");
        }
        int returnCS = _state.GetCS();
        int returnIP = _internalIp;
        if (IsLoggingEnabled())
        {
            _logger.Debug("int {@VectorNumber} handler found in memory, {@SegmentedAddressRepresentation}", ConvertUtils.ToHex(vectorNumber.Value),
                ConvertUtils.ToSegmentedAddressRepresentation(targetCS, targetIP));
        }
        _stack.Push(_state.GetFlags().GetFlagRegister());
        _stack.Push(returnCS);
        _stack.Push(returnIP);
        _state.SetInterruptFlag(false);
        _internalIp = targetIP;
        _state.SetCS(targetCS);
        var recordReturn = true;
        if (external)
        {
            _functionHandlerInUse = _functionHandlerInExternalInterrupt;
            recordReturn = false;
        }
        _functionHandlerInUse.Icall(CallType.INTERRUPT, targetCS, targetIP, returnCS, returnIP, vectorNumber.Value,
            recordReturn);
    }

    private static bool IsStringOpUpdatingFlags(int stringOpCode)
        => stringOpCode == 0xA6 // CMPSB
        || stringOpCode == 0xA7 // CMPSW
        || stringOpCode == 0xAE // SCASB
        || stringOpCode == 0xAF; // SCASW
    private int ProcessPrefixes()
    {
        int opcode = NextUint8();
        while (IsPrefix(opcode))
        {
            ProcessPrefix(opcode);
            opcode = NextUint8();
        }
        return opcode;
    }

    private void ProcessPrefix(int opcode)
    {
        if (opcode == 0x26)
        {
            AddCurrentInstructionPrefix(() => "ES:");
            _state.SetSegmentOverrideIndex(SegmentRegisters.EsIndex);
        }
        if (opcode == 0x2E)
        {
            AddCurrentInstructionPrefix(() => "CS:");
            _state.SetSegmentOverrideIndex(SegmentRegisters.CsIndex);
        }
        if (opcode == 0x36)
        {
            AddCurrentInstructionPrefix(() => "SS:");
            _state.SetSegmentOverrideIndex(SegmentRegisters.SsIndex);
        }
        if (opcode == 0x3E)
        {
            AddCurrentInstructionPrefix(() => "DS:");
            _state.SetSegmentOverrideIndex(SegmentRegisters.DsIndex);
        }
        if (opcode == 0x64)
        {
            AddCurrentInstructionPrefix(() => "FS:");
            _state.SetSegmentOverrideIndex(SegmentRegisters.FsIndex);
        }
        if (opcode == 0x65)
        {
            AddCurrentInstructionPrefix(() => "GS:");
            _state.SetSegmentOverrideIndex(SegmentRegisters.GsIndex);
        }
        if (opcode == 0xF0)
        {
            AddCurrentInstructionPrefix(() => "LOCK");
        }
        if (opcode == 0xF2 || opcode == 0xF3)
        { // REPNZ, REPZ
            bool continueZeroFlagValue = (opcode & 1) == 1;
            _state.SetContinueZeroFlagValue(continueZeroFlagValue);
            AddCurrentInstructionPrefix(() => "REP" + (continueZeroFlagValue ? "Z" : ""));
        }
        throw new InvalidVMOperationException(_machine,
            $"processPrefix Called with a non prefix opcode {opcode}");
    }

    private bool IsPrefix(int opcode) => PREFIXES_OPCODES.Contains(opcode);

    private void HandleExternalInterrupt()
    {
        if (_externalInterruptVectorNumber == null || !_state.GetInterruptFlag())
        {
            return;
        }
        if (IsLoggingEnabled())
        {
            _logger.Debug("Interrupted!, {@ExternalInterruptVectorNumber}", _externalInterruptVectorNumber);
        }
        Interrupt(_externalInterruptVectorNumber, true);
        _externalInterruptVectorNumber = null;
    }

    private bool IsStringOpcode(int opcode)
    {
        return STRING_OPCODES.Contains(opcode);
    }

    public void FarRet(int numberOfBytesToPop)
    {
        _functionHandlerInUse.Ret(CallType.FAR);
        _internalIp = _stack.Pop();
        _state.SetCS(_stack.Pop());
        _state.SetSP(numberOfBytesToPop + _state.GetSP());
    }



    public StaticAddressesRecorder GetStaticAddressesRecorder() => _staticAddressesRecorder;

    public void InterruptRet()
    {
        _functionHandlerInUse.Ret(CallType.INTERRUPT);
        _internalIp = _stack.Pop();
        _state.SetCS(_stack.Pop());
        _state.GetFlags().SetFlagRegister(_stack.Pop());
        _functionHandlerInUse = _functionHandler;
    }

    private int GetDsNextUint16Address()
    {
        return _modRM.GetAddress(SegmentRegisters.DsIndex, NextUint16(), true);
    }


    private int GetRm8Or16(bool op1Byte)
    {
        if (op1Byte)
        {
            return _modRM.GetRm8();
        }
        return _modRM.GetRm16();
    }

    private void HandleDivisionError()
    {
        // Reset IP because instruction is not finished (this is how an actual CPU behaves)
        _internalIp = _state.GetIP();
        Interrupt(0, false);
    }

    private void Grp4()
    {
        _modRM.Read();
        int groupIndex = _modRM.GetRegisterIndex();
        if (groupIndex == 0)
        {
            SetCurrentInstructionName(() => "INC");
            _modRM.SetRm8(_alu.Inc8(_modRM.GetRm8()));
        }
        if (groupIndex == 1)
        {
            SetCurrentInstructionName(() => "DEC");
            _modRM.SetRm8(_alu.Dec8(_modRM.GetRm8()));
        }
        if (groupIndex == 7)
        {
            // Callback, emulator specific instruction FE38 like in dosbox,
            // to allow interrupts to be overridden by the program
            Callback(NextUint16());
        }
        throw new InvalidGroupIndexException(_machine, groupIndex);
    }

    private void Grp5()
    {
        _modRM.Read();
        int groupIndex = _modRM.GetRegisterIndex();
        if (groupIndex == 0)
        {
            SetCurrentInstructionName(() => "INC");
            _modRM.SetRm16(_alu.Inc16(_modRM.GetRm16()));
        }
        if (groupIndex == 1)
        {
            SetCurrentInstructionName(() => "DEC");
            _modRM.SetRm16(_alu.Dec16(_modRM.GetRm16()));
        }
        if (groupIndex == 2)
        {
            SetCurrentInstructionName(() => "NEAR CALL");
            int callAddress = _modRM.GetRm16();
            NearCall(_internalIp, callAddress);
        }
        if (groupIndex == 3)
        {
            SetCurrentInstructionName(() => "FAR CALL");
            int? ipAddress = _modRM.GetMemoryAddress();
            if (ipAddress is null) { return; }
            GetStaticAddressesRecorder().SetCurrentAddressOperation(ValueOperation.READ, OperandSize.Dword32Ptr);
            int ip = _memory.GetUint16(ipAddress.Value);
            int cs = _memory.GetUint16(ipAddress.Value + 2);
            FarCall(_state.GetCS(), _internalIp, cs, ip);
        }
        if (groupIndex == 4)
        {
            int ip = _modRM.GetRm16();
            JumpNear(ip);
        }
        if (groupIndex == 5)
        {
            int? ipAddress = _modRM.GetMemoryAddress();
            if (ipAddress is null) { return; }
            GetStaticAddressesRecorder().SetCurrentAddressOperation(ValueOperation.READ, OperandSize.Dword32Ptr);
            int ip = _memory.GetUint16(ipAddress.Value);
            int cs = _memory.GetUint16(ipAddress.Value + 2);
            JumpFar(cs, ip);
        }
        if (groupIndex == 6)
        {
            SetCurrentInstructionName(() => "PUSH");
            _stack.Push(_modRM.GetRm16());
        }
        throw new InvalidGroupIndexException(_machine, groupIndex);
    }

    private void JumpNear(int ip)
    {
        SetCurrentInstructionName(
            () => $"JMP NEAR {ConvertUtils.ToSegmentedAddressRepresentation(_state.GetCS(), ip)}");
        HandleJump(_state.GetCS(), ip);
    }

    private void JumpFar(int cs, int ip)
    {
        SetCurrentInstructionName(
            () => $"JMP FAR {ConvertUtils.ToSegmentedAddressRepresentation(cs, ip)}");
        HandleJump(cs, ip);
    }

    private void HandleJump(int cs, int ip)
    {
        _internalIp = ip;
        _state.SetCS(cs);
    }

    private void NearCall(int returnIP, int callIP)
    {
        _stack.Push(returnIP);
        _internalIp = callIP;
        HandleCall(CallType.NEAR, _state.GetCS(), returnIP, _state.GetCS(), callIP);
    }

    private void FarCall(int returnCS, int returnIP, int targetCS, int targetIP)
    {
        _stack.Push(returnCS);
        _stack.Push(returnIP);
        _state.SetCS(targetCS);
        _internalIp = targetIP;
        HandleCall(CallType.FAR, returnCS, returnIP, targetCS, targetIP);
    }

    private void HandleCall(CallType callType, int returnCS, int returnIP, int targetCS, int targetIP)
    {
        if (IsLoggingEnabled())
        {
            _logger.Debug("CALL {@TargetCsTargetIp}, will return to {@ReturnCsReturnIp}", ConvertUtils.ToSegmentedAddressRepresentation(targetCS, targetIP),
              ConvertUtils.ToSegmentedAddressRepresentation(returnCS, returnIP));
        }
        _functionHandlerInUse.Call(callType, targetCS, targetIP, returnCS, returnIP);
    }

    public void NearRet(int numberOfBytesToPop)
    {
        _functionHandlerInUse.Ret(CallType.NEAR);
        _internalIp = _stack.Pop();
        _state.SetSP(numberOfBytesToPop + _state.GetSP());
    }

    public byte NextUint8()
    {
        var res = _memory.GetUint8(GetInternalIpPhysicalAddress());
        _internalIp++;
        return res;
    }

    public void SetFlagOnInterruptStack(int flagMask, bool flagValue)
    {
        int flagsAddress = MemoryUtils.ToPhysicalAddress(_state.GetSS(), _state.GetSP() + 4);
        int value = _memory.GetUint16(flagsAddress);
        if (flagValue)
        {
            value = value | flagMask;
        }
        else
        {
            value = value & ~flagMask;
        }
        _memory.SetUint16(flagsAddress, (ushort)value);
    }

    private void Callback(int callbackIndex)
    {
        SetCurrentInstructionName(() => $"CALLBACK {callbackIndex}");
        if (IsLoggingEnabled())
        {
            _logger.Debug("callback {@CallbackIndex}", ConvertUtils.ToHex16(callbackIndex));
        }
        _callbackHandler.Run(callbackIndex);
    }

    public ushort NextUint16()
    {
        var res = _memory.GetUint16(GetInternalIpPhysicalAddress());
        _internalIp += 2;
        return res;
    }

    private int GetInternalIpPhysicalAddress() => MemoryUtils.ToPhysicalAddress(_state.GetCS(), _internalIp);
}
