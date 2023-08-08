using NSubstitute;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.CPU.CfgCpu.ControlFlowGraph;
using Spice86.Core.Emulator.CPU.CfgCpu.Feeder;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.SelfModifying;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.VM;
using Spice86.Logging;
using Spice86.Shared.Interfaces;

using Xunit;

using ExecutionContext = Spice86.Core.Emulator.CPU.CfgCpu.Linker.ExecutionContext;

namespace Spice86.Tests.CfgCpu;

public class CfgNodeFeederTest {
    private const int AxIndex = 0;
    private const int BxIndex = 3;
    private const int CxIndex = 1;
    private const ushort DefaultValue = 0xFFFF;
    private const int MovRegImm16Length = 3;

    private Memory _memory = new(new Ram(64), is20ThAddressLineSilenced: false);
    private State _state = new State();
    private CfgNodeFeeder CreateCfgNodeFeeder() {
        ILoggerService loggerService = Substitute.For<LoggerService>(new LoggerPropertyBag());
        _memory = new(new Ram(64), is20ThAddressLineSilenced: false);
        _state = new State();
        MachineBreakpoints machineBreakpoints = new MachineBreakpoints(_memory, _state, loggerService);
        return new(_memory, _state, machineBreakpoints);
    }
    private void WriteMovReg16(int address, byte opcode, ushort value) {
        _memory.UInt8[address] = opcode;
        _memory.UInt16[address + 1] = value;
    }

    private void WriteMovAx(int address, ushort value) {
        WriteMovReg16(address, 0xB8, value);
    }

    private void WriteMovBx(int address, ushort value) {
        WriteMovReg16(address, 0xBB, value);
    }

    private void WriteMovCx(int address, ushort value) {
        WriteMovReg16(address, 0xB9, value);
    }

    [Fact]
    public void ReadInstructionViaParser() {
        // Arrange
        CfgNodeFeeder cfgNodeFeeder = CreateCfgNodeFeeder();
        WriteMovAx(0, DefaultValue);
        ExecutionContext executionContext = new ExecutionContext();

        // Act
        ICfgNode movAx = cfgNodeFeeder.GetLinkedCfgNodeToExecute(executionContext);

        // Assert
        Assert.Equal(typeof(MovRegImm16), movAx.GetType());
        MovRegImm16 movAxRegImm16 = (MovRegImm16)movAx;
        Assert.Equal(AxIndex, movAxRegImm16.RegIndex);
        Assert.True(movAxRegImm16.ValueField.UseValue);
        Assert.Equal(DefaultValue, movAxRegImm16.ValueField.Value);
    }

    [Fact]
    public void LinkTwoIntructions() {
        // Arrange
        CfgNodeFeeder cfgNodeFeeder = CreateCfgNodeFeeder();
        WriteMovAx(0, DefaultValue);
        WriteMovBx(MovRegImm16Length, DefaultValue);
        ExecutionContext executionContext = new ExecutionContext();
        ICfgNode movAx = cfgNodeFeeder.GetLinkedCfgNodeToExecute(executionContext);
        executionContext.LastExecuted = movAx;
        _state.IP = MovRegImm16Length;


        // Act
        ICfgNode movBx = cfgNodeFeeder.GetLinkedCfgNodeToExecute(executionContext);

        // Assert
        Assert.Equal(typeof(MovRegImm16), movBx.GetType());
        MovRegImm16 movBxRegImm16 = (MovRegImm16)movBx;
        Assert.Equal(BxIndex, movBxRegImm16.RegIndex);
        Assert.True(movBxRegImm16.ValueField.UseValue);
        Assert.Equal(DefaultValue, movBxRegImm16.ValueField.Value);
        Assert.True(movAx.Successors.Contains(movBx));
        Assert.True(movBx.Predecessors.Contains(movAx));
        MovRegImm16 movAxRegImm16 = (MovRegImm16)movAx;
        Assert.Equal(movBx, movAxRegImm16.SuccessorsPerAddress[MovRegImm16Length]);
    }

