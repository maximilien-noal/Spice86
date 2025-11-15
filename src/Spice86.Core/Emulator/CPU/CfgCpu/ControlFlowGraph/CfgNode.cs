namespace Spice86.Core.Emulator.CPU.CfgCpu.ControlFlowGraph;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Builder;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Instruction;
using Spice86.Core.Emulator.CPU.CfgCpu.InstructionExecutor;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents cfg node.
/// </summary>
public abstract class CfgNode : ICfgNode {
    private static int _nextId;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="maxSuccessorsCount">The max successors count.</param>
    public CfgNode(SegmentedAddress address, int? maxSuccessorsCount) {
        Address = address;
        Id = _nextId++;
        MaxSuccessorsCount = maxSuccessorsCount;
    }

    /// <summary>
    /// Gets id.
    /// </summary>
    public int Id { get; }
    /// <summary>
    /// Gets predecessors.
    /// </summary>
    public HashSet<ICfgNode> Predecessors { get; } = new();
    /// <summary>
    /// Gets successors.
    /// </summary>
    public HashSet<ICfgNode> Successors { get; } = new();
    /// <summary>
    /// Gets address.
    /// </summary>
    public SegmentedAddress Address { get; }
    /// <summary>
    /// The can cause context restore.
    /// </summary>
    public virtual bool CanCauseContextRestore => false;

    /// <summary>
    /// Gets is live.
    /// </summary>
    public abstract bool IsLive { get; }

    public abstract void UpdateSuccessorCache();

    public abstract void Execute(InstructionExecutionHelper helper);

    public abstract InstructionNode ToInstructionAst(AstBuilder builder);

    /// <summary>
    /// Gets or sets max successors count.
    /// </summary>
    public int? MaxSuccessorsCount { get; set; }

    /// <summary>
    /// Gets or sets can have more successors.
    /// </summary>
    public bool CanHaveMoreSuccessors { get; set; } = true;

    /// <summary>
    /// Gets or sets unique successor.
    /// </summary>
    public ICfgNode? UniqueSuccessor { get; set; }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    public override string ToString() {
        return $"CfgNode of type {GetType()} with address {Address} and id {Id} IsLive {IsLive}";
    }
}