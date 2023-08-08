namespace Spice86.Core.Emulator.CPU.CfgCpu.InstructionExecutor;

using Spice86.Core.Emulator.CPU.CfgCpu.ControlFlowGraph;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.SelfModifying;
using Spice86.Core.Emulator.InterruptHandlers.Common.Callback;
using Spice86.Core.Emulator.IOPorts;
using Spice86.Core.Emulator.Memory;

public class ExecutorCfgNodeVisitor : ICfgNodeVisitor {
    private readonly State _state;
    private readonly IMemory _memory;
    private readonly IOPortDispatcher? _ioPortDispatcher;
    private readonly CallbackHandler _callbackHandler;
    private readonly InstructionFieldValueRetriever _instructionFieldValueRetriever;

    public ExecutorCfgNodeVisitor(State state, IMemory memory, IOPortDispatcher? ioPortDispatcher,
        CallbackHandler callbackHandler) {
        _state = state;
        _memory = memory;
        _ioPortDispatcher = ioPortDispatcher;
        _callbackHandler = callbackHandler;
        _instructionFieldValueRetriever = new(_memory);
    }
    
    public ICfgNode? NextNode { get; private set; }

    public void Accept(HltInstruction instruction) {
        _state.IsRunning = false;
        NextNode = null;
    }

    public void Accept(JmpNearImm<sbyte> instruction) {
        sbyte offset = _instructionFieldValueRetriever.GetFieldValue(instruction.OffsetField);
        JumpNear(instruction, offset);
        SetNextNodeToSuccessorAtCsIp(instruction);
    }

    public void Accept(DiscriminatedNode discriminatedNode) {
        int address = (int)discriminatedNode.PhysicalAddress;
        foreach (InstructionDiscriminator instructionDiscriminator in discriminatedNode.SuccessorsPerDiscriminator.Keys) {
            int length = instructionDiscriminator.DiscriminatorValue.Count;
            Span<byte> bytes = _memory.GetSpan(address, length);
            if (instructionDiscriminator.Equals(bytes)) {
                NextNode = discriminatedNode.SuccessorsPerDiscriminator[instructionDiscriminator];
                return;
            }
        }

        NextNode = null;
    }

    private void JumpNear(CfgInstruction instruction, int offset) {
        MoveIpToEndOfInstruction(instruction);
        _state.IP = (ushort)(_state.IP + offset);
    }

    private void MoveIpToEndOfInstruction(CfgInstruction instruction) {
        _state.IP = (ushort)(_state.IP + instruction.Length) ;
    }

    private ICfgNode? GetSuccessorAtCsIp(CfgInstruction cfgNode) {
        cfgNode.SuccessorsPerAddress.TryGetValue(_state.IpPhysicalAddress, out ICfgNode? res);
        return res;
    }

    private void SetNextNodeToSuccessorAtCsIp(CfgInstruction cfgNode) {
        NextNode = GetSuccessorAtCsIp(cfgNode);
    }

}