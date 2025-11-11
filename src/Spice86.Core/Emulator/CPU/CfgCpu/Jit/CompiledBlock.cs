namespace Spice86.Core.Emulator.CPU.CfgCpu.Jit;

using Spice86.Core.Emulator.CPU.CfgCpu.InstructionExecutor;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a compiled basic block of instructions
/// </summary>
public sealed record CompiledBlock(
    IReadOnlyList<CfgInstruction> Instructions,
    Action<InstructionExecutionHelper> CompiledMethod) {
    
    /// <summary>
    /// The last instruction in the compiled block
    /// </summary>
    public CfgInstruction LastInstruction { get; } = Instructions[^1];
}
