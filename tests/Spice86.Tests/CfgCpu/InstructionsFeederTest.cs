using NSubstitute;
using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.CPU.CfgCpu.Feeder;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.VM;
using Spice86.Logging;
using Spice86.Shared.Interfaces;
using Xunit;

namespace Spice86.Tests.CfgCpu;

public class InstructionsFeederTest
{
    private readonly Memory _memory = new(new Ram(64), is20ThAddressLineSilenced: false);

    private InstructionsFeeder CreateInstructionsFeeder()
    {
        _memory.Memset8(0, 0, 64);
        ILoggerService loggerService = Substitute.For<LoggerService>(new LoggerPropertyBag());
        State state = new();
        MachineBreakpoints machineBreakpoints = new MachineBreakpoints(_memory, state, loggerService);
        return new InstructionsFeeder(machineBreakpoints, _memory, state);
    }

    private void WriteJumpNear(int address)
    {
        _memory.UInt8[address] = 0xEB;
        _memory.Int8[address + 1] = -2;
    }

    private void WriteMovAx(int address)
    {
        _memory.UInt8[address] = 0xB8;
        _memory.UInt16[address + 1] = 0xFFFF;
    }

    private JmpNearImm8 CreateReplacementInstruction()
    {
        var opcodeField = new InstructionField<byte>(0, 1, 0, 0xEB, new List<byte?>() { 0xEB });
        // Replacement has a null discriminator byte for offset field -> will not be taken into account when comparing with ram
        var offsetField = new InstructionField<sbyte>(1, 1, 1, -2, new List<byte?>() { null });
        JmpNearImm8 res = new JmpNearImm8(0, opcodeField, offsetField);
        res.PostInit();
        return res;
    }

    [Fact]
    public void ReadInstructionViaParser()
    {
        // Arrange
        InstructionsFeeder instructionsFeeder = CreateInstructionsFeeder();
        WriteJumpNear(0);

        // Act
        CfgInstruction instruction = instructionsFeeder.GetInstructionFromMemory(0, 0);

        // Assert
        Assert.Equal(typeof(JmpNearImm8), instruction.GetType());
    }

    [Fact]
    public void ReadInstructionTwiceIsSameInstruction()
    {
        // Arrange
        InstructionsFeeder instructionsFeeder = CreateInstructionsFeeder();
        WriteJumpNear(0);

        // Act
        CfgInstruction instruction1 = instructionsFeeder.GetInstructionFromMemory(0, 0);
        CfgInstruction instruction2 = instructionsFeeder.GetInstructionFromMemory(0, 0);


        // Assert
        Assert.Equal(instruction1, instruction2);
    }

    [Fact]
    public void ReadSeflModifyingCode()
    {
        // Arrange
        InstructionsFeeder instructionsFeeder = CreateInstructionsFeeder();
        WriteJumpNear(0);

        // Act
        // Read the jump
        instructionsFeeder.GetInstructionFromMemory(0, 0);
        // This should cause the breakpoints to trigger and to clear the first instruction from the first current cache ensuring address 0 will be parsed
        WriteMovAx(0);
        CfgInstruction instruction2 = instructionsFeeder.GetInstructionFromMemory(0, 0);


        // Assert
        Assert.Equal(typeof(MovRegImm16), instruction2.GetType());
    }

    [Fact]
    public void ReadSeflModifyingCodeSameInstructionTwice()
    {
        // Arrange
        InstructionsFeeder instructionsFeeder = CreateInstructionsFeeder();
        WriteJumpNear(0);

        // Act
        // Read the jump
        CfgInstruction instruction1 = instructionsFeeder.GetInstructionFromMemory(0, 0);
        WriteMovAx(0);
        // Read the mov
        instructionsFeeder.GetInstructionFromMemory(0, 0);
        WriteJumpNear(0);
        // Read the jump again
        CfgInstruction instruction2 = instructionsFeeder.GetInstructionFromMemory(0, 0);

        // Assert
        Assert.Equal(instruction1, instruction2);
    }

