namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.SelfModifying;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Builder;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Instruction;
using Spice86.Core.Emulator.CPU.CfgCpu.ControlFlowGraph;
using Spice86.Core.Emulator.CPU.CfgCpu.InstructionExecutor;
using Spice86.Shared.Emulator.Memory;

using System.Linq;

/// <summary>
/// Node that precedes self modifying code divergence point.
/// To decide what is next node in the graph, the only way is to compare signatures in SuccessorsPerSignature with actual memory content. 
/// </summary>
public class SelectorNode(SegmentedAddress address) : CfgNode(address, null) {
    /// <summary>
    /// The is live.
    /// </summary>
    public override bool IsLive => true;

    public Dictionary<Signature, CfgInstruction> SuccessorsPerSignature { get; private set; } =
        new();

    /// <summary>
    /// Updates successor cache.
    /// </summary>
    public override void UpdateSuccessorCache() {
        SuccessorsPerSignature = Successors.OfType<CfgInstruction>()
            .OrderBy(node => node.Signature)
            .ToDictionary(node => node.Signature);
    }

    /// <summary>
    /// Executes .
    /// </summary>
    /// <param name="helper">The helper.</param>
    public override void Execute(InstructionExecutionHelper helper) {
        foreach (Signature signature in SuccessorsPerSignature.Keys) {
            int length = signature.SignatureValue.Count;
            IList<byte> bytes = helper.Memory.GetSlice((int)Address.Linear, length);
            if (signature.ListEquivalent(bytes)) {
                helper.NextNode = SuccessorsPerSignature[signature];
                return;
            }
        }

        helper.NextNode = null;
    }

    /// <summary>
    /// Converts to instruction ast.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The result of the operation.</returns>
    public override InstructionNode ToInstructionAst(AstBuilder builder) {
        return new InstructionNode(InstructionOperation.SELECTOR);
    }
}