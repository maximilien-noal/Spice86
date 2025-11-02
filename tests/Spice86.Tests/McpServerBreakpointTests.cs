namespace Spice86.Tests;

using Spice86.Core.CLI;
using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.VM;
using Spice86.Core.Emulator.VM.Breakpoint;
using Spice86.Logging;
using Spice86.MCP.Tools;
using Spice86.Shared.Interfaces;
using System.Threading.Tasks;
using Xunit;

/// <summary>
/// Tests for MCP server breakpoint management functionality.
/// Validates comprehensive breakpoint support including memory, interrupt, IO port, and execution breakpoints
/// for enhanced reverse engineering and debugging capabilities.
/// </summary>
public class McpServerBreakpointTests {
    private readonly State _state;
    private readonly IMemory _memory;
    private readonly EmulatorBreakpointsManager _breakpointsManager;
    private readonly EmulatorTools _tools;

    public McpServerBreakpointTests() {
        var configuration = new Configuration();
        _state = new State(configuration.CpuModel);
        
        var loggerService = new LoggerService();
        var pauseHandler = new TestPauseHandler(loggerService);
        _breakpointsManager = new EmulatorBreakpointsManager(pauseHandler, _state);
        
        var ram = new Ram(A20Gate.EndOfHighMemoryArea);
        var a20Gate = new A20Gate(configuration.A20Gate);
        _memory = new Memory(_breakpointsManager.MemoryReadWriteBreakpoints, ram, a20Gate, initializeResetVector: false);
        
        _tools = new EmulatorTools(_state, _memory, _breakpointsManager, pauseHandler);
    }

    [Fact]
    public async Task AddMemoryReadBreakpoint_AddsBreakpointSuccessfully() {
        // Act
        string result = await _tools.AddMemoryReadBreakpoint("0x1000");

        // Assert
        Assert.Contains("Added memory read breakpoint at 0x1000", result);
    }

    [Fact]
    public async Task AddMemoryWriteBreakpoint_AddsBreakpointSuccessfully() {
        // Act
        string result = await _tools.AddMemoryWriteBreakpoint("0x2000");

        // Assert
        Assert.Contains("Added memory write breakpoint at 0x2000", result);
    }

    [Fact]
    public async Task AddInterruptBreakpoint_AddsBreakpointSuccessfully() {
        // Act
        string result = await _tools.AddInterruptBreakpoint(0x21);

        // Assert
        Assert.Contains("Added interrupt breakpoint for INT 0x21", result);
    }

    [Fact]
    public async Task AddIoReadBreakpoint_AddsBreakpointSuccessfully() {
        // Act
        string result = await _tools.AddIoReadBreakpoint("0x3F8");

        // Assert
        Assert.Contains("Added IO read breakpoint at port 0x3F8", result);
    }

    [Fact]
    public async Task AddIoWriteBreakpoint_AddsBreakpointSuccessfully() {
        // Act
        string result = await _tools.AddIoWriteBreakpoint("0x3F8");

        // Assert
        Assert.Contains("Added IO write breakpoint at port 0x3F8", result);
    }

    [Fact]
    public async Task AddIoAccessBreakpoint_AddsBreakpointSuccessfully() {
        // Act
        string result = await _tools.AddIoAccessBreakpoint("0x3F8");

        // Assert
        Assert.Contains("Added IO access breakpoint at port 0x3F8", result);
    }

    [Fact]
    public async Task AddMemoryAccessBreakpoint_AddsBreakpointSuccessfully() {
        // Act
        string result = await _tools.AddMemoryAccessBreakpoint("0x1000");

        // Assert
        Assert.Contains("Added memory access breakpoint at 0x1000", result);
    }

    [Fact]
    public async Task ListBreakpoints_ReturnsAllBreakpoints() {
        // Arrange
        await _tools.AddBreakpoint("0x1000");
        await _tools.AddMemoryReadBreakpoint("0x2000");

        // Act
        string result = await _tools.ListBreakpoints();

        // Assert
        Assert.Contains("breakpoint", result.ToLower());
    }

    [Fact]
    public async Task PauseEmulation_RequestsPause() {
        // Act
        string result = await _tools.PauseEmulation();

        // Assert
        Assert.Contains("Emulation paused", result);
    }

    [Fact]
    public async Task ResumeEmulation_RequestsResume() {
        // Act
        string result = await _tools.ResumeEmulation();

        // Assert
        Assert.Contains("Emulation resumed", result);
    }

    private sealed class TestPauseHandler : IPauseHandler {
        private readonly ILoggerService _loggerService;
        private bool _isPaused;

        public TestPauseHandler(ILoggerService loggerService) {
            _loggerService = loggerService;
        }

        public bool IsPaused => _isPaused;

        public event Action? Pausing;
        public event Action? Paused;
        public event Action? Resumed;

        public void RequestPause(string? reason = null) {
            _isPaused = true;
            Pausing?.Invoke();
            Paused?.Invoke();
        }

        public void Resume() {
            _isPaused = false;
            Resumed?.Invoke();
        }

        public void WaitIfPaused() {
            // No-op for tests
        }

        public void Dispose() {
            // No-op
        }
    }
}
