namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.Interfaces;

/// <summary>
/// Defines the contract for IInstructionWithSegmentRegisterIndex.
/// </summary>
public interface IInstructionWithSegmentRegisterIndex {
    /// <summary>
    /// Gets or sets the SegmentRegisterIndex.
    /// </summary>
    public int SegmentRegisterIndex { get; }
}