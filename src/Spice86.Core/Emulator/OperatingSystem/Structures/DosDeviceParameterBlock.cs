namespace Spice86.Core.Emulator.OperatingSystem.Structures;

using Spice86.Core.Emulator.Memory.ReaderWriter;
using Spice86.Core.Emulator.ReverseEngineer.DataStructure;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents the DosDeviceParameterBlock class.
/// </summary>
public class DosDeviceParameterBlock : MemoryBasedDataStructure {
    public DosDeviceParameterBlock(IByteReaderWriter byteReaderWriter, uint baseAddress) : base(byteReaderWriter, baseAddress) {
    }

    /// <summary>
    /// The DeviceType.
    /// </summary>
    public byte DeviceType {
        get => UInt8[0x1];
        set => UInt8[0x1] = value;
    }

    /// <summary>
    /// The DeviceAttributes.
    /// </summary>
    public ushort DeviceAttributes {
        get => UInt16[0x2];
        set => UInt16[0x2] = value;
    }

    /// <summary>
    /// The Cylinders.
    /// </summary>
    public ushort Cylinders {
        get => UInt16[0x4];
        set => UInt16[0x4] = value;
    }

    /// <summary>
    /// The MediaType.
    /// </summary>
    public byte MediaType {
        get => UInt8[0x6];
        set => UInt8[0x6] = value;
    }

    /// <summary>
    /// The BiosParameterBlock.
    /// </summary>
    public TruncatedBiosParameterBlock BiosParameterBlock {
        get => new(ByteReaderWriter, BaseAddress + 0x7);
    }
}