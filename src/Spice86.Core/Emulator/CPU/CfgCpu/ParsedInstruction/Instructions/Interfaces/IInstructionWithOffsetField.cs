namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.Interfaces;

/// <summary>
/// Defines the contract for IInstructionWithOffsetField.
/// </summary>
public interface IInstructionWithOffsetField<T> {
    /// <summary>
    /// Gets or sets the OffsetField.
    /// </summary>
    public InstructionField<T> OffsetField { get; }
}