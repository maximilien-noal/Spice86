namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.Interfaces;

/// <summary>
/// Defines the contract for i instruction with register index.
/// </summary>
public interface IInstructionWithRegisterIndex {
    /// <summary>
    /// Gets register index.
    /// </summary>
    public int RegisterIndex { get; }
}