namespace Spice86.Tests.Dos;

using FluentAssertions;

using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.OperatingSystem.Structures;
using Spice86.Shared.Utils;

using Xunit;

public class DosTablesTests {
    [Fact]
    public void Initialize_CreatesCdsAndDbcs() {
        // Arrange
        var ram = new Ram(A20Gate.EndOfHighMemoryArea);
        var a20Gate = new A20Gate(true);
        var memory = new Memory(new(), ram, a20Gate);
        var dosTables = new DosTables();

        // Act
        dosTables.Initialize(memory);

        // Assert
        dosTables.CurrentDirectoryStructure.Should().NotBeNull();
        dosTables.DoubleByteCharacterSet.Should().NotBeNull();
    }

    [Fact]
    public void CurrentDirectoryStructure_InitializedWithCorrectPath() {
        // Arrange
        var ram = new Ram(A20Gate.EndOfHighMemoryArea);
        var a20Gate = new A20Gate(true);
        var memory = new Memory(new(), ram, a20Gate);
        var dosTables = new DosTables();

        // Act
        dosTables.Initialize(memory);

        // Assert
        // The CDS should be initialized with "C:\" which is 0x005c3a43 in little-endian
        // 0x43 = 'C', 0x3a = ':', 0x5c = '\', 0x00 = null terminator
        dosTables.CurrentDirectoryStructure!.CurrentPath.Should().Be(0x005c3a43u);
    }

    [Fact]
    public void DoubleByteCharacterSet_InitializedAsEmptyTable() {
        // Arrange
        var ram = new Ram(A20Gate.EndOfHighMemoryArea);
        var a20Gate = new A20Gate(true);
        var memory = new Memory(new(), ram, a20Gate);
        var dosTables = new DosTables();

        // Act
        dosTables.Initialize(memory);

        // Assert
        // The DBCS table should be initialized as empty (value 0)
        dosTables.DoubleByteCharacterSet!.DbcsLeadByteTable.Should().Be(0u);
    }

    [Fact]
    public void CurrentDirectoryStructure_LocatedAtCorrectSegment() {
        // Arrange
        var ram = new Ram(A20Gate.EndOfHighMemoryArea);
        var a20Gate = new A20Gate(true);
        var memory = new Memory(new(), ram, a20Gate);
        var dosTables = new DosTables();

        // Act
        dosTables.Initialize(memory);

        // Assert
        // CDS should be at DOS_CDS_SEG (0x108)
        uint expectedAddress = MemoryUtils.ToPhysicalAddress(MemoryMap.DosCdsSegment, 0);
        dosTables.CurrentDirectoryStructure!.BaseAddress.Should().Be(expectedAddress);
    }

    [Fact]
    public void DoubleByteCharacterSet_AllocatedInPrivateTablesArea() {
        // Arrange
        var ram = new Ram(A20Gate.EndOfHighMemoryArea);
        var a20Gate = new A20Gate(true);
        var memory = new Memory(new(), ram, a20Gate);
        var dosTables = new DosTables();

        // Act
        dosTables.Initialize(memory);

        // Assert
        // DBCS should be allocated in the DOS private tables segment area
        uint dbcsAddress = dosTables.DoubleByteCharacterSet!.BaseAddress;
        ushort dbcsSegment = MemoryUtils.ToSegment(dbcsAddress);
        dbcsSegment.Should().BeGreaterThanOrEqualTo(DosTables.DosPrivateTablesSegmentStart);
        dbcsSegment.Should().BeLessThan(DosTables.DosPrivateTablesSegmentEnd);
    }

    [Fact]
    public void GetDosPrivateTableWritableAddress_AdvancesMemoryPointer() {
        // Arrange
        var dosTables = new DosTables();
        ushort initialSegment = dosTables.CurrentMemorySegment;

        // Act
        ushort allocatedSegment1 = dosTables.GetDosPrivateTableWritableAddress(2);
        ushort allocatedSegment2 = dosTables.GetDosPrivateTableWritableAddress(3);

        // Assert
        allocatedSegment1.Should().Be(initialSegment);
        allocatedSegment2.Should().Be((ushort)(initialSegment + 2));
        dosTables.CurrentMemorySegment.Should().Be((ushort)(initialSegment + 5));
    }

    [Fact]
    public void GetDosPrivateTableWritableAddress_ThrowsWhenOutOfMemory() {
        // Arrange
        var dosTables = new DosTables();
        // Set to almost end of memory
        dosTables.CurrentMemorySegment = (ushort)(DosTables.DosPrivateTablesSegmentEnd - 1);

        // Act
        Action act = () => dosTables.GetDosPrivateTableWritableAddress(10);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("DOS: Not enough memory for internal tables!");
    }
}
