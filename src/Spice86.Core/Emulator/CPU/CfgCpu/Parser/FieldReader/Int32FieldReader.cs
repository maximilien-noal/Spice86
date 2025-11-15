namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.FieldReader;

using Spice86.Core.Emulator.Memory.Indexable;

/// <summary>
/// Represents int 32 field reader.
/// </summary>
public class Int32FieldReader : InstructionFieldReader<int> {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="memory">The memory.</param>
    /// <param name="addressSource">The address source.</param>
    public Int32FieldReader(IIndexable memory, InstructionReaderAddressSource addressSource) :
        base(memory, addressSource) {
    }

    /// <summary>
    /// Performs the field size operation.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    protected override int FieldSize() {
        return 4;
    }

    /// <summary>
    /// Performs the peek value operation.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    public override int PeekValue() {
        return Memory.Int32[CurrentAddress];
    }
}