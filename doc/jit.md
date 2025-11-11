# JIT Compilation in Spice86

## Overview

The JIT (Just-In-Time) compiler in Spice86 improves x86 emulation performance by compiling basic blocks of instructions into cached delegates. This reduces the overhead of per-instruction CFG (Control Flow Graph) traversal and node linking in the CfgCpu interpreter.

## Architecture

### Components

#### IJitCompiler Interface
Located in `Spice86.Core/Emulator/CPU/CfgCpu/Jit/IJitCompiler.cs`

Defines the contract for JIT compilation:
- `TryGetCompiledBlock`: Retrieves a cached compiled block if available and valid
- `TryCompileBasicBlock`: Attempts to compile a basic block starting from a given node
- `InvalidateBlock`: Invalidates a compiled block (used for self-modifying code)

#### JitCompiler
Located in `Spice86.Core/Emulator/CPU/CfgCpu/Jit/JitCompiler.cs`

The active JIT compiler implementation that:
1. **Identifies Basic Blocks**: Collects sequences of instructions with linear control flow
2. **Caches Compiled Blocks**: Stores compiled blocks indexed by node ID
3. **Validates Blocks**: Checks IsLive property and execution breakpoints before use
4. **Handles Self-Modifying Code**: Automatically invalidates blocks when code changes

#### NullJitCompiler
Located in `Spice86.Core/Emulator/CPU/CfgCpu/Jit/NullJitCompiler.cs`

A null-object implementation that does nothing. Used when JIT is disabled.

#### CompiledBlock
Located in `Spice86.Core/Emulator/CPU/CfgCpu/Jit/CompiledBlock.cs`

A sealed record containing:
- `Instructions`: ReadOnly list of CfgInstruction objects in the block
- `CompiledMethod`: Delegate that executes all instructions sequentially
- `LastInstruction`: Reference to the final instruction for context updates

### Basic Block Collection

A basic block is defined as a sequence of instructions satisfying:
1. **Linear Control Flow**: Single successor (no branches except at end)
2. **No Merge Points**: Successor has single predecessor
3. **Size Constraints**: 
   - Minimum: 3 instructions (overhead threshold)
   - Maximum: 50 instructions (compilation time threshold)

The algorithm:
```csharp
while (current is CfgInstruction instruction) {
    instructions.Add(instruction);
    
    if (HasMultipleSuccessors(instruction)) break;
    
    ICfgNode successor = instruction.UniqueSuccessor ?? instruction.Successors.First();
    if (IsJoinPoint(successor)) break;
    
    if (instructions.Count >= MaximumBlockSize) break;
    
    current = successor;
}
```

### Self-Modifying Code Detection

The JIT integrates with CfgCpu's existing self-modifying code detection:

1. **IsLive Property**: Each CfgInstruction maintains an IsLive property indicating whether the instruction in the graph matches what's currently in memory.

2. **Pre-Execution Validation**: Before returning a cached block, `TryGetCompiledBlock` verifies:
   ```csharp
   return block.Instructions.All(instruction => instruction.IsLive);
   ```

3. **Automatic Invalidation**: If any instruction is not live, the block is removed from cache and will be recompiled on next access.

### Breakpoint Handling

The JIT only checks **execution breakpoints** because:

- **Memory Breakpoints**: Triggered during memory operations, not instruction fetch
- **IO Breakpoints**: Triggered during port I/O operations  
- **Cycle Breakpoints**: Triggered based on cycle count, not code location

Before using a cached block, `BlockContainsExecutionBreakpoint` checks:
```csharp
foreach (CfgInstruction instruction in block.Instructions) {
    uint physicalAddress = MemoryUtils.ToPhysicalAddress(
        instruction.Address.Segment, instruction.Address.Offset);
    
    if (_breakpointsManager.HasExecutionBreakpoint(physicalAddress)) {
        return true;
    }
}
```

If any execution breakpoint is present, the block is not used and interpretation proceeds normally to ensure the breakpoint triggers correctly.

### Integration with CfgCpu

The JIT is integrated into `CfgCpu.ExecuteNext()`:

```csharp
public void ExecuteNext() {
    ICfgNode toExecute = CfgNodeFeeder.GetLinkedCfgNodeToExecute(CurrentExecutionContext);
    
    if (TryExecuteWithJit(toExecute)) {
        return;
    }
    
    ExecuteNodeInterpreted(toExecute);
}
```

The `TryExecuteWithJit` method:
1. Checks if node is a CfgInstruction
2. Attempts to retrieve cached compiled block
3. If not cached, attempts to compile the block
4. Executes the compiled block if available
5. Returns false to fall back to interpretation if JIT unavailable

### Compilation Strategy

The current implementation uses **basic block batching**:

```csharp
Action<InstructionExecutionHelper> compiledMethod = helperParam => {
    foreach (CfgInstruction instruction in instructions) {
        instruction.Execute(helperParam);
    }
};
```

This approach:
- **Pros**: Simple, maintains all existing instruction logic, compatible with all features
- **Cons**: Still interprets individual instructions, doesn't generate native code

**Future Enhancement**: The architecture supports replacing this with true IL generation using System.Reflection.Emit for even better performance.

## Configuration

### Enabling JIT

The JIT requires CfgCpu to be enabled:

```bash
Spice86 -e program.exe --CfgCpu --JitCpu
```

Command-line flags:
- `--CfgCpu`: Enables the Control Flow Graph CPU (required)
- `--JitCpu`: Enables JIT compilation on top of CfgCpu

