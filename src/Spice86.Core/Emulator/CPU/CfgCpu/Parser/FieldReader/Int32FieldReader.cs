namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.FieldReader;

using Spice86.Core.Emulator.Memory.Indexable;

/// <summary>
/// Represents the Int32FieldReader class.
/// </summary>
public class Int32FieldReader : InstructionFieldReader<int> {
    public Int32FieldReader(IIndexable memory, InstructionReaderAddressSource addressSource) :
        base(memory, addressSource) {
    }

    protected override int FieldSize() {
        return 4;
    }

    /// <summary>
    /// int method.
    /// </summary>
    public override int PeekValue() {
        return Memory.Int32[CurrentAddress];
    }
}