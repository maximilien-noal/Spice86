namespace Spice86.Tests.Dos;

using FluentAssertions;

using Spice86.Core.Emulator.OperatingSystem;

using Xunit;

/// <summary>
/// Tests for DosProcessManager utility methods.
/// </summary>
public class DosProcessManagerTests {
    [Theory]
    [InlineData("C:\\INSECTS.EXE", "C:\\")]
    [InlineData("C:\\GAMES\\INSECTS.EXE", "C:\\GAMES\\")]
    [InlineData("C:\\GAMES\\SUBFOLDER\\INSECTS.EXE", "C:\\GAMES\\SUBFOLDER\\")]
    [InlineData("C:/GAMES/INSECTS.EXE", "C:\\GAMES\\")]  // Forward slashes should be converted
    [InlineData("INSECTS.EXE", "")]  // No directory component
    [InlineData("C:\\", "C:\\")]  // Just root
    [InlineData("D:\\FILE.EXE", "D:\\")]  // Different drive letter
    public void ExtractDosDirectory_CorrectlyExtractsDirectory(string dosPath, string expectedDirectory) {
        // Act
        string result = DosProcessManager.ExtractDosDirectory(dosPath);

        // Assert
        result.Should().Be(expectedDirectory);
    }
}
