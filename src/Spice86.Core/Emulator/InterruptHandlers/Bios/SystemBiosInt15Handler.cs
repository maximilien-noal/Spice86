namespace Spice86.Core.Emulator.InterruptHandlers.Bios;

using Serilog.Events;

using Spice86.Core.Emulator.InterruptHandlers;
using Spice86.Core.Emulator.VM;
using Spice86.Core.Emulator.Callback;
using Spice86.Shared.Interfaces;

/// <summary>
/// BIOS services
/// </summary>
public class SystemBiosInt15Handler : InterruptHandler {
    
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="machine">The emulator machine.</param>
    /// <param name="loggerService">The logger service implementation.</param>
    public SystemBiosInt15Handler(Machine machine, ILoggerService loggerService) : base(machine, loggerService) {
        FillDispatchTable();
    }

    private void FillDispatchTable() {
        _dispatchTable.Add(0x24, new Callback(0x24, () => ToggleA20GateOrGetStatus(true)));
        _dispatchTable.Add(0x6, new Callback(0x6, Unsupported));
        _dispatchTable.Add(0xC0, new Callback(0xC0, Unsupported));
        _dispatchTable.Add(0xC2, new Callback(0xC2, Unsupported));
        _dispatchTable.Add(0xC4, new Callback(0xC4, Unsupported));
        _dispatchTable.Add(0x87, new Callback(0x87, CopyExtendedMemory));
        _dispatchTable.Add(0x88, new Callback(0x88, GetExtendedMemorySize));
    }

    /// <inheritdoc />
    public override byte Index => 0x15;

    /// <inheritdoc />
    public override void Run() {
        byte operation = _state.AH;
        Run(operation);
    }

    /// <summary>
    /// Bios support function for the A20 Gate line. <br/>
    /// AL contains one of:
    /// <ul>
    ///   <li>0: Disable</li>
    ///   <li>1: Enable</li>
    ///   <li>2: Query status</li>
    ///   <li>3: Get A20 support</li>
    /// </ul>
    /// </summary>
    public void ToggleA20GateOrGetStatus(bool calledFromVm) {
        switch (_state.AL) {
            case 0:
                _machine.Memory.A20Gate.IsEnabled = false;
                SetCarryFlag(false, calledFromVm);
                break;
            case 1:
                _machine.Memory.A20Gate.IsEnabled = true;
                SetCarryFlag(false, calledFromVm);
                break;
            case 2:
                _state.AL = (byte) (_machine.Memory.A20Gate.IsEnabled ? 0x1 : 0x0);
                _state.AH = 0; // success
                SetCarryFlag(false, calledFromVm);
                break;
            case 3:
                _machine.Memory.A20Gate.IsEnabled = false;
                _state.BX = 0x3; //Bitmask, keyboard and 0x92;
                _state.AH = 0; // success
                SetCarryFlag(false, calledFromVm);
                break;

            default:
                if (_loggerService.IsEnabled(LogEventLevel.Error)) {
                    _loggerService.Error("Unrecognized command in AL for {MethodName}", nameof(ToggleA20GateOrGetStatus));
                }
                break;
        }
    }

    /// <summary>
    /// Reports extended memory size in AX.
    /// </summary>
    public void GetExtendedMemorySize() {
        _state.AX = (ushort) (_memory.A20Gate.IsEnabled ? 0 : _machine.Xms?.ExtendedMemorySize ?? 0);
    }

    public void CopyExtendedMemory() {
        bool enabled = _memory.A20Gate.IsEnabled;
        _machine.Memory.A20Gate.IsEnabled = true;
        uint bytes = _state.ECX;
        uint data = _state.ESI;
        long source = _memory.UInt32[data + 0x12] & 0x00FFFFFF + _memory.UInt8[data + 0x16] << 24;
        long dest = _memory.UInt32[data + 0x1A] & 0x00FFFFFF + _memory.UInt8[data + 0x1E] << 24;
        _state.EAX = (_state.EAX & 0xFFFF) | (_state.EAX & 0xFFFF0000);
        _memory.MemCopy((uint)source, (uint)dest, bytes);
        _memory.A20Gate.IsEnabled = enabled;
    }

    private void Unsupported() {
        // We are not an IBM PS/2
        SetCarryFlag(true, true);
        _state.AH = 0x86;
    }
}