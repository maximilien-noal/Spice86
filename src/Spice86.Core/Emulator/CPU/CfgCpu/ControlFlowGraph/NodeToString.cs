namespace Spice86.Core.Emulator.CPU.CfgCpu.ControlFlowGraph;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Builder;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Instruction;
using Spice86.Core.Emulator.CPU.CfgCpu.InstructionRenderer;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.SelfModifying;

using System.Linq;

/// <summary>
/// For logging and debugging purposes
/// </summary>
public class NodeToString {
    private readonly AstBuilder _astBuilder = new();
    private readonly AstInstructionRenderer _renderer = new();

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>The result of the operation.</returns>
    public string ToString(ICfgNode node) {
        return $"{ToHeaderString(node)} / {ToAssemblyString(node)}";
    }

    /// <summary>
    /// Converts to header string.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>The result of the operation.</returns>
    public string ToHeaderString(ICfgNode node) {
        return $"{node.Address} / {node.Id}";
    }

    /// <summary>
    /// Converts to assembly string.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>The result of the operation.</returns>
    public string ToAssemblyString(ICfgNode node) {
        InstructionNode ast = node.ToInstructionAst(_astBuilder);
        return ast.Accept(_renderer);
    }

    /// <summary>
    /// Performs the successors to string operation.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>The result of the operation.</returns>
    public string SuccessorsToString(ICfgNode node) {
        return string.Join($"{Environment.NewLine}", SuccessorsToEnumerableString(node));
    }

    private IEnumerable<string> SuccessorsToEnumerableString(ICfgNode node) {
        if (node is CfgInstruction cfgInstruction) {
            return SuccessorsToEnumerableString(cfgInstruction);
        }
        if (node is SelectorNode selectorNode) {
            return SuccessorsToEnumerableString(selectorNode);
        }
        throw new ArgumentException($"Invalid node type {node.GetType().Name}");
    }

    private IEnumerable<string> SuccessorsToEnumerableString(CfgInstruction cfgInstruction) {
        return cfgInstruction.SuccessorsPerAddress.Select(e => $"{ToString(e.Value)}");
    }

    private IEnumerable<string> SuccessorsToEnumerableString(SelectorNode selectorNode) {
        return selectorNode.SuccessorsPerSignature.Select(e => $"{e.Key} => {ToString(e.Value)}");
    }
}