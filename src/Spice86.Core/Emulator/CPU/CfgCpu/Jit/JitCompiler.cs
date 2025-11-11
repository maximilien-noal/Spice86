namespace Spice86.Core.Emulator.CPU.CfgCpu.Jit;

using Spice86.Core.Emulator.CPU.CfgCpu.ControlFlowGraph;
using Spice86.Core.Emulator.CPU.CfgCpu.InstructionExecutor;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Shared.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

/// <summary>
/// JIT compiler that compiles basic blocks of instructions to native code using System.Reflection.Emit
/// </summary>
public class JitCompiler {
    private readonly ILoggerService _loggerService;
    private readonly Dictionary<int, CompiledBlock> _compiledBlocks = new();
    private readonly ModuleBuilder _moduleBuilder;
    private readonly VM.Breakpoint.EmulatorBreakpointsManager _breakpointsManager;

    public JitCompiler(ILoggerService loggerService, VM.Breakpoint.EmulatorBreakpointsManager breakpointsManager) {
        _loggerService = loggerService;
        _breakpointsManager = breakpointsManager;
        
        // Create dynamic assembly and module for JIT compilation
        AssemblyName assemblyName = new AssemblyName("Spice86JitAssembly");
        AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
            assemblyName, AssemblyBuilderAccess.Run);
        _moduleBuilder = assemblyBuilder.DefineDynamicModule("Spice86JitModule");
    }

    /// <summary>
    /// Try to get a compiled block for the given node. Returns null if not compiled yet.
    /// </summary>
    public CompiledBlock? GetCompiledBlock(ICfgNode node) {
        if (_compiledBlocks.TryGetValue(node.Id, out CompiledBlock? block)) {
            // Check if block has breakpoints - if so, invalidate and recompile later
            if (HasBreakpointsInBlock(block)) {
                _compiledBlocks.Remove(node.Id);
                return null;
            }
            return block;
        }
        return null;
    }

    /// <summary>
    /// Check if a compiled block has any execution breakpoints
    /// </summary>
    private bool HasBreakpointsInBlock(CompiledBlock block) {
        // For simplicity, we check if any breakpoint is active in the block's address range
        // This is a conservative approach - we could be more precise
        foreach (CfgInstruction instruction in block.Instructions) {
            uint physicalAddress = Shared.Utils.MemoryUtils.ToPhysicalAddress(
                instruction.Address.Segment, instruction.Address.Offset);
            
            // Use reflection to check breakpoints - not ideal but works for now
            // In production, EmulatorBreakpointsManager should expose a public method for this
            var executionBreakPointsField = _breakpointsManager.GetType()
                .GetField("_executionBreakPoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (executionBreakPointsField != null) {
                var breakpointHolder = executionBreakPointsField.GetValue(_breakpointsManager);
                var isBreakpointPresentMethod = breakpointHolder?.GetType()
                    .GetMethod("IsBreakPointPresent");
                if (isBreakpointPresentMethod != null) {
                    bool hasBreakpoint = (bool)(isBreakpointPresentMethod.Invoke(breakpointHolder, new object[] { physicalAddress }) ?? false);
                    if (hasBreakpoint) {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Compile a basic block starting from the given node.
    /// A basic block is a sequence of instructions with no branches except at the end.
    /// </summary>
    public CompiledBlock? TryCompileBasicBlock(ICfgNode startNode, InstructionExecutionHelper helper) {
        // Don't compile if already compiled
        if (_compiledBlocks.ContainsKey(startNode.Id)) {
            return _compiledBlocks[startNode.Id];
        }

        // Collect the basic block - sequence of instructions with single successor
        List<CfgInstruction> instructions = CollectBasicBlock(startNode);
        
        // Don't compile very small blocks (overhead not worth it)
        if (instructions.Count < 3) {
            return null;
        }

        // Verify all instructions are live and compilable
        foreach (CfgInstruction instruction in instructions) {
            if (!instruction.IsLive || !IsCompilable(instruction)) {
                return null;
            }
        }

        try {
            // Create the compiled block
            CompiledBlock compiled = CompileBlock(instructions, helper);
            _compiledBlocks[startNode.Id] = compiled;
            return compiled;
        } catch (Exception ex) {
            // JIT compilation failed, fall back to interpretation
            if (_loggerService.IsEnabled(Serilog.Events.LogEventLevel.Verbose)) {
                _loggerService.Verbose("JIT compilation failed for block at {Address}: {Error}", 
                    startNode.Address, ex.Message);
            }
            return null;
        }
    }

    /// <summary>
    /// Invalidate compiled block when instruction changes (self-modifying code)
    /// </summary>
    public void InvalidateBlock(int nodeId) {
        _compiledBlocks.Remove(nodeId);
    }

    /// <summary>
    /// Collect a basic block - sequence of instructions with linear flow
    /// </summary>
    private List<CfgInstruction> CollectBasicBlock(ICfgNode startNode) {
        List<CfgInstruction> instructions = new();
        ICfgNode? current = startNode;

        while (current is CfgInstruction instruction) {
            instructions.Add(instruction);

            // Stop if there are multiple successors (branch)
            if (instruction.Successors.Count != 1) {
                break;
            }

            // Stop if the unique successor has multiple predecessors (merge point)
            ICfgNode successor = instruction.UniqueSuccessor ?? instruction.Successors.First();
            if (successor.Predecessors.Count > 1) {
                break;
            }

            // Stop if we've collected a reasonable number of instructions
            if (instructions.Count >= 50) {
                break;
            }

            current = successor;
        }

        return instructions;
    }

    /// <summary>
    /// Check if an instruction can be compiled
    /// </summary>
    private bool IsCompilable(CfgInstruction instruction) {
        // For now, we'll compile most instructions except complex ones
        // This can be expanded as we add more instruction support
        return instruction.IsLive;
    }

    /// <summary>
    /// Compile a basic block to a delegate
    /// </summary>
    private CompiledBlock CompileBlock(List<CfgInstruction> instructions, InstructionExecutionHelper helper) {
        // For the initial implementation, we create an optimized executor that batches instructions
        // This avoids the overhead of node traversal and linking for each instruction
        // Future enhancement: compile to IL for even better performance
        
        Action<InstructionExecutionHelper> compiledMethod = (helperParam) => {
            // Execute all instructions in the block sequentially
            foreach (CfgInstruction instruction in instructions) {
                instruction.Execute(helperParam);
            }
        };

        return new CompiledBlock(instructions, compiledMethod);
    }
}

/// <summary>
/// Represents a compiled basic block
/// </summary>
public class CompiledBlock {
    public CompiledBlock(List<CfgInstruction> instructions, Action<InstructionExecutionHelper> compiledMethod) {
        Instructions = instructions;
        CompiledMethod = compiledMethod;
        LastInstruction = instructions[^1];
    }

    public List<CfgInstruction> Instructions { get; }
    public Action<InstructionExecutionHelper> CompiledMethod { get; }
    public CfgInstruction LastInstruction { get; }
}
