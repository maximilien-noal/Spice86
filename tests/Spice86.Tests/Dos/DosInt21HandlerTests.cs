namespace Spice86.Tests.Dos;

using FluentAssertions;

using NSubstitute;

using Spice86.Core.CLI;
using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Function;
using Spice86.Core.Emulator.InterruptHandlers.Bios.Structures;
using Spice86.Core.Emulator.InterruptHandlers.Dos;
using Spice86.Core.Emulator.InterruptHandlers.Input.Keyboard;
using Spice86.Core.Emulator.IOPorts;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.OperatingSystem;
using Spice86.Core.Emulator.OperatingSystem.Devices;
using Spice86.Core.Emulator.OperatingSystem.Enums;
using Spice86.Core.Emulator.OperatingSystem.Structures;
using Spice86.Core.Emulator.VM.Breakpoint;
using Spice86.Shared.Interfaces;

using Xunit;

public class DosInt21HandlerTests {
    [Fact]
    public void MoveFilePointerUsingHandle_ShouldTreatCxDxOffsetAsSignedValue() {
        // Arrange
        IMemory memory = Substitute.For<IMemory>();
        State state = new(CpuModel.INTEL_80286);
        Stack stack = new(memory, state);
        ILoggerService logger = Substitute.For<ILoggerService>();
        IFunctionHandlerProvider functionHandlerProvider = Substitute.For<IFunctionHandlerProvider>();
        string cDrivePath = Path.GetTempPath();
        string executablePath = Path.Combine(cDrivePath, "test.exe");
        DosDriveManager driveManager = new(logger, cDrivePath, executablePath);
        DosStringDecoder stringDecoder = new(memory, state);
        IList<IVirtualDevice> virtualDevices = new List<IVirtualDevice>();
        DosFileManager dosFileManager = new(memory, stringDecoder, driveManager, logger, virtualDevices);
        RecordingVirtualFile recordingFile = new();
        const ushort fileHandle = 0x0003;
        dosFileManager.OpenFiles[fileHandle] = recordingFile;

        // Create minimal real instances for unused dependencies
        Configuration configuration = new();
        DosProgramSegmentPrefixTracker dosPspTracker = new(configuration, memory, logger);
        DosMemoryManager dosMemoryManager = new(memory, dosPspTracker, logger);
        BiosDataArea biosDataArea = new(memory, 640);  // 640KB conventional memory
        BiosKeyboardBuffer biosKeyboardBuffer = new(memory, biosDataArea);
        KeyboardInt16Handler keyboardHandler = new(memory, biosDataArea, functionHandlerProvider, stack, state, logger, biosKeyboardBuffer);
        CountryInfo countryInfo = new();
        DosTables dosTables = new();
        DosProcessManager dosProcessManager = new(memory, state, dosPspTracker, dosMemoryManager, 
            dosFileManager, driveManager, new Dictionary<string, string>(), logger);
        AddressReadWriteBreakpoints ioBreakpoints = new();
        IOPortDispatcher ioPortDispatcher = new(ioBreakpoints, state, logger, false);

        DosInt21Handler handler = new(
            memory,
            dosPspTracker,
            functionHandlerProvider,
            stack,
            state,
            keyboardHandler,
            countryInfo,
            stringDecoder,
            dosMemoryManager,
            dosFileManager,
            driveManager,
            dosProcessManager,
            ioPortDispatcher,
            dosTables,
            logger);

        state.AL = (byte)SeekOrigin.Current;
        state.BX = fileHandle;
        state.CX = 0xFFFF;
        state.DX = 0xFFFF;

        // Act
        handler.MoveFilePointerUsingHandle(false);

        // Assert
        recordingFile.LastSeekOffset.Should().Be(-1);
        recordingFile.LastSeekOrigin.Should().Be(SeekOrigin.Current);
    }

    private sealed class RecordingVirtualFile : VirtualFileBase {
        private long _length;

        public long LastSeekOffset { get; private set; }

        public SeekOrigin LastSeekOrigin { get; private set; }

        public override bool CanRead => false;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => _length;

        public override long Position { get; set; }

        public override void Flush() {
        }

