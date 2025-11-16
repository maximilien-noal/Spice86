namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.Interfaces;

using System.Numerics;

/// <summary>
/// Defines the contract for IInstructionWithValueField.
/// </summary>
public interface IInstructionWithValueField<T> where T : INumberBase<T> {
    /// <summary>
    /// Gets or sets the ValueField.
    /// </summary>
    public InstructionField<T> ValueField { get; }
}