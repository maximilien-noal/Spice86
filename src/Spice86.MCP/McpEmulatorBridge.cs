namespace Spice86.MCP;

using Spice86.Core.CLI;
using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.CPU.CfgCpu;
using Spice86.Core.Emulator.InterruptHandlers.Bios.Structures;
using Spice86.Core.Emulator.IOPorts;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.VM;
using Spice86.Core.Emulator.VM.Breakpoint;
using Spice86.Logging;
using Spice86.Shared.Interfaces;

/// <summary>
/// Provides a bridge between the MCP server and Spice86 emulator components.
/// Can be initialized standalone or from individual emulator components.
/// </summary>
public sealed class McpEmulatorBridge : IDisposable {
    private readonly ILoggerService _loggerService;
    private readonly PauseHandler? _ownedPauseHandler;
    private bool _disposed;

    /// <summary>
    /// Initializes a new standalone instance for testing/development.
    /// </summary>
    /// <param name="configuration">Configuration for the emulator.</param>
    /// <param name="loggerService">Shared logger service instance.</param>
    public McpEmulatorBridge(Configuration configuration, ILoggerService loggerService) {
        _loggerService = loggerService;
        
        // Initialize basic emulator components
        State = new State(configuration.CpuModel);
        
        _ownedPauseHandler = new PauseHandler(_loggerService);
        BreakpointsManager = new EmulatorBreakpointsManager(_ownedPauseHandler, State);
        
        var ram = new Ram(A20Gate.EndOfHighMemoryArea);
        var a20Gate = new A20Gate(configuration.A20Gate);
        Memory = new Memory(BreakpointsManager.MemoryReadWriteBreakpoints, ram, a20Gate, initializeResetVector: false);
        
        // Create minimal IOPortDispatcher for standalone mode
        IOPortDispatcher = new IOPortDispatcher(BreakpointsManager.IoReadWriteBreakpoints, State, _loggerService, false);
        
        // These will be null in standalone mode
        CfgCpu = null;
        BiosDataArea = null;
        
        _loggerService.Information("MCP emulator bridge initialized in standalone mode");
    }

    /// <summary>
    /// Initializes from individual emulator components.
    /// </summary>
    /// <param name="state">The CPU state containing registers and flags.</param>
    /// <param name="memory">The memory interface.</param>
    /// <param name="breakpointsManager">The breakpoints manager.</param>
    /// <param name="ioPortDispatcher">The IO port dispatcher.</param>
    /// <param name="loggerService">Shared logger service instance.</param>
    /// <param name="cfgCpu">Optional CFG CPU instance.</param>
    /// <param name="biosDataArea">Optional BIOS data area.</param>
    /// <param name="pauseHandler">Optional pause handler (if null, emulation control is not available).</param>
    public McpEmulatorBridge(
        State state,
        IMemory memory,
        EmulatorBreakpointsManager breakpointsManager,
        IOPortDispatcher ioPortDispatcher,
        ILoggerService loggerService,
        CfgCpu? cfgCpu = null,
        BiosDataArea? biosDataArea = null,
        IPauseHandler? pauseHandler = null) {
        _loggerService = loggerService;
        State = state;
        Memory = memory;
        BreakpointsManager = breakpointsManager;
        IOPortDispatcher = ioPortDispatcher;
        CfgCpu = cfgCpu;
        BiosDataArea = biosDataArea;
        PauseHandler = pauseHandler;
        
        // We don't own the pause handler in this mode
        _ownedPauseHandler = null;
    }

    /// <summary>
    /// Gets the CPU state containing registers and flags.
    /// </summary>
    public State State { get; }

    /// <summary>
    /// Gets the memory interface.
    /// </summary>
    public IMemory Memory { get; }

    /// <summary>
    /// Gets the breakpoints manager.
    /// </summary>
    public EmulatorBreakpointsManager BreakpointsManager { get; }

    /// <summary>
    /// Gets the IO port dispatcher for port exploration.
    /// </summary>
    public IOPortDispatcher IOPortDispatcher { get; }

    /// <summary>
    /// Gets the CFG CPU instance (may be null if CFG CPU is not enabled).
    /// </summary>
    public CfgCpu? CfgCpu { get; }

    /// <summary>
    /// Gets the BIOS data area (may be null in headless or standalone configurations).
    /// </summary>
    public BiosDataArea? BiosDataArea { get; }

    /// <summary>
    /// Gets the pause handler for controlling emulation flow.
    /// May be null if emulation control is not available.
    /// </summary>
    public IPauseHandler? PauseHandler { get; }

    /// <summary>
    /// Disposes the MCP emulator bridge.
    /// </summary>
    public void Dispose() {
        if (_disposed) {
            return;
        }

        // Only dispose resources we own (standalone mode)
        if (_ownedPauseHandler != null) {
            _loggerService.Information("MCP emulator bridge disposed");
        }
        _ownedPauseHandler?.Dispose();
        
        _disposed = true;
    }
}
