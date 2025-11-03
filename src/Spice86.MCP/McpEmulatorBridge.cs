namespace Spice86.MCP;

using Spice86.Core.CLI;
using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.CPU.CfgCpu;
using Spice86.Core.Emulator.Function;
using Spice86.Core.Emulator.Function.Dump;
using Spice86.Core.Emulator.InterruptHandlers.Bios.Structures;
using Spice86.Core.Emulator.IOPorts;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.VM;
using Spice86.Core.Emulator.VM.Breakpoint;
using Spice86.Logging;
using Spice86.MCP.Extensibility;
using Spice86.Shared.Emulator.Memory;
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
    /// Initializes a new instance for testing/development with minimal components.
    /// For production use, provide all required components via the other constructor.
    /// </summary>
    /// <param name="configuration">Configuration for the emulator.</param>
    /// <param name="loggerService">Shared logger service instance.</param>
    /// <param name="cfgCpu">The CFG CPU instance.</param>
    /// <param name="biosDataArea">The BIOS data area.</param>
    public McpEmulatorBridge(Configuration configuration, ILoggerService loggerService, CfgCpu cfgCpu, BiosDataArea biosDataArea) {
        _loggerService = loggerService;
        
        // Initialize basic emulator components
        State = new State(configuration.CpuModel);
        
        _ownedPauseHandler = new PauseHandler(_loggerService);
        PauseHandler = _ownedPauseHandler;
        BreakpointsManager = new EmulatorBreakpointsManager(_ownedPauseHandler, State);
        
        Ram ram = new Ram(A20Gate.EndOfHighMemoryArea);
        A20Gate a20Gate = new A20Gate(configuration.A20Gate);
        Memory = new Memory(BreakpointsManager.MemoryReadWriteBreakpoints, ram, a20Gate, initializeResetVector: false);
        
        // Create minimal IOPortDispatcher for standalone mode
        IOPortDispatcher = new IOPortDispatcher(BreakpointsManager.IoReadWriteBreakpoints, State, _loggerService, false);
        
        // Use provided required components
        CfgCpu = cfgCpu;
        BiosDataArea = biosDataArea;
        
        // Optional components for extensibility
        ExecutionFlowDumper = null;
        FunctionInformations = null;
        CustomStructureRegistry = new CustomStructureRegistry();
        
        _loggerService.Information("MCP emulator bridge initialized in standalone mode");
    }

    /// <summary>
    /// Initializes from individual emulator components.
    /// </summary>
    /// <param name="state">The CPU state containing registers and flags.</param>
    /// <param name="memory">The memory interface.</param>
    /// <param name="breakpointsManager">The breakpoints manager.</param>
    /// <param name="ioPortDispatcher">The IO port dispatcher.</param>
    /// <param name="cfgCpu">The CFG CPU instance.</param>
    /// <param name="biosDataArea">The BIOS data area.</param>
    /// <param name="pauseHandler">The pause handler for emulation control.</param>
    /// <param name="loggerService">Shared logger service instance.</param>
    /// <param name="executionFlowDumper">Optional execution flow dumper for recording execution data.</param>
    /// <param name="functionInformations">Optional dictionary of function information from reverse engineering.</param>
    /// <param name="customStructureRegistry">Optional registry for custom game-specific structures.</param>
    public McpEmulatorBridge(
        State state,
        IMemory memory,
        EmulatorBreakpointsManager breakpointsManager,
        IOPortDispatcher ioPortDispatcher,
        CfgCpu cfgCpu,
        BiosDataArea biosDataArea,
        IPauseHandler pauseHandler,
        ILoggerService loggerService,
        ExecutionFlowDumper? executionFlowDumper = null,
        IDictionary<SegmentedAddress, FunctionInformation>? functionInformations = null,
        CustomStructureRegistry? customStructureRegistry = null) {
        _loggerService = loggerService;
        State = state;
        Memory = memory;
        BreakpointsManager = breakpointsManager;
        IOPortDispatcher = ioPortDispatcher;
        CfgCpu = cfgCpu;
        BiosDataArea = biosDataArea;
        PauseHandler = pauseHandler;
        ExecutionFlowDumper = executionFlowDumper;
        FunctionInformations = functionInformations;
        CustomStructureRegistry = customStructureRegistry ?? new CustomStructureRegistry();
        
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
    /// Gets the CFG CPU instance.
    /// </summary>
    public CfgCpu CfgCpu { get; }

    /// <summary>
    /// Gets the BIOS data area.
    /// </summary>
    public BiosDataArea BiosDataArea { get; }

    /// <summary>
    /// Gets the pause handler for controlling emulation flow.
    /// </summary>
    public IPauseHandler PauseHandler { get; }

    /// <summary>
    /// Gets the execution flow dumper for recording execution data (may be null).
    /// </summary>
    public ExecutionFlowDumper? ExecutionFlowDumper { get; }

    /// <summary>
    /// Gets the function information dictionary from reverse engineering (may be null).
    /// </summary>
    public IDictionary<SegmentedAddress, FunctionInformation>? FunctionInformations { get; }

    /// <summary>
    /// Gets the registry for custom game-specific structures.
    /// </summary>
    public CustomStructureRegistry CustomStructureRegistry { get; }

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
