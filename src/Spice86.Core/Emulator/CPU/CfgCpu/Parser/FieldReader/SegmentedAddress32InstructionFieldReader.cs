namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.FieldReader;

using Spice86.Core.Emulator.Memory.Indexable;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents segmented address 32 instruction field reader.
/// </summary>
public class SegmentedAddress32InstructionFieldReader : InstructionFieldReader<SegmentedAddress> {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="memory">The memory.</param>
    /// <param name="addressSource">The address source.</param>
    public SegmentedAddress32InstructionFieldReader(IIndexable memory, InstructionReaderAddressSource addressSource) :
        base(memory, addressSource) {
    }

    /// <summary>
    /// Performs the field size operation.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    protected override int FieldSize() {
        return 6;
    }

    /// <summary>
    /// Performs the peek value operation.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    public override SegmentedAddress PeekValue() {
        // We still read a segmented address since in real mode we ignore the last 2 bytes
        return Memory.SegmentedAddress32[CurrentAddress];
    }
}