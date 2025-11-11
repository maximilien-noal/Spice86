namespace Spice86.Tests.PerformanceTests;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.IOPorts;
using Spice86.Shared.Interfaces;

/// <summary>
/// Custom I/O port handler for capturing performance test metrics.
/// Handles ports 0x90-0x9F for test reporting.
/// </summary>
public class PerformancePortHandler : IIOPortHandler {
    private readonly ILoggerService _loggerService;
    private byte _testId;
    private ushort _cycleLow;
    private ushort _cycleHigh;
    private ushort _resultLow;
    private ushort _resultHigh;
    private byte _status;
    private ushort _lastPortRead;
    private uint _lastPortWriteValue;

    /// <summary>
    /// Gets the list of completed test results.
    /// </summary>
    public List<PerformanceTestResult> Results { get; } = new();

    /// <summary>
    /// Gets a value indicating whether all tests are complete.
    /// </summary>
    public bool AllTestsComplete { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformancePortHandler"/> class.
    /// </summary>
    /// <param name="loggerService">The logger service.</param>
    public PerformancePortHandler(ILoggerService loggerService) {
        _loggerService = loggerService;
    }

    /// <inheritdoc/>
    public byte ReadByte(ushort port) {
        return port switch {
            0x90 => _testId,
            0x99 => _status,
            _ => 0xFF
        };
    }

    /// <inheritdoc/>
    public ushort ReadWord(ushort port) {
        return port switch {
            0x91 => _cycleLow,
            0x93 => _cycleHigh,
            0x95 => _resultLow,
            0x97 => _resultHigh,
            _ => 0xFFFF
        };
    }

    /// <inheritdoc/>
    public uint ReadDWord(ushort port) {
        return 0xFFFFFFFF;
    }

    /// <inheritdoc/>
    public void WriteByte(ushort port, byte value) {
        switch (port) {
            case 0x90:
                _testId = value;
                if (value == 0xFF) {
                    AllTestsComplete = true;
                    _loggerService.Information("All performance tests completed. Total results: {Count}", Results.Count);
                }
                break;
            case 0x99:
                _status = value;
                if (value == 1) {
                    // Test completed, capture result
                    var result = new PerformanceTestResult {
                        TestId = _testId,
                        Cycles = ((uint)_cycleHigh << 16) | _cycleLow,
                        Result = ((uint)_resultHigh << 16) | _resultLow,
                        Timestamp = DateTime.UtcNow
                    };
                    Results.Add(result);
                    _loggerService.Information("Test {TestId} completed: Cycles={Cycles}, Result=0x{Result:X8}",
                        _testId, result.Cycles, result.Result);
                }
                break;
        }
    }

    /// <inheritdoc/>
    public void WriteWord(ushort port, ushort value) {
        switch (port) {
            case 0x91:
                _cycleLow = value;
                break;
            case 0x93:
                _cycleHigh = value;
                break;
            case 0x95:
                _resultLow = value;
                break;
            case 0x97:
                _resultHigh = value;
                break;
        }
    }

    /// <inheritdoc/>
    public void WriteDWord(ushort port, uint value) {
        // Not used in this implementation
    }

    /// <inheritdoc/>
    public void UpdateLastPortRead(ushort port) {
        _lastPortRead = port;
    }

    /// <inheritdoc/>
    public void UpdateLastPortWrite(ushort port, uint value) {
        _lastPortRead = port;
        _lastPortWriteValue = value;
    }
}

/// <summary>
/// Represents a single performance test result.
/// </summary>
public class PerformanceTestResult {
    /// <summary>
    /// Gets or sets the test identifier.
    /// </summary>
    public byte TestId { get; set; }

    /// <summary>
    /// Gets or sets the number of cycles (iterations) executed.
    /// </summary>
    public uint Cycles { get; set; }

    /// <summary>
    /// Gets or sets the test result value.
    /// </summary>
    public uint Result { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the test was executed.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets the test name based on the test ID.
    /// </summary>
    public string TestName => TestId switch {
        1 => "Integer Arithmetic",
        2 => "Multiplication",
        3 => "Division",
        4 => "Bit Manipulation",
        5 => "Loop Performance",
        _ => $"Unknown Test {TestId}"
    };
}
