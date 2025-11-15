namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.Interfaces;

/// <summary>
/// Defines the contract for i instruction with offset field.
/// </summary>
public interface IInstructionWithOffsetField<T> {
    /// <summary>
    /// Gets offset field.
    /// </summary>
    public InstructionField<T> OffsetField { get; }
}