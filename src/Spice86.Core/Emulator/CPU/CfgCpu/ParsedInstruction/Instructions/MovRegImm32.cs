namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

using Spice86.Core.Emulator.CPU.CfgCpu.ControlFlowGraph;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;

public class MovRegImm32 : MovRegImm<uint> {
    public MovRegImm32(uint physicalAddress, InstructionField<byte> opcodeField, IList<InstructionPrefix> prefixes,
        InstructionField<uint> valueField, int regIndex) : base(physicalAddress, opcodeField, prefixes, valueField, regIndex) {
    }

    public override void Visit(ICfgNodeVisitor visitor) {
        throw new NotImplementedException();
    }
}