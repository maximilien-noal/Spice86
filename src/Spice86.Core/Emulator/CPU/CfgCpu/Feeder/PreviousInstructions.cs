namespace Spice86.Core.Emulator.CPU.CfgCpu.Feeder;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.Memory;
using Spice86.Shared.Emulator.Memory;

using System.Linq;

/// <summary>
/// Cache of previous instructions that existed in a memory address at a time.
/// </summary>
public class PreviousInstructions : InstructionReplacer {
    private readonly MemoryInstructionMatcher _memoryInstructionMatcher;

    /// <summary>
    /// Instructions that were parsed at a given address. List for address is ordered by instruction decreasing length
    /// </summary>
    private readonly Dictionary<SegmentedAddress, HashSet<CfgInstruction>> _previousInstructionsAtAddress = new();

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="memory">The memory.</param>
    /// <param name="replacerRegistry">The replacer registry.</param>
    public PreviousInstructions(IMemory memory, InstructionReplacerRegistry replacerRegistry) : base(
        replacerRegistry) {
        _memoryInstructionMatcher = new MemoryInstructionMatcher(memory);
    }

    /// <summary>
    /// Gets all.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    public List<CfgInstruction> GetAll() {
        return _previousInstructionsAtAddress.Values.SelectMany(x => x).ToList();
    }

    /// <summary>
    /// Gets at address.
    /// </summary>
    /// <param name="address">The address.</param>
    public HashSet<CfgInstruction>? GetAtAddress(SegmentedAddress address) {
        return _previousInstructionsAtAddress.GetValueOrDefault(address);
    }

    /// <summary>
    /// Gets at address if matches memory.
    /// </summary>
    /// <param name="address">The address.</param>
    public CfgInstruction? GetAtAddressIfMatchesMemory(SegmentedAddress address) {
        HashSet<CfgInstruction>? previousInstructionsAtAddress = GetAtAddress(address);
        if (previousInstructionsAtAddress == null) {
            return null;
        }

        return _memoryInstructionMatcher.MatchExistingInstructionWithMemory(previousInstructionsAtAddress);
    }

    /// <summary>
    /// Performs the replace instruction operation.
    /// </summary>
    /// <param name="oldInstruction">The old instruction.</param>
    /// <param name="newInstruction">The new instruction.</param>
    public override void ReplaceInstruction(CfgInstruction oldInstruction, CfgInstruction newInstruction) {
        SegmentedAddress instructionAddress = newInstruction.Address;

        if (_previousInstructionsAtAddress.TryGetValue(instructionAddress,
                out HashSet<CfgInstruction>? previousInstructionsAtAddress)
            && previousInstructionsAtAddress.Remove(oldInstruction)) {
            AddInstructionInPrevious(newInstruction);
        }
    }

    /// <summary>
    /// Adds instruction in previous.
    /// </summary>
    /// <param name="instruction">The instruction.</param>
    public void AddInstructionInPrevious(CfgInstruction instruction) {
        SegmentedAddress instructionAddress = instruction.Address;

        if (!_previousInstructionsAtAddress.TryGetValue(instructionAddress,
                out HashSet<CfgInstruction>? previousInstructionsAtAddress)) {
            previousInstructionsAtAddress = new HashSet<CfgInstruction>();
            _previousInstructionsAtAddress.Add(instructionAddress, previousInstructionsAtAddress);
        }

        previousInstructionsAtAddress.Add(instruction);
    }
}