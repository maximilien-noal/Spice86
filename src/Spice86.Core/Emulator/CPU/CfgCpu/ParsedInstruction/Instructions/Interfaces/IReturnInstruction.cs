namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.Interfaces;

/// <summary>
/// Common interface for instructions that implement a function return 
/// </summary>
public interface IReturnInstruction : ICfgInstruction {
    /// <summary>
    /// Gets or sets current corresponding call instruction.
    /// </summary>
    public CfgInstruction? CurrentCorrespondingCallInstruction { get; set; }
}