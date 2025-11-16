namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.FieldReader;

using Spice86.Core.Emulator.Memory.Indexable;

/// <summary>
/// Represents the Int16FieldReader class.
/// </summary>
public class Int16FieldReader : InstructionFieldReader<short> {
    public Int16FieldReader(IIndexable memory, InstructionReaderAddressSource addressSource) :
        base(memory, addressSource) {
    }

    protected override int FieldSize() {
        return 2;
    }

    /// <summary>
    /// short method.
    /// </summary>
    public override short PeekValue() {
        return Memory.Int16[CurrentAddress];
    }
}