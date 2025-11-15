namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.Interfaces;

/// <summary>
/// Defines the contract for i instruction with segment register index.
/// </summary>
public interface IInstructionWithSegmentRegisterIndex {
    /// <summary>
    /// Gets segment register index.
    /// </summary>
    public int SegmentRegisterIndex { get; }
}