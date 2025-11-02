namespace Spice86.MCP;

using Spice86.Core.CLI;
using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.VM;
using Spice86.Core.Emulator.VM.Breakpoint;
using Spice86.Logging;
using Spice86.Shared.Interfaces;

/// <summary>
/// Service that provides access to emulator components for MCP tools.
/// </summary>
public sealed class EmulatorService : IDisposable {
    private readonly LoggerService _loggerService;
    private bool _disposed;

    public EmulatorService(Configuration configuration) {
        _loggerService = new LoggerService();
        
        // Initialize basic emulator components
        State = new State(configuration.CpuModel);
        
        var pauseHandler = new PauseHandler(_loggerService);
        BreakpointsManager = new EmulatorBreakpointsManager(pauseHandler, State);
        
        var ram = new Ram(A20Gate.EndOfHighMemoryArea);
        var a20Gate = new A20Gate(configuration.A20Gate);
        Memory = new Memory(BreakpointsManager.MemoryReadWriteBreakpoints, ram, a20Gate, initializeResetVector: false);
        
        _loggerService.Information("Emulator service initialized for MCP server");
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
    /// Disposes the emulator service.
    /// </summary>
    public void Dispose() {
        if (_disposed) {
            return;
        }

        _loggerService.Information("Emulator service disposed");
        _disposed = true;
    }

    private sealed class PauseHandler : IPauseHandler {
        private readonly ILoggerService _loggerService;

        public PauseHandler(ILoggerService loggerService) {
            _loggerService = loggerService;
        }

        public bool IsPaused => false;

        public event Action? Pausing;
        public event Action? Paused;
        public event Action? Resumed;

        public void RequestPause(string? reason = null) {
            _loggerService.Information("Pause requested: {Message}", reason ?? "No message");
            Pausing?.Invoke();
            Paused?.Invoke();
        }

        public void Resume() {
            _loggerService.Information("Resume requested");
            Resumed?.Invoke();
        }

        public void WaitIfPaused() {
            // No-op for MCP server
        }

        public void Dispose() {
            // No-op
        }
    }
}
