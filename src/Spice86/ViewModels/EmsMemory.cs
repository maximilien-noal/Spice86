namespace Spice86.ViewModels;

using Spice86.Core.Emulator.InterruptHandlers.Dos.Ems;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.Memory.Indexable;
using Spice86.Core.Emulator.Memory.Indexer;
using Spice86.Core.Emulator.Memory.ReaderWriter;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// IMemory implementation that aggregates all EMS pages for use in the memory viewer.
/// This allows viewing all allocated EMS pages as a contiguous memory space.
/// </summary>
internal sealed class EmsMemory : Indexable, IMemory, IByteReaderWriter {
    private readonly ExpandedMemoryManager _emsManager;
    private readonly List<EmmPage> _allPages;

    public EmsMemory(ExpandedMemoryManager emsManager) {
        _emsManager = emsManager;
        
        // Collect all allocated EMS pages from all handles
        _allPages = new List<EmmPage>();
        foreach (var handle in emsManager.EmmHandles.Values) {
            _allPages.AddRange(handle.LogicalPages);
        }
        
        Length = _allPages.Count * (int)ExpandedMemoryManager.EmmPageSize;
        
        // Manually initialize indexers
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

    private (EmmPage? page, int offset) FindPage(uint address) {
        int pageNumber = (int)(address / ExpandedMemoryManager.EmmPageSize);
        int pageOffset = (int)(address % ExpandedMemoryManager.EmmPageSize);
        
        if (pageNumber >= 0 && pageNumber < _allPages.Count) {
            return (_allPages[pageNumber], pageOffset);
        }
        
        return (null, 0);
    }

    public byte this[uint address] {
        get {
            var (page, offset) = FindPage(address);
            return page?.Read((uint)offset) ?? 0;
        }
        set {
            var (page, offset) = FindPage(address);
            page?.Write((uint)offset, value);
        }
    }

    public IMemoryDevice Ram => throw new NotSupportedException("Ram not available for EMS memory");
    public A20Gate A20Gate => throw new NotSupportedException("A20Gate not available for EMS memory");
    public byte CurrentlyWritingByte { get; set; }

    public void RegisterMapping(uint startAddress, uint length, IMemoryDevice device) {
        // Not supported for EMS memory view
    }

    public IMemoryDevice? SearchMapping(uint address) {
        var (page, _) = FindPage(address);
        return page;
    }

    public ushort SegmentOverrideForMemoryBreakpoints { get; set; }
    public bool BreakOnNextSegmentAccess { get; set; }

    public byte SneakilyRead(uint address) => this[address];
    public void SneakilyWrite(uint address, byte value) => this[address] = value;

    public byte[] ReadRam(uint length = 0, uint offset = 0) {
        if (length == 0) {
            length = (uint)Length - offset;
        }
        length = Math.Min(length, (uint)Length - offset);
        byte[] result = new byte[length];
        for (uint i = 0; i < length; i++) {
            result[i] = this[offset + i];
        }
        return result;
    }

    public void WriteRam(byte[] data, uint offset = 0) {
        uint length = Math.Min((uint)data.Length, (uint)Length - offset);
        for (uint i = 0; i < length; i++) {
            this[offset + i] = data[i];
        }
    }

    public uint? SearchValue(uint startAddress, int length, IList<byte> value) {
        for (uint addr = startAddress; addr < Math.Min(startAddress + length, (uint)Length); addr++) {
            if (addr + value.Count > Length) break;
            
            bool match = true;
            for (int i = 0; i < value.Count; i++) {
                if (this[addr + (uint)i] != value[i]) {
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
        byte[] result = new byte[length];
        for (int i = 0; i < length && address + i < Length; i++) {
            result[i] = this[(uint)(address + i)];
        }
        return result;
    }

    // IByteReaderWriter implementation for indexer initialization
    public byte Read(uint address) => this[address];
    public void Write(uint address, byte value) => this[address] = value;

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