### Dependency Injection

In `Spice86DependencyInjection.cs`:

```csharp
if (enableJit) {
    _jitCompiler = new JitCompiler(loggerService, emulatorBreakpointsManager);
} else {
    _jitCompiler = new NullJitCompiler();
}
```

The field is always non-null (using Null Object pattern), eliminating null checks.

## Performance Characteristics

### Overhead Analysis

**Without JIT** (per instruction):
- CFG node lookup
- Predecessor/successor linking
- Context management
- Instruction dispatch

**With JIT** (per block):
- Single CFG node lookup (for first instruction)
- Block validation (IsLive check + breakpoint check)
- Batch execution of all instructions
- Single context update (for last instruction)

### Expected Speedup

- **Best Case**: Tight loops with no self-modification → significant speedup (10-50x)
- **Typical Case**: Mixed code with occasional branches → moderate speedup (2-5x)
- **Worst Case**: Heavy self-modification or breakpoints → minimal/no speedup

### Invalidation Costs

When self-modifying code is detected:
1. Block removed from cache (O(1))
2. Next execution recompiles block (O(n) where n = block size)
3. Subsequent executions use new compiled version

## Compatibility

### C# Overrides

C# code overrides work unchanged because they're checked during instruction execution:
- Override hooks are called within `instruction.Execute()`
- JIT batches `Execute()` calls but doesn't bypass them
- Overrides can modify control flow, which JIT respects

### Debugging Support

- **Execution Breakpoints**: JIT checks and avoids compiling blocks with breakpoints
- **Memory/IO Breakpoints**: Work normally (checked during memory/IO operations)
- **Single-Stepping**: Works correctly (JIT disabled when debugger active)
- **GDB Protocol**: Fully supported

### Known Limitations

1. **No Native Code Generation**: Current implementation batches interpretation, doesn't generate IL/native code
2. **Fixed Block Size Limits**: Arbitrary thresholds may not be optimal for all programs
3. **Conservative Invalidation**: Entire block invalidated even if only one instruction modified

## Testing

### Test Coverage

Located in `tests/Spice86.Tests/JitTests.cs`:

1. **TestJitBasicExecution**: Verifies basic JIT execution works
2. **TestJitVsInterpreted**: Ensures JIT produces same results as interpretation
3. **TestJitWithCSharpOverrides**: Validates C# override compatibility
4. **TestJitWithBreakpoints**: Confirms breakpoint infrastructure works
5. **TestJitWithSelfModifyingCode**: Tests direct code modification detection
6. **TestJitWithStackReturnAddressModification**: Tests stack-based self-modification
7. **TestJitWithSelfModifyingLoopDestination**: Tests dynamic jump target modification

### Self-Modifying Code Patterns Tested

- **Direct Modification**: Code that changes its own instructions
- **Stack-Based**: Code that modifies return addresses on stack
- **Loop Modification**: Code that changes jump targets to alter loop behavior

## Future Enhancements

### IL Generation

Replace delegate batching with System.Reflection.Emit:

```csharp
ILGenerator il = dynamicMethod.GetILGenerator();
foreach (var instruction in block.Instructions) {
    EmitInstructionIL(il, instruction);
}
```

Benefits:
- True native code generation
- Better register allocation
- Inlining opportunities
- Significant performance improvement

Challenges:
- Complex instruction semantics
- State management
- Exception handling
- Debugging support

### Adaptive Compilation

- **Hotspot Detection**: Compile frequently executed blocks first
- **Tiered Compilation**: Start with interpretation, compile hot blocks, optimize hot blocks
- **Profiling-Guided**: Use runtime profiling to guide optimization decisions

### Advanced Optimizations

- **Dead Code Elimination**: Remove unreachable instructions
- **Constant Folding**: Evaluate constant expressions at compile time
- **Register Allocation**: Reduce memory traffic for temporary values
- **Loop Unrolling**: Expand small loops for better performance

## Troubleshooting

### JIT Not Activating

Check:
1. Is `--CfgCpu` enabled?
2. Is `--JitCpu` enabled?
3. Are blocks large enough? (Minimum 3 instructions)
4. Are there execution breakpoints in the code?

### Performance Degradation

Possible causes:
1. Heavy self-modifying code (frequent recompilation)
2. Many small blocks (overhead exceeds benefit)
3. Excessive breakpoints (prevents compilation)

### Incorrect Behavior

Debug steps:
1. Disable JIT and verify behavior with interpretation
2. Check for self-modifying code patterns
3. Verify breakpoints are correctly placed
4. Review execution logs for invalidation patterns

## Technical Details

### Memory Management

- Compiled blocks are stored in `Dictionary<int, CompiledBlock>`
- Blocks are never explicitly freed (rely on GC)
- Cache has no size limit (unbounded growth)
- Invalidation removes entries but doesn't trigger GC

### Thread Safety

Current implementation is **not thread-safe**:
- Single-threaded execution assumed
- No locks on cache access
- No atomic operations on shared state

### Instruction Compatibility

All instructions are compatible with JIT because:
- No instruction-specific compilation logic
- Delegates call existing `Execute()` methods
- All instruction semantics preserved

## References

- CfgCpu Architecture: `doc/cfgcpuReadme.md`
- Code Override System: Repository custom instructions
- Breakpoint System: `src/Spice86.Core/Emulator/VM/Breakpoint/`
- Testing Framework: `tests/Spice86.Tests/JitTests.cs`
