namespace Spice86.Core.Emulator.CPU.CfgCpu.Jit;

using Spice86.Core.Emulator.CPU.CfgCpu.ControlFlowGraph;
using Spice86.Core.Emulator.CPU.CfgCpu.InstructionExecutor;

/// <summary>
/// Interface for JIT compiler that compiles basic blocks of instructions
/// </summary>
public interface IJitCompiler {
    /// <summary>
    /// Try to get a compiled block for the given node
    /// </summary>
    /// <param name="node">The CFG node to check for compilation</param>
    /// <param name="compiledBlock">The compiled block if found and valid</param>
    /// <returns>True if a valid compiled block exists</returns>
    bool TryGetCompiledBlock(ICfgNode node, out CompiledBlock? compiledBlock);

    /// <summary>
    /// Try to compile a basic block starting from the given node
    /// </summary>
    /// <param name="startNode">The starting node for the basic block</param>
    /// <param name="helper">Instruction execution helper</param>
    /// <param name="compiledBlock">The compiled block if compilation succeeded</param>
    /// <returns>True if compilation succeeded</returns>
    bool TryCompileBasicBlock(ICfgNode startNode, InstructionExecutionHelper helper, out CompiledBlock? compiledBlock);

    /// <summary>
    /// Invalidate a compiled block when instruction changes (self-modifying code)
    /// </summary>
    /// <param name="nodeId">The node ID to invalidate</param>
    void InvalidateBlock(int nodeId);
}
