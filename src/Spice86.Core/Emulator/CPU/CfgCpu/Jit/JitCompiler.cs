namespace Spice86.Core.Emulator.CPU.CfgCpu.Jit;

using Serilog.Events;

using Spice86.Core.Emulator.CPU.CfgCpu.ControlFlowGraph;
using Spice86.Core.Emulator.CPU.CfgCpu.InstructionExecutor;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.Memory;
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
    private readonly IMemory _memory;

    public JitCompiler(ILoggerService loggerService, EmulatorBreakpointsManager breakpointsManager, IMemory memory) {
        _loggerService = loggerService;
        _breakpointsManager = breakpointsManager;
        _memory = memory;
    }

    /// <inheritdoc />
    public bool TryGetCompiledBlock(ICfgNode node, out CompiledBlock? compiledBlock) {
        if (_compiledBlocks.TryGetValue(node.Id, out CompiledBlock? block)) {
            if (!AllInstructionsAreStillLive(block)) {
                _compiledBlocks.Remove(node.Id);
                compiledBlock = null;
                return false;
            }
            
            if (BlockContainsExecutionBreakpoint(block)) {
                _compiledBlocks.Remove(node.Id);
                compiledBlock = null;
                return false;
            }
            
            compiledBlock = block;
            return true;
        }
        
        compiledBlock = null;
        return false;
    }

    /// <summary>
    /// Check if all instructions in the compiled block match their memory representation.
    /// Also verifies that instruction bytes haven't been modified in memory.
    /// </summary>
    private bool AllInstructionsAreStillLive(CompiledBlock block) {
        // First check the IsLive flag which is updated by CfgCpu
        if (!block.Instructions.All(instruction => instruction.IsLive)) {
            return false;
        }
        
        // Additionally verify that instruction bytes match memory
        // This catches cases where code was overwritten but CfgCpu hasn't re-parsed yet
        foreach (CfgInstruction instruction in block.Instructions) {
            if (!InstructionBytesMatchMemory(instruction)) {
                // Instruction bytes have changed in memory, mark as not live
                instruction.SetLive(false);
                return false;
            }
        }
        
        return true;
    }

    /// <summary>
    /// Check if an instruction's bytes in memory match its parsed representation
    /// </summary>
    private bool InstructionBytesMatchMemory(CfgInstruction instruction) {
        uint physicalAddress = MemoryUtils.ToPhysicalAddress(
            instruction.Address.Segment, instruction.Address.Offset);
        
        foreach (FieldWithValue field in instruction.FieldsInOrder) {
            for (int i = 0; i < field.SignatureValue.Count; i++) {
                byte? expectedByte = field.SignatureValue[i];
                if (expectedByte is null) {
                    // Field not read from memory (e.g., computed value)
                    continue;
                }
                
                uint byteAddress = (uint)(field.PhysicalAddress + i);
                byte actualByte = _memory.UInt8[byteAddress];
                
                if (actualByte != expectedByte.Value) {
                    if (_loggerService.IsEnabled(LogEventLevel.Verbose)) {
                        _loggerService.Verbose("JIT: Instruction at {Address} byte mismatch at offset {Offset}: expected {Expected:X2}, got {Actual:X2}", 
                            instruction.Address, i, expectedByte.Value, actualByte);
                    }
                    return false;
                }
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
    public bool TryCompileBasicBlock(ICfgNode startNode, InstructionExecutionHelper helper, out CompiledBlock? compiledBlock) {
        if (_compiledBlocks.TryGetValue(startNode.Id, out CompiledBlock? existingBlock)) {
            compiledBlock = existingBlock;
            return true;
        }

        List<CfgInstruction> instructions = CollectBasicBlock(startNode);
        
        if (instructions.Count < MinimumBlockSize) {
            compiledBlock = null;
            return false;
        }

        if (!AllInstructionsAreCompilable(instructions)) {
            compiledBlock = null;
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
            compiledBlock = null;
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
            // Check if this instruction has multiple successors BEFORE adding it
            // Don't include branch instructions in compiled blocks
            if (HasMultipleSuccessors(instruction)) {
                break;
            }

            instructions.Add(instruction);

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