        public override int Read(byte[] buffer, int offset, int count) {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            LastSeekOffset = offset;
            LastSeekOrigin = origin;
            return origin switch {
                SeekOrigin.Begin => Position = offset,
                SeekOrigin.Current => Position += offset,
                SeekOrigin.End => Position = _length + offset,
                _ => Position
            };
        }

        public override void SetLength(long value) {
            _length = value;
        }

        public override void Write(byte[] buffer, int offset, int count) {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Creates a DosInt21Handler instance with minimal dependencies for testing.
    /// </summary>
    private static (DosInt21Handler handler, State state, DosMemoryManager dosMemoryManager) CreateHandler() {
        IMemory memory = Substitute.For<IMemory>();
        State state = new(CpuModel.INTEL_80286);
        Stack stack = new(memory, state);
        ILoggerService logger = Substitute.For<ILoggerService>();
        IFunctionHandlerProvider functionHandlerProvider = Substitute.For<IFunctionHandlerProvider>();
        string cDrivePath = Path.GetTempPath();
        string executablePath = Path.Combine(cDrivePath, "test.exe");
        DosDriveManager driveManager = new(logger, cDrivePath, executablePath);
        DosStringDecoder stringDecoder = new(memory, state);
        IList<IVirtualDevice> virtualDevices = new List<IVirtualDevice>();
        DosFileManager dosFileManager = new(memory, stringDecoder, driveManager, logger, virtualDevices);

        Configuration configuration = new();
        DosProgramSegmentPrefixTracker dosPspTracker = new(configuration, memory, logger);
        DosMemoryManager dosMemoryManager = new(memory, dosPspTracker, logger);
        BiosDataArea biosDataArea = new(memory, 640);
        BiosKeyboardBuffer biosKeyboardBuffer = new(memory, biosDataArea);
        KeyboardInt16Handler keyboardHandler = new(memory, biosDataArea, functionHandlerProvider, stack, state, logger, biosKeyboardBuffer);
        CountryInfo countryInfo = new();
        DosTables dosTables = new();
        DosProcessManager dosProcessManager = new(memory, state, dosPspTracker, dosMemoryManager,
            dosFileManager, driveManager, new Dictionary<string, string>(), logger);
        AddressReadWriteBreakpoints ioBreakpoints = new();
        IOPortDispatcher ioPortDispatcher = new(ioBreakpoints, state, logger, false);

        DosInt21Handler handler = new(
            memory,
            dosPspTracker,
            functionHandlerProvider,
            stack,
            state,
            keyboardHandler,
            countryInfo,
            stringDecoder,
            dosMemoryManager,
            dosFileManager,
            driveManager,
            dosProcessManager,
            ioPortDispatcher,
            dosTables,
            logger);

        return (handler, state, dosMemoryManager);
    }

    /// <summary>
    /// Tests that subfunction 0x00 returns the current allocation strategy.
    /// </summary>
    [Fact]
    public void GetSetMemoryAllocationStrategy_Subfunction00_ReturnsCurrentStrategy() {
        // Arrange
        (DosInt21Handler handler, State state, DosMemoryManager dosMemoryManager) = CreateHandler();
        dosMemoryManager.AllocationStrategy = DosMemoryAllocationStrategy.BestFit;
        state.AL = 0x00; // Get allocation strategy subfunction

        // Act
        handler.GetSetMemoryAllocationStrategy(false);

        // Assert
        state.AX.Should().Be((ushort)DosMemoryAllocationStrategy.BestFit);
    }

    /// <summary>
    /// Tests that subfunction 0x01 correctly sets a valid strategy.
    /// </summary>
    [Fact]
    public void GetSetMemoryAllocationStrategy_Subfunction01_SetsValidStrategy() {
        // Arrange
        (DosInt21Handler handler, State state, DosMemoryManager dosMemoryManager) = CreateHandler();
        state.AL = 0x01; // Set allocation strategy subfunction
        state.BX = (ushort)DosMemoryAllocationStrategy.LastFit;

        // Act
        handler.GetSetMemoryAllocationStrategy(false);

        // Assert
        dosMemoryManager.AllocationStrategy.Should().Be(DosMemoryAllocationStrategy.LastFit);
    }

    /// <summary>
    /// Tests that subfunction 0x01 rejects invalid fit types (> 0x02).
    /// </summary>
    [Fact]
    public void GetSetMemoryAllocationStrategy_Subfunction01_RejectsInvalidFitType() {
        // Arrange
        (DosInt21Handler handler, State state, DosMemoryManager dosMemoryManager) = CreateHandler();
        DosMemoryAllocationStrategy originalStrategy = dosMemoryManager.AllocationStrategy;
        state.AL = 0x01; // Set allocation strategy subfunction
        state.BX = 0x03; // Invalid fit type (> 0x02)

        // Act
        handler.GetSetMemoryAllocationStrategy(false);

        // Assert - carry flag should be set and strategy unchanged
        state.CarryFlag.Should().BeTrue();
        state.AX.Should().Be((ushort)DosErrorCode.FunctionNumberInvalid);
        dosMemoryManager.AllocationStrategy.Should().Be(originalStrategy);
    }

    /// <summary>
    /// Tests that subfunction 0x01 rejects strategies with bits 2-5 set.
    /// </summary>
    [Fact]
    public void GetSetMemoryAllocationStrategy_Subfunction01_RejectsBits2To5Set() {
        // Arrange
        (DosInt21Handler handler, State state, DosMemoryManager dosMemoryManager) = CreateHandler();
        DosMemoryAllocationStrategy originalStrategy = dosMemoryManager.AllocationStrategy;
        state.AL = 0x01; // Set allocation strategy subfunction
        state.BX = 0x04; // Bit 2 set (invalid)

        // Act
        handler.GetSetMemoryAllocationStrategy(false);

        // Assert - carry flag should be set and strategy unchanged
        state.CarryFlag.Should().BeTrue();
        state.AX.Should().Be((ushort)DosErrorCode.FunctionNumberInvalid);
        dosMemoryManager.AllocationStrategy.Should().Be(originalStrategy);
    }

    /// <summary>
    /// Tests that subfunction 0x01 rejects invalid high memory bits.
    /// </summary>
    [Fact]
    public void GetSetMemoryAllocationStrategy_Subfunction01_RejectsInvalidHighMemBits() {
        // Arrange
        (DosInt21Handler handler, State state, DosMemoryManager dosMemoryManager) = CreateHandler();
        DosMemoryAllocationStrategy originalStrategy = dosMemoryManager.AllocationStrategy;
        state.AL = 0x01; // Set allocation strategy subfunction
        state.BX = 0xC0; // Both bits 6 and 7 set (invalid - only 0x00, 0x40, or 0x80 are valid)

        // Act
        handler.GetSetMemoryAllocationStrategy(false);

        // Assert - carry flag should be set and strategy unchanged
        state.CarryFlag.Should().BeTrue();
        state.AX.Should().Be((ushort)DosErrorCode.FunctionNumberInvalid);
        dosMemoryManager.AllocationStrategy.Should().Be(originalStrategy);
    }

    /// <summary>
    /// Tests that UMB subfunctions (0x02, 0x03) return error since UMBs are not implemented.
    /// </summary>
    [Theory]
    [InlineData(0x02)] // Get UMB link state
    [InlineData(0x03)] // Set UMB link state
    public void GetSetMemoryAllocationStrategy_UmbSubfunctions_ReturnError(byte subfunction) {
        // Arrange
        (DosInt21Handler handler, State state, DosMemoryManager _) = CreateHandler();
        state.AL = subfunction;
        state.BX = 0; // Valid value for set UMB link state

        // Act
        handler.GetSetMemoryAllocationStrategy(false);

        // Assert - carry flag should be set and error returned
        state.CarryFlag.Should().BeTrue();
        state.AX.Should().Be((ushort)DosErrorCode.FunctionNumberInvalid);
    }

    /// <summary>
    /// Tests that invalid subfunctions set carry flag and return FunctionNumberInvalid error.
    /// </summary>
    [Theory]
    [InlineData(0x04)]
    [InlineData(0xFF)]
    public void GetSetMemoryAllocationStrategy_InvalidSubfunction_SetsCarryAndReturnsError(byte subfunction) {
        // Arrange
        (DosInt21Handler handler, State state, DosMemoryManager _) = CreateHandler();
        state.AL = subfunction;

        // Act
        handler.GetSetMemoryAllocationStrategy(false);

        // Assert
        state.CarryFlag.Should().BeTrue();
        state.AX.Should().Be((ushort)DosErrorCode.FunctionNumberInvalid);
    }
}