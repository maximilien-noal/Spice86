namespace Spice86.Core.Emulator.CPU.CfgCpu.Jit;

using Serilog.Events;

using Spice86.Core.Emulator.CPU.CfgCpu.ControlFlowGraph;
using Spice86.Core.Emulator.CPU.CfgCpu.InstructionExecutor;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.VM.Breakpoint;
using Spice86.Shared.Interfaces;
using Spice86.Shared.Utils;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// JIT compiler that compiles basic blocks of instructions into cached delegates
/// </summary>
public class JitCompiler : IJitCompiler {
    private const int MinimumBlockSize = 3;
    private const int MaximumBlockSize = 50;
    
    private readonly ILoggerService _loggerService;
    private readonly Dictionary<int, CompiledBlock> _compiledBlocks = new();
    private readonly EmulatorBreakpointsManager _breakpointsManager;

    public JitCompiler(ILoggerService loggerService, EmulatorBreakpointsManager breakpointsManager) {
        _loggerService = loggerService;
        _breakpointsManager = breakpointsManager;
    }

    /// <inheritdoc />
    public bool TryGetCompiledBlock(ICfgNode node, out CompiledBlock compiledBlock) {
        if (_compiledBlocks.TryGetValue(node.Id, out CompiledBlock? block)) {
            if (!AllInstructionsAreStillLive(block)) {
                _compiledBlocks.Remove(node.Id);
                compiledBlock = null!;
                return false;
            }
            
            if (BlockContainsExecutionBreakpoint(block)) {
                _compiledBlocks.Remove(node.Id);
                compiledBlock = null!;
                return false;
            }
            
            compiledBlock = block;
            return true;
        }
        
        compiledBlock = null!;
        return false;
    }

    /// <summary>
    /// Check if all instructions in the compiled block match their memory representation
    /// </summary>
    private bool AllInstructionsAreStillLive(CompiledBlock block) {
        foreach (CfgInstruction instruction in block.Instructions) {
            if (!instruction.IsLive) {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Check if the compiled block contains any execution breakpoints
    /// </summary>
    private bool BlockContainsExecutionBreakpoint(CompiledBlock block) {
        foreach (CfgInstruction instruction in block.Instructions) {
            uint physicalAddress = MemoryUtils.ToPhysicalAddress(
                instruction.Address.Segment, instruction.Address.Offset);
            
            if (_breakpointsManager.HasExecutionBreakpoint(physicalAddress)) {
                return true;
            }
        }
        return false;
    }

    /// <inheritdoc />
    public bool TryCompileBasicBlock(ICfgNode startNode, InstructionExecutionHelper helper, out CompiledBlock compiledBlock) {
        if (_compiledBlocks.TryGetValue(startNode.Id, out CompiledBlock? existingBlock)) {
            compiledBlock = existingBlock;
            return true;
        }

        List<CfgInstruction> instructions = CollectBasicBlock(startNode);
        
        if (instructions.Count < MinimumBlockSize) {
            compiledBlock = null!;
            return false;
        }

        if (!AllInstructionsAreCompilable(instructions)) {
            compiledBlock = null!;
            return false;
        }

        try {
            CompiledBlock compiled = CompileInstructionsToBlock(instructions);
            _compiledBlocks[startNode.Id] = compiled;
            compiledBlock = compiled;
            return true;
        } catch (Exception ex) {
            if (_loggerService.IsEnabled(LogEventLevel.Verbose)) {
                _loggerService.Verbose("JIT compilation failed for block at {Address}: {Error}", 
                    startNode.Address, ex.Message);
            }
            compiledBlock = null!;
            return false;
        }
    }

    private bool AllInstructionsAreCompilable(List<CfgInstruction> instructions) {
        foreach (CfgInstruction instruction in instructions) {
            if (!instruction.IsLive) {
                return false;
            }
        }
        return true;
    }

    /// <inheritdoc />
    public void InvalidateBlock(int nodeId) {
        _compiledBlocks.Remove(nodeId);
    }

    private List<CfgInstruction> CollectBasicBlock(ICfgNode startNode) {
        List<CfgInstruction> instructions = new();
        ICfgNode? current = startNode;

        while (current is CfgInstruction instruction) {
            instructions.Add(instruction);

            if (HasMultipleSuccessors(instruction)) {
                break;
            }

            ICfgNode successor = instruction.UniqueSuccessor ?? instruction.Successors.First();
            if (IsJoinPoint(successor)) {
                break;
            }

            if (instructions.Count >= MaximumBlockSize) {
                break;
            }

            current = successor;
        }

        return instructions;
    }

    private static bool HasMultipleSuccessors(CfgInstruction instruction) {
        return instruction.Successors.Count != 1;
    }

    private static bool IsJoinPoint(ICfgNode node) {
        return node.Predecessors.Count > 1;
    }

    private CompiledBlock CompileInstructionsToBlock(List<CfgInstruction> instructions) {
        Action<InstructionExecutionHelper> compiledMethod = helperParam => {
            foreach (CfgInstruction instruction in instructions) {
                instruction.Execute(helperParam);
            }
        };

        return new CompiledBlock(instructions, compiledMethod);
    }
}
