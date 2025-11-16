namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.FieldReader;

using Spice86.Core.Emulator.Memory.Indexable;

/// <summary>
/// Represents the Int8FieldReader class.
/// </summary>
public class Int8FieldReader : InstructionFieldReader<sbyte> {
    public Int8FieldReader(IIndexable memory, InstructionReaderAddressSource addressSource) :
        base(memory, addressSource) {
    }

    protected override int FieldSize() {
        return 1;
    }

    /// <summary>
    /// sbyte method.
    /// </summary>
    public override sbyte PeekValue() {
        return Memory.Int8[CurrentAddress];
    }
}