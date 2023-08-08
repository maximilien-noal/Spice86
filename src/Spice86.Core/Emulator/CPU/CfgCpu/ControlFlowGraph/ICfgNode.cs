namespace Spice86.Core.Emulator.CPU.CfgCpu.ControlFlowGraph;

public interface ICfgNode {
    ISet<ICfgNode> Predecessors { get; }
    ISet<ICfgNode> Successors { get; }
    
    public uint PhysicalAddress { get; }
    public bool IsAssembly { get; }
    
    public void UpdateSuccessorCache();


    public void Visit(ICfgNodeVisitor visitor);
}