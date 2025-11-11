namespace Spice86.Tests.PerformanceTests;

using NSubstitute;
using Spice86.Core.CLI;
using Spice86.Core.Emulator.VM;
using Spice86.Shared.Interfaces;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

/// <summary>
/// Test runner for executing performance test binaries and collecting metrics.
/// </summary>
public sealed class PerformanceTestRunner : IDisposable {
    private readonly ITestOutputHelper _output;
    private readonly string _testBinaryPath;
    private Spice86DependencyInjection? _emulator;
    private PerformancePortHandler? _portHandler;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceTestRunner"/> class.
    /// </summary>
    /// <param name="output">XUnit test output helper.</param>
    /// <param name="testBinaryPath">Path to the test binary.</param>
    public PerformanceTestRunner(ITestOutputHelper output, string testBinaryPath) {
        _output = output;
        _testBinaryPath = testBinaryPath;
    }

    /// <summary>
    /// Runs the performance test binary and collects results.
    /// </summary>
    /// <param name="maxInstructions">Maximum number of instructions to execute.</param>
    /// <param name="timeoutSeconds">Timeout in seconds.</param>
    /// <returns>List of performance test results.</returns>
    public List<PerformanceTestResult> RunTest(long maxInstructions = 10_000_000, int timeoutSeconds = 30) {
        _output.WriteLine($"Running performance test: {_testBinaryPath}");
        _output.WriteLine($"Max instructions: {maxInstructions:N0}");

        // Create emulator using Spice86Creator pattern
        var creator = new Spice86.Tests.Spice86Creator(
            binName: _testBinaryPath,
            enableCfgCpu: true,
            enablePit: false,
            recordData: false,
            maxCycles: maxInstructions,
            installInterruptVectors: true,
            failOnUnhandledPort: false);
        
        _emulator = creator.Create();
        
        // Create a simple logger for the port handler
        var loggerService = NSubstitute.Substitute.For<ILoggerService>();
        _portHandler = new PerformancePortHandler(loggerService);

        // Register port handlers (0x90-0x9F)
        var ioDispatcher = _emulator.Machine.IoPortDispatcher;
        for (ushort port = 0x90; port <= 0x9F; port++) {
            ioDispatcher.AddIOPortHandler(port, _portHandler);
        }

        // Set up execution timer
        var stopwatch = Stopwatch.StartNew();

        _output.WriteLine("Starting execution...");

        try {
            // Run the emulator using ProgramExecutor
            _emulator.ProgramExecutor.Run();

            stopwatch.Stop();

            // Log results
            _output.WriteLine($"Execution completed:");
            _output.WriteLine($"  Time elapsed: {stopwatch.ElapsedMilliseconds}ms");
            _output.WriteLine($"  Tests completed: {_portHandler.Results.Count}");
            _output.WriteLine($"  All tests complete: {_portHandler.AllTestsComplete}");

            foreach (var result in _portHandler.Results) {
                _output.WriteLine($"  Test {result.TestId} ({result.TestName}): Cycles={result.Cycles}, Result=0x{result.Result:X8}");
            }

            return _portHandler.Results;
        } catch (Exception ex) {
            _output.WriteLine($"Error during execution: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    /// <inheritdoc/>
    public void Dispose() {
        if (!_disposed) {
            _emulator?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
