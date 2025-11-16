namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.FieldReader;

using Spice86.Core.Emulator.Memory.Indexable;

/// <summary>
/// Represents the InstructionReader class.
/// </summary>
public class InstructionReader {
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
    /// Gets or sets the InstructionReaderAddressSource.
    /// </summary>
    public InstructionReaderAddressSource InstructionReaderAddressSource { get; }
    /// <summary>
    /// Gets or sets the Int8.
    /// </summary>
    public Int8FieldReader Int8 { get; }
    /// <summary>
    /// Gets or sets the UInt8.
    /// </summary>
    public UInt8FieldReader UInt8 { get; }
    /// <summary>
    /// Gets or sets the UInt8AsUshort.
    /// </summary>
    public UInt8AsUshortFieldReader UInt8AsUshort { get; }
    /// <summary>
    /// Gets or sets the Int16.
    /// </summary>
    public Int16FieldReader Int16 { get; }
    /// <summary>
    /// Gets or sets the UInt16.
    /// </summary>
    public UInt16FieldReader UInt16 { get; }
    /// <summary>
    /// Gets or sets the UInt16BigEndian.
    /// </summary>
    public UInt16BigEndianFieldReader UInt16BigEndian { get; }
    /// <summary>
    /// Gets or sets the Int32.
    /// </summary>
    public Int32FieldReader Int32 { get; }
    /// <summary>
    /// Gets or sets the UInt32.
    /// </summary>
    public UInt32FieldReader UInt32 { get; }
    /// <summary>
    /// Gets or sets the SegmentedAddress16.
    /// </summary>
    public SegmentedAddress16InstructionFieldReader SegmentedAddress16 { get; }

    /// <summary>
    /// Gets or sets the SegmentedAddress32.
    /// </summary>
    public SegmentedAddress32InstructionFieldReader SegmentedAddress32 { get; }
}