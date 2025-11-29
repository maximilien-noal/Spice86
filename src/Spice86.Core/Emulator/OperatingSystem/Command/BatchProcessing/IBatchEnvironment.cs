namespace Spice86.Core.Emulator.OperatingSystem.Command.BatchProcessing;

/// <summary>
/// Provides access to environment variables for batch file expansion.
/// </summary>
/// <remarks>
/// This abstraction allows environment variable access through different sources:
/// - Test environment (for unit testing)
/// - DOS environment (for full emulation integration)
/// Based on DOSBox staging's Environment interface pattern.
/// </remarks>
public interface IBatchEnvironment {
    /// <summary>
    /// Gets the value of an environment variable.
    /// </summary>
    /// <param name="name">The name of the environment variable (case-insensitive).</param>
    /// <returns>The value of the variable, or null if not found.</returns>
    string? GetEnvironmentValue(string name);
}
