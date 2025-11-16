namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.FieldReader;

using Spice86.Core.Emulator.Memory.Indexable;

/// <summary>
/// Represents the UInt16FieldReader class.
/// </summary>
public class UInt16FieldReader : InstructionFieldReader<ushort> {
    public UInt16FieldReader(IIndexable memory, InstructionReaderAddressSource addressSource) :
        base(memory, addressSource) {
    }

    protected override int FieldSize() {
        return 2;
    }

    /// <summary>
    /// ushort method.
    /// </summary>
    public override ushort PeekValue() {
        return Memory.UInt16[CurrentAddress];
    }
}