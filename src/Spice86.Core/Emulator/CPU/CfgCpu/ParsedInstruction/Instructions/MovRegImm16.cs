namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

using Spice86.Core.Emulator.CPU.CfgCpu.ControlFlowGraph;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;

public class MovRegImm16 : MovRegImm<ushort> {
    public MovRegImm16(uint physicalAddress, InstructionField<byte> opcodeField, IList<InstructionPrefix> prefixes,
        InstructionField<ushort> valueField, int regIndex) : base(physicalAddress, opcodeField, prefixes, valueField, regIndex) {
    }

    public override void Visit(ICfgNodeVisitor visitor) {
        throw new NotImplementedException();
    }
}