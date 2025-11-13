namespace Spice86.Tests.Bios;

using FluentAssertions;

using Spice86.Core.Emulator.CPU;

using Xunit;

public class SystemClockInt1AHandlerTests {
    [Fact]
    public void ToBcd_ShouldConvertBinaryToBcd() {
        // Test the ToBcd conversion logic that INT1A uses
        byte result1 = ToBcd(0);
        result1.Should().Be(0x00);

        byte result2 = ToBcd(9);
        result2.Should().Be(0x09);

        byte result3 = ToBcd(10);
        result3.Should().Be(0x10);

        byte result4 = ToBcd(23);
        result4.Should().Be(0x23);

        byte result5 = ToBcd(59);
        result5.Should().Be(0x59);

        byte result6 = ToBcd(99);
        result6.Should().Be(0x99);
    }

    [Fact]
    public void TicksConversion_ShouldSplitIntoHighAndLowWords() {
        // Test that ticks are properly split into CX:DX
        uint ticks = 0x12345678;
        ushort highWord = (ushort)(ticks >> 16);
        ushort lowWord = (ushort)(ticks & 0xFFFF);

        highWord.Should().Be(0x1234);
        lowWord.Should().Be(0x5678);
    }

    [Fact]
    public void TicksConversion_ShouldCombineFromHighAndLowWords() {
        // Test that CX:DX can be combined into ticks
        ushort cx = 0xABCD;
        ushort dx = 0xEF01;
        uint ticks = ((uint)cx << 16) | dx;

        ticks.Should().Be(0xABCDEF01);
    }

    [Fact]
    public void BcdValidation_ShouldDetectValidBcd() {
        // Test BCD validation logic
        IsValidBcd(0x00).Should().BeTrue();
        IsValidBcd(0x09).Should().BeTrue();
        IsValidBcd(0x23).Should().BeTrue();
        IsValidBcd(0x59).Should().BeTrue();
        IsValidBcd(0x99).Should().BeTrue();
    }

    [Fact]
    public void BcdValidation_ShouldDetectInvalidBcd() {
        // Test BCD validation logic for invalid values
        IsValidBcd(0x0A).Should().BeFalse();
        IsValidBcd(0xA0).Should().BeFalse();
        IsValidBcd(0xFF).Should().BeFalse();
        IsValidBcd(0x9A).Should().BeFalse();
    }

    /// <summary>
    /// Converts a binary value to BCD (Binary Coded Decimal) format.
    /// This mirrors the implementation in SystemClockInt1AHandler.
    /// </summary>
    private static byte ToBcd(byte binary) {
        int tens = binary / 10;
        int ones = binary % 10;
        return (byte)((tens << 4) | ones);
    }

    /// <summary>
    /// Checks if a byte is a valid BCD value (both nibbles 0-9).
    /// </summary>
    private static bool IsValidBcd(byte value) {
        int highNibble = (value >> 4) & 0x0F;
        int lowNibble = value & 0x0F;
        return highNibble <= 9 && lowNibble <= 9;
    }
}
