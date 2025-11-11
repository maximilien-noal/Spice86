namespace Spice86.Core.Emulator.CPU.CfgCpu.Jit;

using Spice86.Core.Emulator.CPU.CfgCpu.ControlFlowGraph;
using Spice86.Core.Emulator.CPU.CfgCpu.InstructionExecutor;

/// <summary>
/// Null implementation of IJitCompiler that does nothing
/// </summary>
public class NullJitCompiler : IJitCompiler {
    /// <inheritdoc />
    public bool TryGetCompiledBlock(ICfgNode node, out CompiledBlock? compiledBlock) {
        compiledBlock = null;
        return false;
    }

    /// <inheritdoc />
    public bool TryCompileBasicBlock(ICfgNode startNode, InstructionExecutionHelper helper, out CompiledBlock? compiledBlock) {
        compiledBlock = null;
        return false;
    }

    /// <inheritdoc />
    public void InvalidateBlock(int nodeId) {
        // Do nothing
    }
}
