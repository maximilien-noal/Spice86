namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;

/// <summary>
/// InstructionSuccessorType enumeration.
/// </summary>
public enum InstructionSuccessorType {
    Normal,
    CallToReturn,
    CallToMisalignedReturn,
    CpuFault
}