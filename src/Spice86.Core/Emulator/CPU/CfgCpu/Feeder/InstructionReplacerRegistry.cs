namespace Spice86.Core.Emulator.CPU.CfgCpu.Feeder;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;

/// <summary>
/// Represents the InstructionReplacerRegistry class.
/// </summary>
public class InstructionReplacerRegistry : IInstructionReplacer {
    private List<InstructionReplacer> _replacers = new();

    /// <summary>
    /// Register method.
    /// </summary>
    public void Register(InstructionReplacer replacer) {
        _replacers.Add(replacer);
    }

    /// <summary>
    /// ReplaceInstruction method.
    /// </summary>
    public void ReplaceInstruction(CfgInstruction oldInstruction, CfgInstruction newInstruction) {
        foreach (InstructionReplacer instructionReplacer in _replacers) {
            instructionReplacer.ReplaceInstruction(oldInstruction, newInstruction);
        }
    }
}