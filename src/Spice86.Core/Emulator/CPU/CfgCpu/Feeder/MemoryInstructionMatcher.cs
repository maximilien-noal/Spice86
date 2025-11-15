namespace Spice86.Core.Emulator.CPU.CfgCpu.Feeder;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.Memory;

using System.Linq;

/// <summary>
/// Represents memory instruction matcher.
/// </summary>
public class MemoryInstructionMatcher {
    private readonly IMemory _memory;

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="memory">The memory.</param>
    public MemoryInstructionMatcher(IMemory memory) {
        this._memory = memory;
    }

    /// <summary>
    /// Performs the match existing instruction with memory operation.
    /// </summary>
    /// <param name="instructions">The instructions.</param>
    public CfgInstruction? MatchExistingInstructionWithMemory(IEnumerable<CfgInstruction> instructions) {
        return instructions.FirstOrDefault(i => IsMatchingWithCurrentMemory(i));
    }

    private bool IsMatchingWithCurrentMemory(CfgInstruction instruction) {
        IList<byte> bytesInMemory = _memory.GetSlice((int)instruction.Address.Linear, instruction.Length);
        return instruction.Signature.ListEquivalent(bytesInMemory);
    }

}