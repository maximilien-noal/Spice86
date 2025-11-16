namespace Spice86.Core.Emulator.CPU.CfgCpu.Exceptions;

/// <summary>
/// Represents the UnhandledCfgDiscrepancyException class.
/// </summary>
public class UnhandledCfgDiscrepancyException : Exception {
    public UnhandledCfgDiscrepancyException(string message) : base(message) {
    }
}