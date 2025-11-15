namespace Spice86.Core.Emulator.InterruptHandlers.Dos;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Function;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.OperatingSystem;
using Spice86.Shared.Interfaces;

/// <summary>
/// Represents dos disk int 26 handler.
/// </summary>
public class DosDiskInt26Handler : InterruptHandler {
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
    public DosDiskInt26Handler(IMemory memory, DosDriveManager dosDriveManager,
        IFunctionHandlerProvider functionHandlerProvider, Stack stack, State state,
        ILoggerService loggerService)
        : base(memory, functionHandlerProvider, stack, state, loggerService) {
        _dosDriveManager = dosDriveManager;
    }

    /// <summary>
    /// The vector number.
    /// </summary>
    public override byte VectorNumber => 0x26;

    /// <summary>
    /// Performs the run operation.
    /// </summary>
    public override void Run() {
        if (LoggerService.IsEnabled(Serilog.Events.LogEventLevel.Warning)) {
            LoggerService.Warning("DOS INT26H was called, hope for the best!");
        }
        if (State.AL >= DosDriveManager.MaxDriveCount || !_dosDriveManager.HasDriveAtIndex(State.AL)) {
            State.AX = 0x8002;
            State.CarryFlag = true;
        } else {
            State.CarryFlag = false;
            State.AX = 0;
        }
    }
}