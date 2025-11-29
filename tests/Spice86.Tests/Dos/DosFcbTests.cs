namespace Spice86.Tests.Dos;

using FluentAssertions;

using NSubstitute;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.Memory.ReaderWriter;
using Spice86.Core.Emulator.OperatingSystem;
using Spice86.Core.Emulator.OperatingSystem.Devices;
using Spice86.Core.Emulator.OperatingSystem.Structures;
using Spice86.Core.Emulator.VM.Breakpoint;
using Spice86.Shared.Interfaces;

using Xunit;

/// <summary>
/// Unit tests for DOS File Control Block (FCB) operations.
/// </summary>
public class DosFcbTests : IDisposable {
    private readonly ILoggerService _loggerService;
    private readonly IMemory _memory;
    private string? _testDir;

    public DosFcbTests() {
        _loggerService = Substitute.For<ILoggerService>();
        
        // Create backing memory
        IMemoryDevice ram = new Ram(A20Gate.EndOfHighMemoryArea);
        Core.Emulator.VM.PauseHandler pauseHandler = new(_loggerService);
        State cpuState = new(CpuModel.INTEL_80286);
        EmulatorBreakpointsManager emulatorBreakpointsManager = new(pauseHandler, cpuState);
        A20Gate a20Gate = new(enabled: false);
        _memory = new Memory(emulatorBreakpointsManager.MemoryReadWriteBreakpoints, ram, a20Gate,
            initializeResetVector: true);
    }

