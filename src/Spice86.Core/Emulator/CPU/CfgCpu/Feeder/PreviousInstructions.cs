namespace Spice86.Core.Emulator.CPU.CfgCpu.Feeder;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.Memory;

/// <summary>
/// Cache of previous instructions that existed in a memory address at a time.
/// </summary>
public class PreviousInstructions : IInstructionReplacer<CfgInstruction> {
    private readonly MemoryInstructionMatcher _memoryInstructionMatcher;

    /// <summary>
    /// Instructions that were parsed at a given address. List for address is ordered by instruction decreasing length
    /// </summary>
    private readonly IDictionary<uint, ISet<CfgInstruction>> _previousInstructionsAtAddress =
        new Dictionary<uint, ISet<CfgInstruction>>();

    public PreviousInstructions(IMemory memory) {
        _memoryInstructionMatcher = new MemoryInstructionMatcher(memory);
    }

    public CfgInstruction? GetAtAddress(uint physicalAddress) {
        if (_previousInstructionsAtAddress.TryGetValue(physicalAddress,
                out ISet<CfgInstruction>? previousInstructionsAtAddress)) {
            return _memoryInstructionMatcher.MatchExistingInstructionWithMemory(previousInstructionsAtAddress);
        }

        return null;
    }

    public void ReplaceInstruction(CfgInstruction old, CfgInstruction instruction) {
        uint instructionAddress = instruction.PhysicalAddress;

        if (_previousInstructionsAtAddress.TryGetValue(instructionAddress,
                out ISet<CfgInstruction>? previousInstructionsAtAddress)) {
            if (previousInstructionsAtAddress.Remove(old)) {
                AddInstructionInPrevious(instruction);
            }
        }
    }

    public void AddInstructionInPrevious(CfgInstruction instruction) {
        uint instructionAddress = instruction.PhysicalAddress;

        if (!_previousInstructionsAtAddress.TryGetValue(instructionAddress,
                out ISet<CfgInstruction>? previousInstructionsAtAddress)) {
            previousInstructionsAtAddress = new HashSet<CfgInstruction>();
            _previousInstructionsAtAddress.Add(instructionAddress, previousInstructionsAtAddress);
        }

        previousInstructionsAtAddress.Add(instruction);
    }
}