    [Fact]
    public void MovAxChangedToMovBx() {
        // Arrange
        CfgNodeFeeder cfgNodeFeeder = CreateCfgNodeFeeder();
        WriteMovAx(0, DefaultValue);
        WriteMovAx(MovRegImm16Length, DefaultValue);
        ExecutionContext executionContext = new ExecutionContext();
        ICfgNode movAx0 = cfgNodeFeeder.GetLinkedCfgNodeToExecute(executionContext);
        executionContext.LastExecuted = movAx0;
        _state.IP = MovRegImm16Length;
        ICfgNode movAx1 = cfgNodeFeeder.GetLinkedCfgNodeToExecute(executionContext);
        WriteMovBx(MovRegImm16Length, DefaultValue);
        executionContext.NodeToExecuteNextAccordingToGraph = movAx1;

        // Act
        // We are still after movAx0 but it changed to MOV BX.
        ICfgNode discriminated = cfgNodeFeeder.GetLinkedCfgNodeToExecute(executionContext);

        // Assert
        // Mov AX0 is not anymore linked to mov ax1, there is a discriminator node between them.
        Assert.False(movAx0.Successors.Contains(movAx1));
        Assert.False(movAx1.Predecessors.Contains(movAx0));
        // Check the discriminator node is there
        Assert.Equal(typeof(DiscriminatedNode), discriminated.GetType());
        DiscriminatedNode discriminatedNode = (DiscriminatedNode)discriminated;
        Assert.True(movAx0.Successors.Contains(discriminated));
        Assert.True(discriminated.Predecessors.Contains(movAx0));
        // Check discriminator node also contains Mov BX
        ICfgNode movBx = discriminated.Successors.First(node => node != movAx1);
        Assert.Equal(typeof(MovRegImm16), movBx.GetType());
        MovRegImm16 movBxRegImm16 = (MovRegImm16)movBx;
        MovRegImm16 movAx1RegImm16 = (MovRegImm16)movAx1;

        Assert.Equal(BxIndex, movBxRegImm16.RegIndex);
        Assert.True(movBxRegImm16.ValueField.UseValue);
        Assert.Equal(DefaultValue, movBxRegImm16.ValueField.Value);
        Assert.True(movBx.Predecessors.Contains(discriminated));
        Assert.Equal(movBxRegImm16, discriminatedNode.SuccessorsPerDiscriminator[movBxRegImm16.Discriminator]);
        Assert.Equal(movAx1RegImm16, discriminatedNode.SuccessorsPerDiscriminator[movAx1RegImm16.Discriminator]);
    }
    
    [Fact]
    public void MovAxChangedValue() {
        // Arrange
        CfgNodeFeeder cfgNodeFeeder = CreateCfgNodeFeeder();
        WriteMovAx(0, DefaultValue);
        WriteMovAx(MovRegImm16Length, DefaultValue);
        ExecutionContext executionContext = new ExecutionContext();
        ICfgNode movAx0 = cfgNodeFeeder.GetLinkedCfgNodeToExecute(executionContext);
        executionContext.LastExecuted = movAx0;
        _state.IP = MovRegImm16Length;
        ICfgNode movAx1 = cfgNodeFeeder.GetLinkedCfgNodeToExecute(executionContext);
        WriteMovAx(MovRegImm16Length, 0x1234);
        executionContext.NodeToExecuteNextAccordingToGraph = movAx1;

        // Act
        // We are still after movAx0 but it changed to MOV AX 1234.
        ICfgNode shouldBeMovAx1 = cfgNodeFeeder.GetLinkedCfgNodeToExecute(executionContext);

        // Assert
        Assert.Equal(movAx1, shouldBeMovAx1);
    }

    [Fact]
    public void MovAxChangedToMovBxThenMovCx() {
        // Arrange
        CfgNodeFeeder cfgNodeFeeder = CreateCfgNodeFeeder();
        WriteMovAx(0, DefaultValue);
        WriteMovAx(MovRegImm16Length, DefaultValue);
        ExecutionContext executionContext = new ExecutionContext();
        ICfgNode movAx0 = cfgNodeFeeder.GetLinkedCfgNodeToExecute(executionContext);
        executionContext.LastExecuted = movAx0;
        _state.IP = MovRegImm16Length;
        ICfgNode movAx1 = cfgNodeFeeder.GetLinkedCfgNodeToExecute(executionContext);
        WriteMovBx(MovRegImm16Length, DefaultValue);
        executionContext.NodeToExecuteNextAccordingToGraph = movAx1;
        ICfgNode discriminator = cfgNodeFeeder.GetLinkedCfgNodeToExecute(executionContext);
        WriteMovCx(MovRegImm16Length, DefaultValue);
        // CPU executed discriminator but Mov CX was in memory => no successor of the discriminator matched
        executionContext.LastExecuted = discriminator;
        executionContext.NodeToExecuteNextAccordingToGraph = null;

        // Act
        // We at discriminator but instruction got changed to something that is not yet in the discriminator list of values.
        ICfgNode movCx = cfgNodeFeeder.GetLinkedCfgNodeToExecute(executionContext);

        // Assert
        Assert.Equal(typeof(MovRegImm16), movCx.GetType());
        Assert.Equal(CxIndex, ((MovRegImm16)movCx).RegIndex);
        Assert.True(discriminator.Successors.Contains(movCx));
        Assert.True(movCx.Predecessors.Contains(discriminator));
    }
    
    // todo: 
    // test value change in node after discriminator?
    // test add another value change after first value change encounter
}