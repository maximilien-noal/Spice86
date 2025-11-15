namespace Spice86.Core.Emulator.CPU.CfgCpu.Feeder;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;

/// <summary>
/// Represents instruction replacer.
/// </summary>
public abstract class InstructionReplacer : IInstructionReplacer {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="replacerRegistry">The replacer registry.</param>
    protected InstructionReplacer(InstructionReplacerRegistry replacerRegistry) {
        replacerRegistry.Register(this);
    }
    public abstract void ReplaceInstruction(CfgInstruction oldInstruction, CfgInstruction newInstruction);
}