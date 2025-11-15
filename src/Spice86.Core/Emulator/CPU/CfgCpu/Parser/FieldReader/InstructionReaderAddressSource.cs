namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.FieldReader;

using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents instruction reader address source.
/// </summary>
public class InstructionReaderAddressSource {
    private SegmentedAddress _instructionAddress;

    public SegmentedAddress InstructionAddress {
        get => _instructionAddress;
        set {
            _instructionAddress = value;
            IndexInInstruction = 0;
        }
    }

    /// <summary>
    /// Gets or sets index in instruction.
    /// </summary>
    public int IndexInInstruction { get; set; }

    /// <summary>
    /// The current address.
    /// </summary>
    public SegmentedAddress CurrentAddress => new SegmentedAddress(InstructionAddress.Segment,
        (ushort)(InstructionAddress.Offset + IndexInInstruction));

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="instructionAddress">The instruction address.</param>
    public InstructionReaderAddressSource(SegmentedAddress instructionAddress) {
        _instructionAddress = instructionAddress;
    }
}