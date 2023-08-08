namespace Spice86.Core.Emulator.CPU.CfgCpu;

using Spice86.Core.Emulator.CPU.CfgCpu.ControlFlowGraph;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.SelfModifying;

public interface ICfgNodeVisitor {
    public ICfgNode? NextNode { get; }
    public void Accept(HltInstruction instruction);
    public void Accept(JmpNearImm<sbyte> instruction);
    public void Accept(DiscriminatedNode discriminatedNode);
}