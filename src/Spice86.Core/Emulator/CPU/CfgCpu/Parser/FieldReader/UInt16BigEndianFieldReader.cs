namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.FieldReader;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.Memory.Indexable;

/// <summary>
/// Represents the UInt16BigEndianFieldReader class.
/// </summary>
public class UInt16BigEndianFieldReader : InstructionFieldReader<ushort> {
    public UInt16BigEndianFieldReader(IIndexable memory, InstructionReaderAddressSource addressSource) :
        base(memory, addressSource) {
    }

    /// <summary>
    /// InstructionField method.
    /// </summary>
    public override InstructionField<ushort> PeekField(bool finalValue) {
        if (!finalValue) {
            throw new ArgumentException("Can only peek final value for this type of field");
        }
        return base.PeekField(true);
    }

    protected override int FieldSize() {
        return 2;
    }

    /// <summary>
    /// ushort method.
    /// </summary>
    public override ushort PeekValue() {
        return Memory.UInt16BigEndian[CurrentAddress];
    }
}