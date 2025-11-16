namespace Spice86.Core.Emulator.Function.Dump;

/// <summary>
/// Defines the contract for IExecutionDumpFactory.
/// </summary>
public interface IExecutionDumpFactory {
    /// <summary>
    /// Dump method.
    /// </summary>
    public ExecutionDump Dump();
}