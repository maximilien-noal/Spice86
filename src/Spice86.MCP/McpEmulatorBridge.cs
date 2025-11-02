namespace Spice86.MCP;

using Spice86.Core.CLI;
using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.VM;
using Spice86.Core.Emulator.VM.Breakpoint;
using Spice86.Logging;
using Spice86.Shared.Interfaces;

/// <summary>
/// Provides a bridge between the MCP server and Spice86 emulator components.
/// Initializes and exposes core emulator services (CPU state, memory, breakpoints, pause control)
/// for programmatic access through the Model Context Protocol.
/// </summary>
public sealed class McpEmulatorBridge : IDisposable {
    private readonly LoggerService _loggerService;
    private readonly PauseHandler _pauseHandler;
    private bool _disposed;

    public McpEmulatorBridge(Configuration configuration) {
        _loggerService = new LoggerService();
        
        // Initialize basic emulator components
        State = new State(configuration.CpuModel);
        
        _pauseHandler = new PauseHandler(_loggerService);
        BreakpointsManager = new EmulatorBreakpointsManager(_pauseHandler, State);
        
        var ram = new Ram(A20Gate.EndOfHighMemoryArea);
        var a20Gate = new A20Gate(configuration.A20Gate);
        Memory = new Memory(BreakpointsManager.MemoryReadWriteBreakpoints, ram, a20Gate, initializeResetVector: false);
        
        _loggerService.Information("MCP emulator bridge initialized");
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
    /// Gets the pause handler for controlling emulation flow.
    /// </summary>
    public IPauseHandler PauseHandler => _pauseHandler;

    /// <summary>
    /// Disposes the MCP emulator bridge.
    /// </summary>
    public void Dispose() {
        if (_disposed) {
            return;
        }

        _loggerService.Information("MCP emulator bridge disposed");
        _pauseHandler.Dispose();
        _disposed = true;
    }
}
