namespace Spice86.Tests.Dos;

using FluentAssertions;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Devices.Input.Keyboard;
using Spice86.Core.Emulator.IOPorts;
using Spice86.Shared.Interfaces;

using System.Runtime.CompilerServices;

using Xunit;

/// <summary>
/// Integration tests for DOS INT 21h keyboard input functionality.
/// Tests verify that DOS keyboard functions properly wait for input using the ASM stub approach.
/// </summary>
public class DosKeyboardIntegrationTests {
    private const int ResultPort = 0x999;
    private const int DetailsPort = 0x998;

    enum TestResult : byte {
        Success = 0x00,
        Failure = 0xFF
    }

    /// <summary>
    /// Tests INT 21h, AH=07h - Direct character input without echo.
    /// Verifies that the function waits for a keystroke and returns it in AL.
    /// </summary>
    [Fact]
    public void Int21H_DirectInputWithoutEcho_ShouldWaitForKeyAndReturnInAL() {
        // Program that reads one character using INT 21h, AH=07h
        // This should wait for a key (using the ASM stub that calls INT 16h)
        byte[] program = new byte[] {
            0xB4, 0x07,             // mov ah, 07h - Direct character input without echo
            0xCD, 0x21,             // int 21h - Returns: AL = character

            // Write result (AL) to result port
            0xBA, 0x99, 0x09,       // mov dx, ResultPort
            0xEE,                   // out dx, al
            0xF4                    // hlt
        };

        DosKeyboardTestHandler testHandler = RunDosKeyboardTest(program, setupKeys: (ps2kbd) => {
            // Simulate pressing and releasing the 'A' key
            ps2kbd.AddKey(KbdKey.A, isPressed: true);
            ps2kbd.AddKey(KbdKey.A, isPressed: false);
        });

        // The ASCII code for lowercase 'a' is 0x61
        testHandler.Results.Should().Contain(0x61,
            "INT 21h AH=07h should return ASCII code 0x61 for lowercase 'a'");
    }

    /// <summary>
    /// Tests INT 21h, AH=08h - Character input without echo (checks for Ctrl-C).
    /// Verifies that the function waits for a keystroke and returns it in AL.
    /// </summary>
    [Fact]
    public void Int21H_CharacterInputWithoutEcho_ShouldWaitForKeyAndReturnInAL() {
        // Program that reads one character using INT 21h, AH=08h
        byte[] program = new byte[] {
            0xB4, 0x08,             // mov ah, 08h - Character input without echo
            0xCD, 0x21,             // int 21h - Returns: AL = character

            // Write result (AL) to result port
            0xBA, 0x99, 0x09,       // mov dx, ResultPort
            0xEE,                   // out dx, al
            0xF4                    // hlt
        };

        DosKeyboardTestHandler testHandler = RunDosKeyboardTest(program, setupKeys: (ps2kbd) => {
            // Simulate pressing and releasing the 'B' key
            ps2kbd.AddKey(KbdKey.B, isPressed: true);
            ps2kbd.AddKey(KbdKey.B, isPressed: false);
        });

        // The ASCII code for lowercase 'b' is 0x62
        testHandler.Results.Should().Contain(0x62,
            "INT 21h AH=08h should return ASCII code 0x62 for lowercase 'b'");
    }

    /// <summary>
    /// Tests INT 21h, AH=01h - Read character from keyboard with echo.
    /// Verifies that the function waits for a keystroke, echoes it, and returns it in AL.
    /// </summary>
    [Fact]
    public void Int21H_ReadKeyboardWithEcho_ShouldWaitForKeyAndReturnInAL() {
        // Program that reads one character using INT 21h, AH=01h
        // This should wait for a key and echo it to the screen
        byte[] program = new byte[] {
            0xB4, 0x01,             // mov ah, 01h - Read character with echo
            0xCD, 0x21,             // int 21h - Returns: AL = character (also echoed)

            // Write result (AL) to result port
            0xBA, 0x99, 0x09,       // mov dx, ResultPort
            0xEE,                   // out dx, al
            0xF4                    // hlt
        };

        DosKeyboardTestHandler testHandler = RunDosKeyboardTest(program, setupKeys: (ps2kbd) => {
            // Simulate pressing and releasing the 'C' key
            ps2kbd.AddKey(KbdKey.C, isPressed: true);
            ps2kbd.AddKey(KbdKey.C, isPressed: false);
        });

        // The ASCII code for lowercase 'c' is 0x63
        testHandler.Results.Should().Contain(0x63,
            "INT 21h AH=01h should return ASCII code 0x63 for lowercase 'c'");
    }

