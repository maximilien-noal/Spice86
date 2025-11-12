namespace Spice86.ViewModels;

using Spice86.Core.Emulator.InterruptHandlers.Dos.Xms;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.Memory.Indexable;
using Spice86.Core.Emulator.Memory.Indexer;
using Spice86.Core.Emulator.Memory.ReaderWriter;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// IMemory implementation that wraps XMS RAM for use in the memory viewer.
/// This allows viewing the separate XMS RAM that is managed by the XMS driver.
/// </summary>
internal sealed class XmsMemory : Indexable, IMemory, IByteReaderWriter {
    private readonly Ram _xmsRam;

    public XmsMemory(ExtendedMemoryManager xmsManager) {
        _xmsRam = xmsManager.XmsRam;
        Ram = _xmsRam;
        Length = (int)_xmsRam.Size;
        
        // Manually initialize indexers using this wrapper as IByteReaderWriter
        UInt8 = new UInt8Indexer(this);
        UInt16 = new UInt16Indexer(this);
        UInt16BigEndian = new UInt16BigEndianIndexer(this);
        UInt32 = new UInt32Indexer(this);
        Int8 = new Int8Indexer(UInt8);
        Int16 = new Int16Indexer(UInt16);
        Int32 = new Int32Indexer(UInt32);
        SegmentedAddress16 = new SegmentedAddress16Indexer(UInt16);
        SegmentedAddress32 = new SegmentedAddress32Indexer(UInt16, UInt32);
    }

    public int Length { get; }

    public byte this[uint address] {
        get => _xmsRam.Read(address);
        set => _xmsRam.Write(address, value);
    }

    public IMemoryDevice Ram { get; }
    
    // A20Gate not applicable for XMS memory
    public A20Gate A20Gate => throw new NotSupportedException("A20Gate not available for XMS memory");
    
    public byte CurrentlyWritingByte { get; set; }

    public void RegisterMapping(uint startAddress, uint length, IMemoryDevice device) {
        // Not supported for XMS memory view
    }

    public IMemoryDevice? SearchMapping(uint address) {
        return address < _xmsRam.Size ? _xmsRam : null;
    }

    public ushort SegmentOverrideForMemoryBreakpoints { get; set; }
    public bool BreakOnNextSegmentAccess { get; set; }

    public byte SneakilyRead(uint address) => _xmsRam.Read(address);
    public void SneakilyWrite(uint address, byte value) => _xmsRam.Write(address, value);

    public byte[] ReadRam(uint length = 0, uint offset = 0) {
        if (length == 0) {
            length = _xmsRam.Size - offset;
        }
        length = Math.Min(length, _xmsRam.Size - offset);
        byte[] result = new byte[length];
        for (uint i = 0; i < length; i++) {
            result[i] = _xmsRam.Read(offset + i);
        }
        return result;
    }

    public void WriteRam(byte[] data, uint offset = 0) {
        uint length = Math.Min((uint)data.Length, _xmsRam.Size - offset);
        for (uint i = 0; i < length; i++) {
            _xmsRam.Write(offset + i, data[i]);
        }
    }

    public uint? SearchValue(uint startAddress, int length, IList<byte> value) {
        for (uint addr = startAddress; addr < Math.Min(startAddress + length, _xmsRam.Size); addr++) {
            if (addr + value.Count > _xmsRam.Size) break;
            
            bool match = true;
            for (int i = 0; i < value.Count; i++) {
                if (_xmsRam.Read(addr + (uint)i) != value[i]) {
                    match = false;
                    break;
                }
            }
            if (match) {
                return addr;
            }
        }
        return null;
    }

    public IList<byte> GetSlice(int address, int length) {
        return _xmsRam.GetSlice(address, length);
    }

    // Indexers are initialized in constructor via InstantiateIndexersFromByteReaderWriter
    public override UInt8Indexer UInt8 { get; }
    public override UInt16Indexer UInt16 { get; }
    public override UInt16BigEndianIndexer UInt16BigEndian { get; }
    public override UInt32Indexer UInt32 { get; }
    public override Int8Indexer Int8 { get; }
    public override Int16Indexer Int16 { get; }
    public override Int32Indexer Int32 { get; }
    public override SegmentedAddress16Indexer SegmentedAddress16 { get; }
    public override SegmentedAddress32Indexer SegmentedAddress32 { get; }
}
