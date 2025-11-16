namespace Spice86.Core.Emulator.CPU.CfgCpu.ControlFlowGraph;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Builder;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Instruction;
using Spice86.Core.Emulator.CPU.CfgCpu.InstructionExecutor;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// The class.
/// </summary>
public abstract class CfgNode : ICfgNode {
    private static int _nextId;
    public CfgNode(SegmentedAddress address, int? maxSuccessorsCount) {
        Address = address;
        Id = _nextId++;
        MaxSuccessorsCount = maxSuccessorsCount;
    }

    /// <summary>
    /// Gets or sets the Id.
    /// </summary>
    public int Id { get; }
    /// <summary>
    /// Predecessors method.
    /// </summary>
    public HashSet<ICfgNode> Predecessors { get; } = new();
    /// <summary>
    /// Successors method.
    /// </summary>
    public HashSet<ICfgNode> Successors { get; } = new();
    /// <summary>
    /// Gets or sets the Address.
    /// </summary>
    public SegmentedAddress Address { get; }
    /// <summary>
    /// The bool.
    /// </summary>
    public virtual bool CanCauseContextRestore => false;

    /// <summary>
    /// Gets or sets the bool.
    /// </summary>
    public abstract bool IsLive { get; }

    /// <summary>
    /// void method.
    /// </summary>
    public abstract void UpdateSuccessorCache();

    /// <summary>
    /// void method.
    /// </summary>
    public abstract void Execute(InstructionExecutionHelper helper);

    /// <summary>
    /// InstructionNode method.
    /// </summary>
    public abstract InstructionNode ToInstructionAst(AstBuilder builder);

    public int? MaxSuccessorsCount { get; set; }

    /// <summary>
    /// Gets or sets the CanHaveMoreSuccessors.
    /// </summary>
    public bool CanHaveMoreSuccessors { get; set; } = true;

    public ICfgNode? UniqueSuccessor { get; set; }

    /// <summary>
    /// string method.
    /// </summary>
    public override string ToString() {
        return $"CfgNode of type {GetType()} with address {Address} and id {Id} IsLive {IsLive}";
    }
}