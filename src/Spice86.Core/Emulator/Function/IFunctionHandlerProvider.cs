namespace Spice86.Core.Emulator.Function;

/// <summary>
/// Defines the contract for i function handler provider.
/// </summary>
public interface IFunctionHandlerProvider {
    /// <summary>
    /// Gets function handler in use.
    /// </summary>
    public FunctionHandler FunctionHandlerInUse { get; }
    /// <summary>
    /// Gets is initial execution context.
    /// </summary>
    public bool IsInitialExecutionContext { get; }
}