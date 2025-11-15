namespace Spice86.Core.Emulator.CPU;

using Spice86.Core.Emulator.Errors;

/// <summary>
/// Represents memory address mandatory exception.
/// </summary>
public class MemoryAddressMandatoryException : InvalidVMOperationException {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="state">The state.</param>
    public MemoryAddressMandatoryException(State state) : base(state,
        "Memory address is mandatory for this instruction") {
    }
}