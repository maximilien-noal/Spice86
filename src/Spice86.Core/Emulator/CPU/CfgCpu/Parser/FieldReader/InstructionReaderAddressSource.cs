namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.FieldReader;

using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents the InstructionReaderAddressSource class.
/// </summary>
public class InstructionReaderAddressSource {
    private SegmentedAddress _instructionAddress;

    /// <summary>
    /// The InstructionAddress.
    /// </summary>
    public SegmentedAddress InstructionAddress {
        get => _instructionAddress;
        set {
            _instructionAddress = value;
            IndexInInstruction = 0;
        }
    }

    /// <summary>
    /// Gets or sets the IndexInInstruction.
    /// </summary>
    public int IndexInInstruction { get; set; }

    /// <summary>
    /// CurrentAddress method.
    /// </summary>
    public SegmentedAddress CurrentAddress => new SegmentedAddress(InstructionAddress.Segment,
        (ushort)(InstructionAddress.Offset + IndexInInstruction));

    public InstructionReaderAddressSource(SegmentedAddress instructionAddress) {
        _instructionAddress = instructionAddress;
    }
}