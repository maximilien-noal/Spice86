namespace Spice86.Core.Emulator.Function.Dump;

/// <summary>
/// Defines the contract for i execution dump factory.
/// </summary>
public interface IExecutionDumpFactory {
    public ExecutionDump Dump();
}