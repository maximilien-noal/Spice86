namespace Spice86.Core.Emulator.CPU.CfgCpu.Feeder;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;

/// <summary>
/// The class.
/// </summary>
public abstract class InstructionReplacer : IInstructionReplacer {
    protected InstructionReplacer(InstructionReplacerRegistry replacerRegistry) {
        replacerRegistry.Register(this);
    }
    /// <summary>
    /// void method.
    /// </summary>
    public abstract void ReplaceInstruction(CfgInstruction oldInstruction, CfgInstruction newInstruction);
}