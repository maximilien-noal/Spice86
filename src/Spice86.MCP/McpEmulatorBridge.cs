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
/// Can be initialized standalone or from a fully-configured Machine instance.
/// </summary>
public sealed class McpEmulatorBridge : IDisposable {
    private readonly LoggerService? _ownedLoggerService;
    private readonly PauseHandler? _ownedPauseHandler;
    private bool _disposed;

    /// <summary>
    /// Initializes a new standalone instance for testing/development.
    /// </summary>
    public McpEmulatorBridge(Configuration configuration) {
        _ownedLoggerService = new LoggerService();
        
        // Initialize basic emulator components
        State = new State(configuration.CpuModel);
        
        _ownedPauseHandler = new PauseHandler(_ownedLoggerService);
        BreakpointsManager = new EmulatorBreakpointsManager(_ownedPauseHandler, State);
        
        var ram = new Ram(A20Gate.EndOfHighMemoryArea);
        var a20Gate = new A20Gate(configuration.A20Gate);
        Memory = new Memory(BreakpointsManager.MemoryReadWriteBreakpoints, ram, a20Gate, initializeResetVector: false);
        
        // Create minimal IOPortDispatcher for standalone mode
        IOPortDispatcher = new IOPortDispatcher(BreakpointsManager.IoReadWriteBreakpoints, State, _ownedLoggerService, false);
        
        // These will be null in standalone mode
        CfgCpu = null;
        BiosDataArea = null;
        
        _ownedLoggerService.Information("MCP emulator bridge initialized in standalone mode");
    }

    /// <summary>
    /// Initializes from a fully-configured Machine instance (preferred for production).
    /// </summary>
    public McpEmulatorBridge(Machine machine) {
        State = machine.CpuState;
        Memory = machine.Memory;
        BreakpointsManager = machine.EmulatorBreakpointsManager;
        IOPortDispatcher = machine.IoPortDispatcher;
        CfgCpu = machine.CfgCpu;
        BiosDataArea = machine.BiosDataArea;
        
        // We don't own these resources in this mode
        _ownedLoggerService = null;
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
    /// Returns the owned pause handler if in standalone mode, or null if using shared instance.
    /// </summary>
    public IPauseHandler? PauseHandler => _ownedPauseHandler;

    /// <summary>
    /// Disposes the MCP emulator bridge.
    /// </summary>
    public void Dispose() {
        if (_disposed) {
            return;
        }

        // Only dispose resources we own (standalone mode)
        if (_ownedLoggerService != null) {
            _ownedLoggerService.Information("MCP emulator bridge disposed");
        }
        _ownedPauseHandler?.Dispose();
        
        _disposed = true;
    }
}
