namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

using Spice86.Core.Emulator.CPU.CfgCpu.ControlFlowGraph;

public class JmpNearImm8 : JmpNearImm<sbyte> {
    public override void Visit(ICfgNodeVisitor visitor) {
        visitor.Accept(this);
    }

    public JmpNearImm8(uint physicalAddress, InstructionField<byte> opcodeField, InstructionField<sbyte> offsetField) :
        base(physicalAddress, opcodeField, offsetField) {
    }
}