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
/// Tests for the EmulatorTools MCP tools.
/// </summary>
public class EmulatorToolsTests {
    private readonly State _state;
    private readonly IMemory _memory;
    private readonly EmulatorBreakpointsManager _breakpointsManager;
    private readonly EmulatorTools _tools;

    public EmulatorToolsTests() {
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
    public async Task GetCpuRegisters_ReturnsFormattedRegisters() {
        // Arrange
        _state.EAX = 0x12345678;
        _state.EBX = 0xABCDEF00;
        _state.CS = 0x1000;
        _state.IP = 0x0100;

        // Act
        string result = await _tools.GetCpuRegisters();

        // Assert
        Assert.Contains("EAX: 0x12345678", result);
        Assert.Contains("EBX: 0xABCDEF00", result);
        Assert.Contains("CS: 0x1000", result);
        Assert.Contains("IP: 0x0100", result);
    }

    [Fact]
    public async Task ReadMemory_ReturnsMemoryContents() {
        // Arrange
        uint address = 0x1000;
        _memory.SneakilyWrite(address, 0x90);  // NOP instruction
        _memory.SneakilyWrite(address + 1, 0x90);
        _memory.SneakilyWrite(address + 2, 0xC3);  // RET instruction

        // Act
        string result = await _tools.ReadMemory("0x1000", 16);

        // Assert
        Assert.Contains("0x1000", result);
        Assert.Contains("90", result);
        Assert.Contains("C3", result);
    }

    [Fact]
    public async Task WriteMemory_WritesMemoryCorrectly() {
        // Arrange
        uint address = 0x2000;

        // Act
        string result = await _tools.WriteMemory("0x2000", "90 90 C3");

        // Assert
        Assert.Contains("Successfully wrote 3 byte(s)", result);
        Assert.Equal(0x90, _memory.SneakilyRead(address));
        Assert.Equal(0x90, _memory.SneakilyRead(address + 1));
        Assert.Equal(0xC3, _memory.SneakilyRead(address + 2));
    }

    [Fact]
    public async Task GetEmulatorState_ReturnsState() {
        // Arrange
        _state.CS = 0xF000;
        _state.IP = 0xFFF0;

        // Act
        string result = await _tools.GetEmulatorState();

        // Assert
        Assert.Contains("Current IP: 0xFFF0", result);
        Assert.Contains("Current CS: 0xF000", result);
        Assert.Contains("Cycles:", result);
    }

    [Fact]
    public async Task Disassemble_ReturnsDisassembly() {
        // Arrange
        uint address = 0x3000;
        _memory.SneakilyWrite(address, 0x90);      // NOP
        _memory.SneakilyWrite(address + 1, 0xB8);  // MOV AX, imm16
        _memory.SneakilyWrite(address + 2, 0x00);
        _memory.SneakilyWrite(address + 3, 0x00);
        _memory.SneakilyWrite(address + 4, 0xC3);  // RET

        // Act
        string result = await _tools.Disassemble("0x3000", 3);

        // Assert
        Assert.Contains("nop", result.ToLower());
        Assert.Contains("mov", result.ToLower());
        Assert.Contains("ret", result.ToLower());
    }

    [Fact]
    public async Task AddBreakpoint_AddsBreakpointSuccessfully() {
        // Act
        string result = await _tools.AddBreakpoint("0x5000");

        // Assert
        Assert.Contains("Added execution breakpoint at 0x5000", result);
    }

    [Fact]
    public async Task RemoveBreakpoint_RemovesBreakpointSuccessfully() {
        // Act
        string result = await _tools.RemoveBreakpoint("0x6000");

        // Assert
        Assert.Contains("Removed execution breakpoint at 0x6000", result);
    }

    [Fact]
    public async Task ReadMemory_InvalidAddress_ReturnsError() {
        // Act
        string result = await _tools.ReadMemory("invalid", 16);

        // Assert
        Assert.Contains("Error: Invalid address format", result);
    }

    [Fact]
    public async Task WriteMemory_InvalidHexBytes_ReturnsError() {
        // Act
        string result = await _tools.WriteMemory("0x1000", "ZZ");

        // Assert
        Assert.Contains("Error: Invalid hex byte", result);
    }

    private sealed class TestPauseHandler : IPauseHandler {
        private readonly ILoggerService _loggerService;

        public TestPauseHandler(ILoggerService loggerService) {
            _loggerService = loggerService;
        }

        public bool IsPaused => false;

        public event Action? Pausing;
        public event Action? Paused;
        public event Action? Resumed;

        public void RequestPause(string? reason = null) {
            Pausing?.Invoke();
            Paused?.Invoke();
        }

        public void Resume() {
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
