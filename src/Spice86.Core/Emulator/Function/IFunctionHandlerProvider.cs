namespace Spice86.Core.Emulator.Function;

/// <summary>
/// Defines the contract for IFunctionHandlerProvider.
/// </summary>
public interface IFunctionHandlerProvider {
    /// <summary>
    /// Gets or sets the FunctionHandlerInUse.
    /// </summary>
    public FunctionHandler FunctionHandlerInUse { get; }
    /// <summary>
    /// Gets or sets the IsInitialExecutionContext.
    /// </summary>
    public bool IsInitialExecutionContext { get; }
}