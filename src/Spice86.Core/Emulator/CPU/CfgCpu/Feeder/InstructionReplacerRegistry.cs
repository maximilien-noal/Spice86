namespace Spice86.Core.Emulator.CPU.CfgCpu.Feeder;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;

/// <summary>
/// Represents instruction replacer registry.
/// </summary>
public class InstructionReplacerRegistry : IInstructionReplacer {
    private List<InstructionReplacer> _replacers = new();

    /// <summary>
    /// Performs the register operation.
    /// </summary>
    /// <param name="replacer">The replacer.</param>
    public void Register(InstructionReplacer replacer) {
        _replacers.Add(replacer);
    }

    /// <summary>
    /// Performs the replace instruction operation.
    /// </summary>
    /// <param name="oldInstruction">The old instruction.</param>
    /// <param name="newInstruction">The new instruction.</param>
    public void ReplaceInstruction(CfgInstruction oldInstruction, CfgInstruction newInstruction) {
        foreach (InstructionReplacer instructionReplacer in _replacers) {
            instructionReplacer.ReplaceInstruction(oldInstruction, newInstruction);
        }
    }
}