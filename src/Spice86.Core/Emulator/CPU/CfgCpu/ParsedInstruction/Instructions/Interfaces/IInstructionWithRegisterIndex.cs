namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.Interfaces;

/// <summary>
/// Defines the contract for IInstructionWithRegisterIndex.
/// </summary>
public interface IInstructionWithRegisterIndex {
    /// <summary>
    /// Gets or sets the RegisterIndex.
    /// </summary>
    public int RegisterIndex { get; }
}