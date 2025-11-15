namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.Interfaces;

using System.Numerics;

/// <summary>
/// Defines the contract for i instruction with value field.
/// </summary>
public interface IInstructionWithValueField<T> where T : INumberBase<T> {
    /// <summary>
    /// Gets value field.
    /// </summary>
    public InstructionField<T> ValueField { get; }
}