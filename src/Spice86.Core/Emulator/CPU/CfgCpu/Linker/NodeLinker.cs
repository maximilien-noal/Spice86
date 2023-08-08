namespace Spice86.Core.Emulator.CPU.CfgCpu.Linker;

using Spice86.Core.Emulator.CPU.CfgCpu.ControlFlowGraph;
using Spice86.Core.Emulator.CPU.CfgCpu.Feeder;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.SelfModifying;

public class NodeLinker : IInstructionReplacer<ICfgNode> {
    /// <summary>
    /// Ensure current and next are linked together.
    /// </summary>
    /// <param name="current"></param>
    /// <param name="next"></param>
    public void Link(ICfgNode current, ICfgNode next) {
        if (current is CfgInstruction currentCfgInstruction) {
            LinkCfgInstruction(currentCfgInstruction, next);
        } else if (current is DiscriminatedNode discriminatedNode) {
            LinkDiscriminatedNode(discriminatedNode, next);
        }
    }

    private void LinkCfgInstruction(CfgInstruction current, ICfgNode next) {
        IDictionary<uint, ICfgNode> successors = current.SuccessorsPerAddress;
        if (!successors.TryGetValue(next.PhysicalAddress, out ICfgNode? shouldBeNext)) {
            AttachCurrentToNext(current, next);
            return;
        }

        if (shouldBeNext != next) {
            // Discrepancy was not detected by CfgNodeFeeder. CRASH!!!
        }
    }

    private void LinkDiscriminatedNode(DiscriminatedNode current, ICfgNode next) {
        if (next is CfgInstruction nextCfgInstruction) {
            IDictionary<InstructionDiscriminator, CfgInstruction> successors = current.SuccessorsPerDiscriminator;
            if (!successors.TryGetValue(nextCfgInstruction.Discriminator, out CfgInstruction? shouldBeNextOrNull)) {
                AttachCurrentToNext(current, next);
                return;
            }
            if (shouldBeNextOrNull != next) {
                // Means a different instruction with the same discriminator and address was in the cache but not in the successors of this node. CRASH!!!
            }
        } else {
            // Means we are trying to attach a non ASM instruction to a discriminated node which is not allowed. CRASH!!!
        }
    }

    private void AttachCurrentToNext(ICfgNode current, ICfgNode next) {
        LinkCurrentToNext(current, next);
        current.UpdateSuccessorCache();
    }

    public void LinkCurrentToNext(ICfgNode current, ICfgNode next) {
        current.Successors.Add(next);
        next.Predecessors.Add(current);
        current.UpdateSuccessorCache();
    }

    public void ReplaceInstruction(ICfgNode old, ICfgNode instruction) {
        ReplacePredecessors(old, instruction);
        foreach (ICfgNode successor in old.Successors) {
            LinkCurrentToNext(instruction, successor);
            successor.Predecessors.Remove(old);
            old.Successors.Remove(successor);
        }

        instruction.UpdateSuccessorCache();
        old.UpdateSuccessorCache();
    }

    public void ReplacePredecessors(ICfgNode old, ICfgNode instruction) {
        foreach (ICfgNode predecessor in old.Predecessors) {
            predecessor.Successors.Remove(old);
            LinkCurrentToNext(predecessor, instruction);
        }
        old.Predecessors.Clear();
        instruction.UpdateSuccessorCache();
        old.UpdateSuccessorCache();
    }
}