    [Fact]
    public void ReadSeflModifyingCodeSameInstructionTwiceThenDetectSwitchAgain()
    {
        // Arrange
        InstructionsFeeder instructionsFeeder = CreateInstructionsFeeder();
        WriteJumpNear(0);

        // Act
        // Read the jump
        instructionsFeeder.GetInstructionFromMemory(0, 0);
        WriteMovAx(0);
        // Read the mov
        instructionsFeeder.GetInstructionFromMemory(0, 0);
        WriteJumpNear(0);
        // Read the jump again
        instructionsFeeder.GetInstructionFromMemory(0, 0);
        WriteMovAx(0);
        // Read the mov, normally last restore that is from "previous" cache should also have set breakpoints and so on for self modifying code detection
        CfgInstruction instruction2 = instructionsFeeder.GetInstructionFromMemory(0, 0);

        // Assert
        Assert.Equal(typeof(MovRegImm16), instruction2.GetType());
    }

    [Fact]
    public void ReplaceInstruction()
    {
        // Arrange
        InstructionsFeeder instructionsFeeder = CreateInstructionsFeeder();
        WriteJumpNear(0);
        CfgInstruction old = instructionsFeeder.GetInstructionFromMemory(0, 0);

        // Act
        CfgInstruction newInstruction = CreateReplacementInstruction();
        instructionsFeeder.ReplaceInstruction(old, newInstruction);
        CfgInstruction instruction = instructionsFeeder.GetInstructionFromMemory(0, 0);

        // Assert
        Assert.Equal(newInstruction, instruction);
    }
    
    [Fact]
    public void ReplaceInstructionAndEnsureSelfModifyingCodeIsDetected()
    {
        // Arrange
        InstructionsFeeder instructionsFeeder = CreateInstructionsFeeder();
        WriteJumpNear(0);
        CfgInstruction old = instructionsFeeder.GetInstructionFromMemory(0, 0);

        // Act
        CfgInstruction newInstruction = CreateReplacementInstruction();
        instructionsFeeder.ReplaceInstruction(old, newInstruction);
        WriteMovAx(0);
        CfgInstruction instruction = instructionsFeeder.GetInstructionFromMemory(0, 0);
        
        // Assert
        Assert.Equal(typeof(MovRegImm16), instruction.GetType());
    }
    
    [Fact]
    public void ReplaceInstructionAndEnsureItStaysAfterSelfModifyingCode()
    {
        // Arrange
        InstructionsFeeder instructionsFeeder = CreateInstructionsFeeder();
        WriteJumpNear(0);
        CfgInstruction old = instructionsFeeder.GetInstructionFromMemory(0, 0);

        // Act
        CfgInstruction newInstruction = CreateReplacementInstruction();
        instructionsFeeder.ReplaceInstruction(old, newInstruction);
        WriteMovAx(0);
        instructionsFeeder.GetInstructionFromMemory(0, 0);
        WriteJumpNear(0);
        CfgInstruction instruction = instructionsFeeder.GetInstructionFromMemory(0, 0);

        // Assert
        Assert.Equal(newInstruction, instruction);
    }
    
    [Fact]
    public void SameInstructionAsDifferentAddressIsDifferentNode()
    {
        // Arrange
        InstructionsFeeder instructionsFeeder = CreateInstructionsFeeder();
        WriteJumpNear(0);
        WriteJumpNear(2);

        // Act
        CfgInstruction instructionAddress0 = instructionsFeeder.GetInstructionFromMemory(0, 0);      
        CfgInstruction instructionAddress2 = instructionsFeeder.GetInstructionFromMemory(0, 2);


        // Assert
        Assert.Equal(typeof(JmpNearImm8), instructionAddress2.GetType());
        Assert.NotEqual(instructionAddress0, instructionAddress2);
    }
    
    [Fact]
    public void SameInstructionSamePhysicalAddressDifferentSegmentedAddressIsSame()
    {
        // Arrange
        InstructionsFeeder instructionsFeeder = CreateInstructionsFeeder();
        WriteJumpNear(16);

        // Act
        CfgInstruction instruction1 = instructionsFeeder.GetInstructionFromMemory(0, 16);
        CfgInstruction instruction2 = instructionsFeeder.GetInstructionFromMemory(1, 0);


        // Assert
        Assert.Equal(instruction1, instruction2);
    }
}