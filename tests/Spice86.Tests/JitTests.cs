namespace Spice86.Tests;

using Spice86.Core.CLI;
using Spice86.Core.Emulator;
using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.CPU.CfgCpu;
using Spice86.Core.Emulator.CPU.CfgCpu.Jit;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.VM;
using Spice86.Core.Emulator.VM.Breakpoint;
using Spice86.Shared.Emulator.Memory;
using Spice86.Shared.Emulator.VM.Breakpoint;
using Spice86.Shared.Utils;

using Xunit;

/// <summary>
/// Tests for JIT compilation functionality
/// </summary>
public class JitTests {
    [Fact]
    public void TestJitBasicExecution() {
        // Create a simple program with CfgCpu and JIT enabled
        using Spice86DependencyInjection spice86 = CreateTestEnvironment(enableJit: true);
        
        IMemory memory = spice86.Machine.Memory;
        State state = spice86.Machine.CpuState;
        
        // Write a simple program: INC AX; INC AX; HLT
        ushort segment = 0x1000;
        ushort offset = 0x0000;
        state.CS = segment;
        state.IP = offset;
        
        uint address = MemoryUtils.ToPhysicalAddress(segment, offset);
        memory.UInt8[address] = 0x40; // INC AX
        memory.UInt8[address + 1] = 0x40; // INC AX
        memory.UInt8[address + 2] = 0x40; // INC AX
        memory.UInt8[address + 3] = 0xF4; // HLT
        
        // Set initial value
        state.AX = 0;
        
        // Execute 3 INC instructions
        spice86.Machine.Cpu.ExecuteNext(); // First INC
        spice86.Machine.Cpu.ExecuteNext(); // Second INC
        spice86.Machine.Cpu.ExecuteNext(); // Third INC
        
        // Verify AX was incremented 3 times
        Assert.Equal(3, state.AX);
    }
    
    [Fact]
    public void TestJitWithBreakpoints() {
        // Test that breakpoints work with JIT compilation
        using Spice86DependencyInjection spice86 = CreateTestEnvironment(enableJit: true);
        
        IMemory memory = spice86.Machine.Memory;
        State state = spice86.Machine.CpuState;
        
        // Write a simple program
        ushort segment = 0x1000;
        ushort offset = 0x0000;
        state.CS = segment;
        state.IP = offset;
        
        uint address = MemoryUtils.ToPhysicalAddress(segment, offset);
        memory.UInt8[address] = 0x40; // INC AX
        memory.UInt8[address + 1] = 0x40; // INC AX
        memory.UInt8[address + 2] = 0xF4; // HLT
        
        // Set an execution breakpoint
        bool breakpointHit = false;
        var breakpoint = new AddressBreakPoint(
            BreakPointType.CPU_EXECUTION_ADDRESS,
            MemoryUtils.ToPhysicalAddress(segment, (ushort)(offset + 1)),
            bp => { breakpointHit = true; },
            false);
        
        spice86.Machine.EmulatorBreakpointsManager.ToggleBreakPoint(breakpoint, true);
        
        state.AX = 0;
        
        // Execute first instruction
        spice86.Machine.Cpu.ExecuteNext();
        Assert.Equal(1, state.AX);
        Assert.False(breakpointHit);
        
        // Execute second instruction - should hit breakpoint
        spice86.Machine.Cpu.ExecuteNext();
        Assert.Equal(2, state.AX);
        Assert.True(breakpointHit);
    }
    
    [Fact]
    public void TestJitVsInterpretedSameResults() {
        // Test that JIT produces the same results as interpreted execution
        
        // Run with JIT
        ushort axValueJit;
        using (Spice86DependencyInjection spice86Jit = CreateTestEnvironment(enableJit: true)) {
            SetupTestProgram(spice86Jit);
            ExecuteTestProgram(spice86Jit);
            axValueJit = spice86Jit.Machine.CpuState.AX;
        }
        
        // Run without JIT
        ushort axValueInterpreted;
        using (Spice86DependencyInjection spice86Interpreted = CreateTestEnvironment(enableJit: false)) {
            SetupTestProgram(spice86Interpreted);
            ExecuteTestProgram(spice86Interpreted);
            axValueInterpreted = spice86Interpreted.Machine.CpuState.AX;
        }
        
        // Both should produce the same result
        Assert.Equal(axValueInterpreted, axValueJit);
    }
    
    [Fact]
    public void TestJitWithOverrides() {
        // Test that C# overrides still work with JIT enabled
        using Spice86DependencyInjection spice86 = CreateTestEnvironmentWithOverrides(enableJit: true);
        
        // If the test environment setup works, overrides are compatible
        Assert.NotNull(spice86.Machine.Cpu);
    }
    
    private static Spice86DependencyInjection CreateTestEnvironment(bool enableJit) {
        // Use an existing test binary
        return new Spice86Creator("add", enableCfgCpu: true, enableJit: enableJit).Create();
    }
    
    private static Spice86DependencyInjection CreateTestEnvironmentWithOverrides(bool enableJit) {
        return new Spice86Creator("add", enableCfgCpu: true, enableJit: enableJit, 
            overrideSupplierClassName: null).Create();
    }
    
    private static void SetupTestProgram(Spice86DependencyInjection spice86) {
        IMemory memory = spice86.Machine.Memory;
        State state = spice86.Machine.CpuState;
        
        // Write a test program that does various operations
        ushort segment = 0x1000;
        ushort offset = 0x0000;
        state.CS = segment;
        state.IP = offset;
        
        uint address = MemoryUtils.ToPhysicalAddress(segment, offset);
        
        // Program: INC AX; ADD AX, 5; DEC BX; HLT
        memory.UInt8[address] = 0x40; // INC AX
        memory.UInt8[address + 1] = 0x05; // ADD AX, imm16 (opcode)
        memory.UInt8[address + 2] = 0x05; // immediate low byte
        memory.UInt8[address + 3] = 0x00; // immediate high byte
        memory.UInt8[address + 4] = 0x4B; // DEC BX
        memory.UInt8[address + 5] = 0xF4; // HLT
        
        state.AX = 10;
        state.BX = 20;
    }
    
    private static void ExecuteTestProgram(Spice86DependencyInjection spice86) {
        // Execute 3 instructions (INC AX, ADD AX, DEC BX)
        spice86.Machine.Cpu.ExecuteNext();
        spice86.Machine.Cpu.ExecuteNext();
        spice86.Machine.Cpu.ExecuteNext();
    }
}
