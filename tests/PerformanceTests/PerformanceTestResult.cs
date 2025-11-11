namespace Spice86.Tests.PerformanceTests;

/// <summary>
/// Represents a single performance test result.
/// </summary>
public record PerformanceTestResult {
    /// <summary>
    /// Gets or sets the test identifier.
    /// </summary>
    public required byte TestId { get; init; }

    /// <summary>
    /// Gets or sets the number of cycles (iterations) executed.
    /// </summary>
    public required uint Cycles { get; init; }

    /// <summary>
    /// Gets or sets the test result value.
    /// </summary>
    public required uint Result { get; init; }

    /// <summary>
    /// Gets or sets the timestamp when the test was executed.
    /// </summary>
    public required DateTime Timestamp { get; init; }

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
