namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

public class HltInstruction : CfgInstruction {
    public HltInstruction(uint physicalAddress, InstructionField<byte> opcodeField) :
        base(physicalAddress, opcodeField) {
    }

    public override void Visit(ICfgNodeVisitor visitor) {
        visitor.Accept(this);
    }
}