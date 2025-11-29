namespace Spice86.Tests.Bios;

using FluentAssertions;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.IOPorts;
using Spice86.Shared.Interfaces;

using System.Runtime.CompilerServices;

using Xunit;

/// <summary>
/// Integration tests for BIOS INT 15h OS HOOK functions (AH=90h and AH=91h).
/// Tests run inline x86 machine code through the emulation stack.
/// </summary>
public class OsHookIntegrationTests {
    private const int ResultPort = 0x999;
    private const int DetailsPort = 0x998;

    private enum TestResult : byte {
        Success = 0x00,
        Failure = 0xFF
    }

    /// <summary>
    /// Tests BIOS INT 15h function 90h - OS HOOK - DEVICE BUSY
    /// </summary>
    [Fact]
    public void Int15h_DeviceBusy_ShouldClearCarryAndSetAhToZero() {
        // Test INT 15h, AH=90h - Device Busy
        // Should clear CF and set AH=0
        byte[] program = new byte[]
        {
            0xB4, 0x90,             // mov ah, 90h - Device Busy
            0xB0, 0x00,             // mov al, 00h - device type (disk)
            0xCD, 0x15,             // int 15h
            // Check if carry flag is clear (CF=0)
            0x72, 0x0E,             // jc failed (if carry set, fail)
            // Check if AH is 0
            0x80, 0xFC, 0x00,       // cmp ah, 0
            0x75, 0x09,             // jne failed (if AH != 0, fail)
            // success:
            0xB0, 0x00,             // mov al, TestResult.Success
            0xBA, 0x99, 0x09,       // mov dx, ResultPort
            0xEE,                   // out dx, al
            0xF4,                   // hlt
            // failed:
            0xB0, 0xFF,             // mov al, TestResult.Failure
            0xBA, 0x99, 0x09,       // mov dx, ResultPort
            0xEE,                   // out dx, al
            0xF4                    // hlt
        };

        OsHookTestHandler testHandler = RunOsHookTest(program);

        testHandler.Results.Should().Contain((byte)TestResult.Success,
            "INT 15h function 90h should clear carry flag and set AH=0");
    }

    /// <summary>
    /// Tests BIOS INT 15h function 91h - OS HOOK - DEVICE POST
    /// </summary>
    [Fact]
    public void Int15h_DevicePost_ShouldClearCarryAndSetAhToZero() {
        // Test INT 15h, AH=91h - Device Post
        // Should clear CF and set AH=0
        byte[] program = new byte[]
        {
            0xB4, 0x91,             // mov ah, 91h - Device Post
            0xB0, 0x00,             // mov al, 00h - device type (disk)
            0xCD, 0x15,             // int 15h
            // Check if carry flag is clear (CF=0)
            0x72, 0x0E,             // jc failed (if carry set, fail)
            // Check if AH is 0
            0x80, 0xFC, 0x00,       // cmp ah, 0
            0x75, 0x09,             // jne failed (if AH != 0, fail)
            // success:
            0xB0, 0x00,             // mov al, TestResult.Success
            0xBA, 0x99, 0x09,       // mov dx, ResultPort
            0xEE,                   // out dx, al
            0xF4,                   // hlt
            // failed:
            0xB0, 0xFF,             // mov al, TestResult.Failure
            0xBA, 0x99, 0x09,       // mov dx, ResultPort
            0xEE,                   // out dx, al
            0xF4                    // hlt
        };

        OsHookTestHandler testHandler = RunOsHookTest(program);

        testHandler.Results.Should().Contain((byte)TestResult.Success,
            "INT 15h function 91h should clear carry flag and set AH=0");
    }

    /// <summary>
    /// Tests BIOS INT 15h function 90h with different device types
    /// </summary>
    [Theory]
    [InlineData(0x00)]  // Disk
    [InlineData(0x01)]  // Diskette
    [InlineData(0x02)]  // Keyboard
    [InlineData(0x80)]  // Reentrant type
    public void Int15h_DeviceBusy_ShouldWorkWithDifferentDeviceTypes(byte deviceType) {
        // Test INT 15h, AH=90h with different device types in AL
        byte[] program = new byte[]
        {
            0xB4, 0x90,             // mov ah, 90h - Device Busy
            0xB0, deviceType,       // mov al, deviceType
            0xCD, 0x15,             // int 15h
            // Check if carry flag is clear (CF=0)
            0x72, 0x0E,             // jc failed
            // Check if AH is 0
            0x80, 0xFC, 0x00,       // cmp ah, 0
            0x75, 0x09,             // jne failed
            // success:
            0xB0, 0x00,             // mov al, TestResult.Success
            0xBA, 0x99, 0x09,       // mov dx, ResultPort
            0xEE,                   // out dx, al
            0xF4,                   // hlt
            // failed:
            0xB0, 0xFF,             // mov al, TestResult.Failure
            0xBA, 0x99, 0x09,       // mov dx, ResultPort
            0xEE,                   // out dx, al
            0xF4                    // hlt
        };

        OsHookTestHandler testHandler = RunOsHookTest(program);

        testHandler.Results.Should().Contain((byte)TestResult.Success,
            $"INT 15h function 90h should work with device type 0x{deviceType:X2}");
    }

    /// <summary>
    /// Runs OS Hook test program and returns handler with results
    /// </summary>
    private OsHookTestHandler RunOsHookTest(byte[] program, [CallerMemberName] string unitTestName = "test") {
        // Write program to temp file
        string filePath = Path.Combine(Path.GetTempPath(), $"{unitTestName}_{Guid.NewGuid()}.com");
        File.WriteAllBytes(filePath, program);

        try {
            // Setup emulator
            Spice86DependencyInjection spice86DependencyInjection = new Spice86Creator(
                binName: filePath,
                enableCfgCpu: false,
                enablePit: true,
                recordData: false,
                maxCycles: 100000L,
                installInterruptVectors: true,
                enableA20Gate: false,
                enableXms: false
            ).Create();

            OsHookTestHandler testHandler = new(
                spice86DependencyInjection.Machine.CpuState,
                NSubstitute.Substitute.For<ILoggerService>(),
                spice86DependencyInjection.Machine.IoPortDispatcher
            );

            spice86DependencyInjection.ProgramExecutor.Run();

            return testHandler;
        } finally {
            // Clean up the temp file
            if (File.Exists(filePath)) {
                try {
                    File.Delete(filePath);
                } catch (IOException) {
                    // ignore file in use
                } catch (UnauthorizedAccessException) {
                    // ignore permission issues
                }
            }
        }
    }

    /// <summary>
    /// Captures OS Hook test results from designated I/O ports
    /// </summary>
    private class OsHookTestHandler : DefaultIOPortHandler {
        public List<byte> Results { get; } = new();
        public List<byte> Details { get; } = new();

        public OsHookTestHandler(State state, ILoggerService loggerService,
            IOPortDispatcher ioPortDispatcher) : base(state, true, loggerService) {
            ioPortDispatcher.AddIOPortHandler(ResultPort, this);
            ioPortDispatcher.AddIOPortHandler(DetailsPort, this);
        }

        public override void WriteByte(ushort port, byte value) {
            if (port == ResultPort) {
                Results.Add(value);
            } else if (port == DetailsPort) {
                Details.Add(value);
            }
        }
    }
}
