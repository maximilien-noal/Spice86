namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.FieldReader;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.Memory.Indexable;

/// <summary>
/// Represents the UInt8AsUshortFieldReader class.
/// </summary>
public class UInt8AsUshortFieldReader : InstructionFieldReader<ushort> {
    public UInt8AsUshortFieldReader(IIndexable memory, InstructionReaderAddressSource addressSource) :
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
        return 1;
    }

    /// <summary>
    /// ushort method.
    /// </summary>
    public override ushort PeekValue() {
        return Memory.UInt8[CurrentAddress];
    }
}