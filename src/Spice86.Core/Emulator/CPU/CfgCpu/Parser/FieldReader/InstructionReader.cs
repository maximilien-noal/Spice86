namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.FieldReader;

using Spice86.Core.Emulator.Memory.Indexable;

/// <summary>
/// Represents instruction reader.
/// </summary>
public class InstructionReader {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="memory">The memory.</param>
    public InstructionReader(IIndexable memory) {
        InstructionReaderAddressSource = new InstructionReaderAddressSource(new(0, 0));
        Int8 = new(memory, InstructionReaderAddressSource);
        UInt8 = new(memory, InstructionReaderAddressSource);
        UInt8AsUshort = new(memory, InstructionReaderAddressSource);
        Int16 = new(memory, InstructionReaderAddressSource);
        UInt16 = new(memory, InstructionReaderAddressSource);
        UInt16BigEndian = new UInt16BigEndianFieldReader(memory, InstructionReaderAddressSource);
        Int32 = new(memory, InstructionReaderAddressSource);
        UInt32 = new(memory, InstructionReaderAddressSource);
        SegmentedAddress16 = new(memory, InstructionReaderAddressSource);
        SegmentedAddress32 = new(memory, InstructionReaderAddressSource);
    }

    /// <summary>
    /// Gets instruction reader address source.
    /// </summary>
    public InstructionReaderAddressSource InstructionReaderAddressSource { get; }
    /// <summary>
    /// Gets int 8.
    /// </summary>
    public Int8FieldReader Int8 { get; }
    /// <summary>
    /// Gets u int 8.
    /// </summary>
    public UInt8FieldReader UInt8 { get; }
    /// <summary>
    /// Gets u int 8 as ushort.
    /// </summary>
    public UInt8AsUshortFieldReader UInt8AsUshort { get; }
    /// <summary>
    /// Gets int 16.
    /// </summary>
    public Int16FieldReader Int16 { get; }
    /// <summary>
    /// Gets u int 16.
    /// </summary>
    public UInt16FieldReader UInt16 { get; }
    /// <summary>
    /// Gets u int 16 big endian.
    /// </summary>
    public UInt16BigEndianFieldReader UInt16BigEndian { get; }
    /// <summary>
    /// Gets int 32.
    /// </summary>
    public Int32FieldReader Int32 { get; }
    /// <summary>
    /// Gets u int 32.
    /// </summary>
    public UInt32FieldReader UInt32 { get; }
    /// <summary>
    /// Gets segmented address 16.
    /// </summary>
    public SegmentedAddress16InstructionFieldReader SegmentedAddress16 { get; }

    /// <summary>
    /// Gets segmented address 32.
    /// </summary>
    public SegmentedAddress32InstructionFieldReader SegmentedAddress32 { get; }
}