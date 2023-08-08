namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.SelfModifying;

using Spice86.Core.Emulator.CPU.CfgCpu.ControlFlowGraph;

using System.Linq;

public class DiscriminatedNode : CfgNode {
    public DiscriminatedNode(uint physicalAddress) : base(physicalAddress) {
    }

    public override bool IsAssembly => false;

    public IDictionary<InstructionDiscriminator, CfgInstruction> SuccessorsPerDiscriminator { get; private set; } =
        new Dictionary<InstructionDiscriminator, CfgInstruction>();

    public override void UpdateSuccessorCache() {
        SuccessorsPerDiscriminator = Successors.OfType<CfgInstruction>().ToDictionary(node => node.Discriminator);
    }

    public override void Visit(ICfgNodeVisitor visitor) {
        visitor.Accept(this);
    }
}