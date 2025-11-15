namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.FieldReader;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.Memory.Indexable;

/// <summary>
/// Represents u int 16 big endian field reader.
/// </summary>
public class UInt16BigEndianFieldReader : InstructionFieldReader<ushort> {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="memory">The memory.</param>
    /// <param name="addressSource">The address source.</param>
    public UInt16BigEndianFieldReader(IIndexable memory, InstructionReaderAddressSource addressSource) :
        base(memory, addressSource) {
    }

    /// <summary>
    /// Performs the peek field operation.
    /// </summary>
    /// <param name="finalValue">The final value.</param>
    /// <returns>The result of the operation.</returns>
    public override InstructionField<ushort> PeekField(bool finalValue) {
        if (!finalValue) {
            throw new ArgumentException("Can only peek final value for this type of field");
        }
        return base.PeekField(true);
    }

    /// <summary>
    /// Performs the field size operation.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    protected override int FieldSize() {
        return 2;
    }

    /// <summary>
    /// Performs the peek value operation.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    public override ushort PeekValue() {
        return Memory.UInt16BigEndian[CurrentAddress];
    }
}