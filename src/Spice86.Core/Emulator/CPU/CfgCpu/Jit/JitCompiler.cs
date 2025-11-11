namespace Spice86.Core.Emulator.CPU.CfgCpu.Jit;

using Serilog.Events;

using Spice86.Core.Emulator.CPU.CfgCpu.ControlFlowGraph;
using Spice86.Core.Emulator.CPU.CfgCpu.InstructionExecutor;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.SelfModifying;
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
    // Cache key includes content hash to handle self-modifying code
    private readonly Dictionary<(int NodeId, int ContentHash), CompiledBlock> _compiledBlocks = new();
    private readonly EmulatorBreakpointsManager _breakpointsManager;
    private readonly IMemory _memory;

    public JitCompiler(ILoggerService loggerService, EmulatorBreakpointsManager breakpointsManager, IMemory memory) {
        _loggerService = loggerService;
        _breakpointsManager = breakpointsManager;
        _memory = memory;
    }

    /// <inheritdoc />
    public bool TryGetCompiledBlock(ICfgNode node, out CompiledBlock? compiledBlock) {
        if (node is not CfgInstruction startInstruction) {
            compiledBlock = null;
            return false;
        }
        
        int contentHash = ComputeInstructionContentHash(startInstruction);
        (int NodeId, int ContentHash) key = (node.Id, contentHash);
        
        if (_compiledBlocks.TryGetValue(key, out CompiledBlock? block)) {
            if (!AllInstructionsAreStillLive(block)) {
                _compiledBlocks.Remove(key);
                compiledBlock = null;
                return false;
            }
            
            if (BlockContainsExecutionBreakpoint(block)) {
                _compiledBlocks.Remove(key);
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
    /// Compute a hash of the instruction's bytes to detect code changes
    /// </summary>
    private int ComputeInstructionContentHash(CfgInstruction instruction) {
        HashCode hash = new();
        IList<byte?> signatureBytes = instruction.Signature.SignatureValue;
        
        foreach (byte? b in signatureBytes) {
            if (b.HasValue) {
                hash.Add(b.Value);
            }
        }
        
        return hash.ToHashCode();
    }

    /// <summary>
    /// Check if the node's current instruction matches the first instruction in the cached compiled block.
    /// This is critical for self-modifying code where the same CFG node ID is reused with different instruction content.
    /// </summary>
    private bool NodeInstructionsMatchCachedBlock(ICfgNode node, CompiledBlock block) {
        // The startNode used for compilation should be the first instruction in the block
        if (node is not CfgInstruction startInstruction) {
            if (_loggerService.IsEnabled(LogEventLevel.Verbose)) {
                _loggerService.Verbose("JIT: Node is not a CfgInstruction");
            }
            return false;
        }
        
        // Check if the start node is still the same instruction object (reference equality)
        // When CfgCpu replaces code, it creates new CfgInstruction objects
        bool same = ReferenceEquals(startInstruction, block.Instructions[0]);
        if (!same) {
            if (_loggerService.IsEnabled(LogEventLevel.Verbose)) {
                _loggerService.Verbose("JIT: Instruction reference mismatch for node {NodeId} at {Address}",  
                    node.Id, startInstruction.Address);
            }
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// Check if all instructions in the compiled block match their memory representation.
    /// Also verifies that instruction bytes haven't been modified in memory.
    /// </summary>
    private bool AllInstructionsAreStillLive(CompiledBlock block) {
        // First check the IsLive flag which is updated by CfgCpu
        if (!block.Instructions.All(instruction => instruction.IsLive)) {
            if (_loggerService.IsEnabled(LogEventLevel.Verbose)) {
                _loggerService.Verbose("JIT: Block has non-live instructions");
            }
            return false;
        }
        
        // Additionally verify that instruction bytes match memory
        // This catches cases where code was overwritten but CfgCpu hasn't re-parsed yet
        foreach (CfgInstruction instruction in block.Instructions) {
            if (!InstructionBytesMatchMemory(instruction)) {
                // Instruction bytes have changed in memory, mark as not live
                if (_loggerService.IsEnabled(LogEventLevel.Verbose)) {
                    _loggerService.Verbose("JIT: Instruction bytes don't match memory at {Address}", instruction.Address);
                }
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
        // Compare the signature bytes (from when instruction was parsed) with current memory
        IList<byte?> signatureBytes = instruction.Signature.SignatureValue;
        uint physicalAddress = MemoryUtils.ToPhysicalAddress(
            instruction.Address.Segment, instruction.Address.Offset);
        
        for (int i = 0; i < signatureBytes.Count; i++) {
            byte? expectedByte = signatureBytes[i];
            if (expectedByte is null) {
                // Wildcard byte in signature, skip
                continue;
            }
            
            byte actualByte = _memory.UInt8[physicalAddress + (uint)i];
            
            if (actualByte != expectedByte.Value) {
                if (_loggerService.IsEnabled(LogEventLevel.Verbose)) {
                    _loggerService.Verbose("JIT: Instruction at {Address} byte mismatch at offset {Offset}: expected {Expected:X2}, got {Actual:X2}", 
                        instruction.Address, i, expectedByte.Value, actualByte);
                }
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
    public bool TryCompileBasicBlock(ICfgNode startNode, InstructionExecutionHelper helper, out CompiledBlock? compiledBlock) {
        // Don't compile from SelectorNodes - they handle self-modifying code
        if (startNode is SelectorNode) {
            compiledBlock = null;
            return false;
        }
        
        if (startNode is not CfgInstruction startInstruction) {
            compiledBlock = null;
            return false;
        }
        
        int contentHash = ComputeInstructionContentHash(startInstruction);
        (int NodeId, int ContentHash) key = (startNode.Id, contentHash);
        
        if (_compiledBlocks.TryGetValue(key, out CompiledBlock? existingBlock)) {
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
            _compiledBlocks[key] = compiled;
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
        // Remove all cached blocks for this node ID (all content variations)
        List<(int NodeId, int ContentHash)> keysToRemove = _compiledBlocks.Keys.Where(k => k.NodeId == nodeId).ToList();
        foreach ((int NodeId, int ContentHash) key in keysToRemove) {
            _compiledBlocks.Remove(key);
        }
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
            
            // CRITICAL: Don't compile through SelectorNodes - they handle self-modifying code
            if (successor is SelectorNode) {
                break;
            }
            
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
