namespace Spice86.Core.Emulator.CPU.CfgCpu.Feeder;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;

/// <summary>
/// Defines the contract for i instruction replacer.
/// </summary>
public interface IInstructionReplacer {
    void ReplaceInstruction(CfgInstruction oldInstruction, CfgInstruction newInstruction);
}