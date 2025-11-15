namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;

/// <summary>
/// Defines instruction successor type values.
/// </summary>
public enum InstructionSuccessorType {
    Normal,
    CallToReturn,
    CallToMisalignedReturn,
    CpuFault
}