    /// <summary>
    /// Tests that INT 21h, AH=07h correctly handles special keys like Enter.
    /// </summary>
    [Fact]
    public void Int21H_DirectInput_ShouldHandleEnterKey() {
        byte[] program = new byte[] {
            0xB4, 0x07,             // mov ah, 07h - Direct character input without echo
            0xCD, 0x21,             // int 21h

            // Write result to result port
            0xBA, 0x99, 0x09,       // mov dx, ResultPort
            0xEE,                   // out dx, al
            0xF4                    // hlt
        };

        DosKeyboardTestHandler testHandler = RunDosKeyboardTest(program, setupKeys: (ps2kbd) => {
            // Simulate pressing Enter
            ps2kbd.AddKey(KbdKey.Enter, isPressed: true);
            ps2kbd.AddKey(KbdKey.Enter, isPressed: false);
        });

        // Enter key should return 0x0D (carriage return)
        testHandler.Results.Should().Contain(0x0D,
            "INT 21h AH=07h should return 0x0D for Enter key");
    }

    /// <summary>
    /// Tests INT 21h, AH=07h with number keys.
    /// </summary>
    [Theory]
    [InlineData(KbdKey.D1, 0x31)] // '1'
    [InlineData(KbdKey.D5, 0x35)] // '5'
    [InlineData(KbdKey.D0, 0x30)] // '0'
    public void Int21H_DirectInput_ShouldHandleNumberKeys(KbdKey key, byte expectedAscii) {
        byte[] program = new byte[] {
            0xB4, 0x07,             // mov ah, 07h - Direct character input without echo
            0xCD, 0x21,             // int 21h

            // Write result to result port
            0xBA, 0x99, 0x09,       // mov dx, ResultPort
            0xEE,                   // out dx, al
            0xF4                    // hlt
        };

        DosKeyboardTestHandler testHandler = RunDosKeyboardTest(program, setupKeys: (ps2kbd) => {
            ps2kbd.AddKey(key, isPressed: true);
            ps2kbd.AddKey(key, isPressed: false);
        });

        testHandler.Results.Should().Contain(expectedAscii);
    }

    /// <summary>
    /// Runs DOS keyboard test program and returns handler with results
    /// </summary>
    private DosKeyboardTestHandler RunDosKeyboardTest(
        byte[] program,
        Action<PS2Keyboard> setupKeys,
        [CallerMemberName] string unitTestName = "test") {
        // Write program to temp file
        string filePath = Path.GetFullPath($"{unitTestName}.com");
        File.WriteAllBytes(filePath, program);

        // Setup emulator with PIT enabled (needed for keyboard timing)
        Spice86DependencyInjection spice86DependencyInjection = new Spice86Creator(
            binName: filePath,
            enableCfgCpu: false,
            enablePit: true,
            recordData: false,
            maxCycles: 1000000L,
            installInterruptVectors: true,
            enableA20Gate: false,
            enableXms: false
        ).Create();

        DosKeyboardTestHandler testHandler = new(
            spice86DependencyInjection.Machine.CpuState,
            NSubstitute.Substitute.For<ILoggerService>(),
            spice86DependencyInjection.Machine.IoPortDispatcher
        );

        // Setup keyboard events before running
        PS2Keyboard ps2kbd = spice86DependencyInjection.Machine.KeyboardController.KeyboardDevice;
        setupKeys(ps2kbd);

        spice86DependencyInjection.ProgramExecutor.Run();

        return testHandler;
    }

    /// <summary>
    /// Captures DOS keyboard test results from designated I/O ports
    /// </summary>
    private class DosKeyboardTestHandler : DefaultIOPortHandler {
        public List<byte> Results { get; } = new();
        public List<byte> Details { get; } = new();

        public DosKeyboardTestHandler(State state, ILoggerService loggerService,
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
