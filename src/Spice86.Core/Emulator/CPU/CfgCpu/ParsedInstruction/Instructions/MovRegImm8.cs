namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

using Spice86.Core.Emulator.CPU.CfgCpu.ControlFlowGraph;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;

public class MovRegImm8 : MovRegImm<byte> {
    public MovRegImm8(uint physicalAddress, InstructionField<byte> opcodeField, IList<InstructionPrefix> prefixes,
        InstructionField<byte> valueField, int regIndex) : base(physicalAddress, opcodeField, prefixes, valueField, regIndex) {
    }

    public override void Visit(ICfgNodeVisitor visitor) {
        throw new NotImplementedException();
    }
}