namespace Spice86.Core.Emulator.CPU.CfgCpu.ControlFlowGraph;

public abstract class CfgNode : ICfgNode {
    public CfgNode(uint physicalAddress) {
        PhysicalAddress = physicalAddress;
    }

    public ISet<ICfgNode> Predecessors { get; } = new HashSet<ICfgNode>();
    public ISet<ICfgNode> Successors { get; } = new HashSet<ICfgNode>();
    public uint PhysicalAddress { get; }

    public abstract bool IsAssembly { get; }
    public abstract void UpdateSuccessorCache();


    public abstract void Visit(ICfgNodeVisitor visitor);
}