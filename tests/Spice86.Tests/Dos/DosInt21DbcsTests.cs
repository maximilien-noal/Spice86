namespace Spice86.Tests.Dos;

using FluentAssertions;

using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.OperatingSystem.Structures;
using Spice86.Shared.Utils;

using Xunit;

/// <summary>
/// Tests for the DBCS (Double Byte Character Set) functionality in INT 21h, AH=63h.
/// These tests verify that the DOS tables correctly initialize and provide access to the DBCS table.
/// </summary>
public class DosInt21DbcsTests {
    [Fact]
    public void DbcsTable_InitializedCorrectly_PointsToMemory() {
        // Arrange
        var ram = new Ram(A20Gate.EndOfHighMemoryArea);
        var a20Gate = new A20Gate(true);
        var memory = new Memory(new(), ram, a20Gate);
        var dosTables = new DosTables();

        // Act
        dosTables.Initialize(memory);

        // Assert
        dosTables.DoubleByteCharacterSet.Should().NotBeNull();
        dosTables.DoubleByteCharacterSet!.BaseAddress.Should().BeGreaterThan(0u);
    }

    [Fact]
    public void DbcsTable_InitializedAsEmpty_ContainsZero() {
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
        
        // Verify it's actually written to memory
        uint dbcsAddress = dosTables.DoubleByteCharacterSet.BaseAddress;
        memory.UInt32[dbcsAddress].Should().Be(0u);
    }

    [Fact]
    public void DbcsTable_AddressCalculation_MatchesSegmentOffset() {
        // Arrange
        var ram = new Ram(A20Gate.EndOfHighMemoryArea);
        var a20Gate = new A20Gate(true);
        var memory = new Memory(new(), ram, a20Gate);
        var dosTables = new DosTables();

        // Act
        dosTables.Initialize(memory);

        // Assert
        uint dbcsAddress = dosTables.DoubleByteCharacterSet!.BaseAddress;
        ushort segment = MemoryUtils.ToSegment(dbcsAddress);
        ushort offset = (ushort)(dbcsAddress & 0xF);
        
        // Verify we can reconstruct the address from segment:offset
        uint reconstructedAddress = MemoryUtils.ToPhysicalAddress(segment, offset);
        reconstructedAddress.Should().Be(dbcsAddress);
    }
}
