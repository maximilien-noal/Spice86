namespace Spice86.Core.Emulator.CPU.CfgCpu.Exceptions;

/// <summary>
/// Represents unhandled cfg discrepancy exception.
/// </summary>
public class UnhandledCfgDiscrepancyException : Exception {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="message">The message.</param>
    public UnhandledCfgDiscrepancyException(string message) : base(message) {
    }
}