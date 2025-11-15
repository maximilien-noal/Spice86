namespace Spice86.Core.Emulator.InterruptHandlers.Dos;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Function;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.OperatingSystem;
using Spice86.Shared.Emulator.Memory;
using Spice86.Shared.Interfaces;

/// <summary>
/// Represents dos disk int 25 handler.
/// </summary>
public class DosDiskInt25Handler : InterruptHandler {
    private readonly DosDriveManager _dosDriveManager;

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="memory">The memory.</param>
    /// <param name="dosDriveManager">The dos drive manager.</param>
    /// <param name="functionHandlerProvider">The function handler provider.</param>
    /// <param name="stack">The stack.</param>
    /// <param name="state">The state.</param>
    /// <param name="loggerService">The logger service.</param>
    public DosDiskInt25Handler(IMemory memory, DosDriveManager dosDriveManager,
        IFunctionHandlerProvider functionHandlerProvider, Stack stack, State state,
        ILoggerService loggerService)
        : base(memory, functionHandlerProvider, stack, state, loggerService) {
        _dosDriveManager = dosDriveManager;
    }

    /// <summary>
    /// The vector number.
    /// </summary>
    public override byte VectorNumber => 0x25;

    /// <summary>
    /// Performs the run operation.
    /// </summary>
    public override void Run() {
        byte driveNumber = State.AL;
        ushort sectorToRead = State.CX;
        ushort startingLogicalSector = State.DX;
        SegmentedAddress bufferForData = new(State.DS, State.BX);

        if (driveNumber >= DosDriveManager.MaxDriveCount || !_dosDriveManager.HasDriveAtIndex(State.AL)) {
            State.AX = 0x8002;
            State.CarryFlag = true;
        } else {
            if (sectorToRead == 1 && startingLogicalSector == 0) {
                if (driveNumber >= 2) {
                    // write some BPB data into buffer for MicroProse installers
                    Memory.UInt16[bufferForData.Segment, (ushort)(bufferForData.Offset + 0x1c)] = 0x3f; // hidden sectors
                }
            } else if (LoggerService.IsEnabled(Serilog.Events.LogEventLevel.Warning)) {
                LoggerService.Warning("Interrupt 25 called but not as disk detection, {DriveIndex}", State.AL);
            }
            State.CarryFlag = false;
            State.AX = 0;
        }
    }
}