    /// <summary>
    /// Creates a temporary test directory and returns the FCB manager set up to use it.
    /// </summary>
    private DosFcbManager CreateFcbManagerWithTestDirectory() {
        _testDir = Path.Combine(Path.GetTempPath(), "fcbtest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDir);
        
        string executablePath = Path.Combine(_testDir, "test.exe");
        DosDriveManager driveManager = new(_loggerService, _testDir, executablePath);
        DosStringDecoder stringDecoder = new(_memory, null!);
        DosFileManager dosFileManager = new(_memory, stringDecoder, driveManager, _loggerService, 
            new List<IVirtualDevice>());
        
        return new DosFcbManager(_memory, dosFileManager, driveManager, _loggerService);
    }

    /// <summary>
    /// Cleans up the test directory if it was created.
    /// </summary>
    public void Dispose() {
        if (_testDir != null && Directory.Exists(_testDir)) {
            Directory.Delete(_testDir, true);
        }
    }

    /// <summary>
    /// Tests that the DosFileControlBlock structure correctly reads and writes drive numbers.
    /// </summary>
    [Fact]
    public void DosFileControlBlock_DriveNumber_ReadsAndWritesCorrectly() {
        // Arrange
        DosFileControlBlock fcb = new(_memory, 0x1000);

        // Act
        fcb.DriveNumber = 3; // C: drive

        // Assert
        fcb.DriveNumber.Should().Be(3);
    }

    /// <summary>
    /// Tests that the DosFileControlBlock structure correctly handles file names with space padding.
    /// </summary>
    [Fact]
    public void DosFileControlBlock_FileName_IsSpacePadded() {
        // Arrange
        DosFileControlBlock fcb = new(_memory, 0x1000);

        // Act
        fcb.FileName = "TEST";

        // Assert
        fcb.FileName.Should().Be("TEST    "); // Padded to 8 characters
    }

    /// <summary>
    /// Tests that the DosFileControlBlock structure correctly handles file extensions.
    /// </summary>
    [Fact]
    public void DosFileControlBlock_FileExtension_IsSpacePadded() {
        // Arrange
        DosFileControlBlock fcb = new(_memory, 0x1000);

        // Act
        fcb.FileExtension = "TXT";

        // Assert
        fcb.FileExtension.Should().Be("TXT");
    }

    /// <summary>
    /// Tests that the DosFileControlBlock correctly calculates the full file name.
    /// </summary>
    [Fact]
    public void DosFileControlBlock_FullFileName_CombinesNameAndExtension() {
        // Arrange
        DosFileControlBlock fcb = new(_memory, 0x1000);

        // Act
        fcb.FileName = "TEST";
        fcb.FileExtension = "TXT";

        // Assert
        fcb.FullFileName.Should().Be("TEST.TXT");
    }

    /// <summary>
    /// Tests that the DosFileControlBlock correctly handles file size.
    /// </summary>
    [Fact]
    public void DosFileControlBlock_FileSize_ReadsAndWritesCorrectly() {
        // Arrange
        DosFileControlBlock fcb = new(_memory, 0x1000);

        // Act
        fcb.FileSize = 12345;

        // Assert
        fcb.FileSize.Should().Be(12345);
    }

    /// <summary>
    /// Tests that the DosFileControlBlock correctly handles record operations.
    /// </summary>
    [Fact]
    public void DosFileControlBlock_NextRecord_AdvancesCorrectly() {
        // Arrange
        DosFileControlBlock fcb = new(_memory, 0x1000);
        fcb.CurrentBlock = 0;
        fcb.CurrentRecord = 127;

        // Act
        fcb.NextRecord();

        // Assert
        fcb.CurrentRecord.Should().Be(0);
        fcb.CurrentBlock.Should().Be(1);
    }

    /// <summary>
    /// Tests that the DosExtendedFileControlBlock correctly identifies extended FCBs.
    /// </summary>
    [Fact]
    public void DosExtendedFileControlBlock_IsExtendedFcb_ReturnsTrueWhenFlagIsSet() {
        // Arrange
        DosExtendedFileControlBlock xfcb = new(_memory, 0x1000);

        // Act
        xfcb.Flag = DosExtendedFileControlBlock.ExtendedFcbFlag;

        // Assert
        xfcb.IsExtendedFcb.Should().BeTrue();
    }

    /// <summary>
    /// Tests that the DosFcbManager correctly parses a simple filename.
    /// </summary>
    [Fact]
    public void DosFcbManager_ParseFilename_ParsesSimpleFilename() {
        // Arrange
        string cDrivePath = Path.GetTempPath();
        string executablePath = Path.Combine(cDrivePath, "test.exe");
        DosDriveManager driveManager = new(_loggerService, cDrivePath, executablePath);
        DosStringDecoder stringDecoder = new(_memory, null!);
        DosFileManager dosFileManager = new(_memory, stringDecoder, driveManager, _loggerService, 
            new List<IVirtualDevice>());
        
        DosFcbManager fcbManager = new(_memory, dosFileManager, driveManager, _loggerService);
        
        // Set up the filename string at address 0x1000
        string filename = "TEST.TXT\0";
        for (int i = 0; i < filename.Length; i++) {
            _memory.UInt8[0x1000 + (uint)i] = (byte)filename[i];
        }
        
        // Set up the FCB at address 0x2000
        for (int i = 0; i < DosFileControlBlock.StructureSize; i++) {
            _memory.UInt8[0x2000 + (uint)i] = (byte)' ';
        }
        _memory.UInt8[0x2000] = 0; // Drive = default

        // Act
        byte result = fcbManager.ParseFilename(0x1000, 0x2000, 0);

        // Assert
        result.Should().Be(DosFcbManager.FcbSuccess); // No wildcards
        
        DosFileControlBlock fcb = new(_memory, 0x2000);
        fcb.FileName.TrimEnd().Should().Be("TEST");
        fcb.FileExtension.TrimEnd().Should().Be("TXT");
    }

    /// <summary>
    /// Tests that the DosFcbManager correctly detects wildcards during parsing.
    /// </summary>
    [Fact]
    public void DosFcbManager_ParseFilename_DetectsWildcards() {
        // Arrange
        string cDrivePath = Path.GetTempPath();
        string executablePath = Path.Combine(cDrivePath, "test.exe");
        DosDriveManager driveManager = new(_loggerService, cDrivePath, executablePath);
        DosStringDecoder stringDecoder = new(_memory, null!);
        DosFileManager dosFileManager = new(_memory, stringDecoder, driveManager, _loggerService, 
            new List<IVirtualDevice>());
        
        DosFcbManager fcbManager = new(_memory, dosFileManager, driveManager, _loggerService);
        
        // Set up the filename string "*.TXT" at address 0x1000
        string filename = "*.TXT\0";
        for (int i = 0; i < filename.Length; i++) {
            _memory.UInt8[0x1000 + (uint)i] = (byte)filename[i];
        }
        
        // Set up the FCB at address 0x2000
        for (int i = 0; i < DosFileControlBlock.StructureSize; i++) {
            _memory.UInt8[0x2000 + (uint)i] = (byte)' ';
        }
        _memory.UInt8[0x2000] = 0; // Drive = default

        // Act
        byte result = fcbManager.ParseFilename(0x1000, 0x2000, 0);

        // Assert
        result.Should().Be(0x01); // Wildcards present
        
        DosFileControlBlock fcb = new(_memory, 0x2000);
        // Filename should be all '?' (wildcards expanded from *)
        fcb.FileName.Should().Be("????????");
    }

    /// <summary>
    /// Tests that the DosFcbManager correctly handles drive letters.
    /// </summary>
    [Fact]
    public void DosFcbManager_ParseFilename_ParsesDriveLetter() {
        // Arrange
        string cDrivePath = Path.GetTempPath();
        string executablePath = Path.Combine(cDrivePath, "test.exe");
        DosDriveManager driveManager = new(_loggerService, cDrivePath, executablePath);
        DosStringDecoder stringDecoder = new(_memory, null!);
        DosFileManager dosFileManager = new(_memory, stringDecoder, driveManager, _loggerService, 
            new List<IVirtualDevice>());
        
        DosFcbManager fcbManager = new(_memory, dosFileManager, driveManager, _loggerService);
        
        // Set up the filename string "C:TEST.TXT" at address 0x1000
        string filename = "C:TEST.TXT\0";
        for (int i = 0; i < filename.Length; i++) {
            _memory.UInt8[0x1000 + (uint)i] = (byte)filename[i];
        }
        
        // Set up the FCB at address 0x2000
        for (int i = 0; i < DosFileControlBlock.StructureSize; i++) {
            _memory.UInt8[0x2000 + (uint)i] = (byte)' ';
        }
        _memory.UInt8[0x2000] = 0; // Drive = default

        // Act
        byte result = fcbManager.ParseFilename(0x1000, 0x2000, 0);

        // Assert
        result.Should().Be(DosFcbManager.FcbSuccess); // No wildcards
        
        DosFileControlBlock fcb = new(_memory, 0x2000);
        fcb.DriveNumber.Should().Be(3); // C: = 3
        fcb.FileName.TrimEnd().Should().Be("TEST");
        fcb.FileExtension.TrimEnd().Should().Be("TXT");
    }

    /// <summary>
    /// Tests that FCB FindFirst returns success when a matching file exists.
    /// </summary>
    [Fact]
    public void DosFcbManager_FindFirst_ReturnsSuccessWhenFileExists() {
        // Arrange
        DosFcbManager fcbManager = CreateFcbManagerWithTestDirectory();
        File.WriteAllText(Path.Combine(_testDir!, "TEST.TXT"), "test content");
        
        const uint fcbAddress = 0x1000;
        const uint dtaAddress = 0x2000;
        
        DosFileControlBlock fcb = new(_memory, fcbAddress);
        fcb.DriveNumber = 3; // C:
        fcb.FileName = "TEST    ";
        fcb.FileExtension = "TXT";

        // Act
        byte result = fcbManager.FindFirst(fcbAddress, dtaAddress);

        // Assert
        result.Should().Be(DosFcbManager.FcbSuccess);
        
        // Check that DTA was filled with the file info
        // Drive number should be at DTA+0
        _memory.UInt8[dtaAddress].Should().Be(3);
    }

    /// <summary>
    /// Tests that FCB FindFirst returns error when no matching file exists.
    /// </summary>
    [Fact]
    public void DosFcbManager_FindFirst_ReturnsErrorWhenFileNotFound() {
        // Arrange
        DosFcbManager fcbManager = CreateFcbManagerWithTestDirectory();
        
        const uint fcbAddress = 0x1000;
        const uint dtaAddress = 0x2000;
        
        DosFileControlBlock fcb = new(_memory, fcbAddress);
        fcb.DriveNumber = 3; // C:
        fcb.FileName = "NOTFOUND";
        fcb.FileExtension = "TXT";

        // Act
        byte result = fcbManager.FindFirst(fcbAddress, dtaAddress);

        // Assert
        result.Should().Be(DosFcbManager.FcbError);
    }

    /// <summary>
    /// Tests that FCB FindFirst with wildcards finds the first matching file.
    /// </summary>
    [Fact]
    public void DosFcbManager_FindFirst_WithWildcards_FindsFirstMatch() {
        // Arrange
        DosFcbManager fcbManager = CreateFcbManagerWithTestDirectory();
        File.WriteAllText(Path.Combine(_testDir!, "FILE1.TXT"), "content1");
        File.WriteAllText(Path.Combine(_testDir!, "FILE2.TXT"), "content2");
        File.WriteAllText(Path.Combine(_testDir!, "OTHER.DAT"), "other");
        
        const uint fcbAddress = 0x1000;
        const uint dtaAddress = 0x2000;
        
        DosFileControlBlock fcb = new(_memory, fcbAddress);
        fcb.DriveNumber = 3; // C:
        fcb.FileName = "????????"; // All wildcards
        fcb.FileExtension = "TXT";

        // Act
        byte result = fcbManager.FindFirst(fcbAddress, dtaAddress);

        // Assert
        result.Should().Be(DosFcbManager.FcbSuccess);
    }

    /// <summary>
    /// Tests that FCB FindNext correctly finds subsequent matches after FindFirst.
    /// </summary>
    [Fact]
    public void DosFcbManager_FindNext_FindsSubsequentMatches() {
        // Arrange
        DosFcbManager fcbManager = CreateFcbManagerWithTestDirectory();
        File.WriteAllText(Path.Combine(_testDir!, "FILE1.TXT"), "content1");
        File.WriteAllText(Path.Combine(_testDir!, "FILE2.TXT"), "content2");
        
        const uint fcbAddress = 0x1000;
        const uint dtaAddress = 0x2000;
        
        DosFileControlBlock fcb = new(_memory, fcbAddress);
        fcb.DriveNumber = 3; // C:
        fcb.FileName = "????????"; // All wildcards
        fcb.FileExtension = "TXT";

        // Act - Find First
        byte result1 = fcbManager.FindFirst(fcbAddress, dtaAddress);
        result1.Should().Be(DosFcbManager.FcbSuccess);

        // Act - Find Next should find the second file
        byte result2 = fcbManager.FindNext(fcbAddress, dtaAddress);
        result2.Should().Be(DosFcbManager.FcbSuccess);

        // Act - Find Next should return error (no more files)
        byte result3 = fcbManager.FindNext(fcbAddress, dtaAddress);
        result3.Should().Be(DosFcbManager.FcbError);
    }

    /// <summary>
    /// Tests that FCB FindNext returns error when called without FindFirst.
    /// </summary>
    [Fact]
    public void DosFcbManager_FindNext_ReturnsErrorWithoutFindFirst() {
        // Arrange
        DosFcbManager fcbManager = CreateFcbManagerWithTestDirectory();
        
        const uint fcbAddress = 0x1000;
        const uint dtaAddress = 0x2000;
        
        DosFileControlBlock fcb = new(_memory, fcbAddress);
        fcb.DriveNumber = 3;
        fcb.FileName = "TEST    ";
        fcb.FileExtension = "TXT";

        // Act - Call FindNext without FindFirst
        byte result = fcbManager.FindNext(fcbAddress, dtaAddress);

        // Assert
        result.Should().Be(DosFcbManager.FcbError);
    }
}
