namespace Spice86.Core.Emulator.OperatingSystem.Enums;

/// <summary>
/// Defines the load mode for DOS INT 21h function 4Bh (Load and/or Execute).
/// This enum represents the AL register value when calling the function.
/// </summary>
public enum DosExecLoadMode : byte {
    /// <summary>
    /// Load and execute program (AL=0x00).
    /// Creates a new PSP and executes the program.
    /// </summary>
    LoadAndExecute = 0x00,

    /// <summary>
    /// Load program as overlay (AL=0x01).
    /// No PSP is created, program is loaded at specified segment.
    /// </summary>
    LoadAsOverlay = 0x01,

    /// <summary>
    /// Load program but don't execute (AL=0x03).
    /// Creates PSP but doesn't transfer control to the program.
    /// </summary>
    LoadDontExecute = 0x03,

    /// <summary>
    /// Load and execute in background (AL=0x04).
    /// DOS 5.0+ only, used for task switching.
    /// </summary>
    LoadAndExecuteInBackground = 0x04,

    /// <summary>
    /// Prepare program for execution (AL=0x05).
    /// DOS 5.0+ only, part of EXEC state preparation.
    /// </summary>
    PrepareForExecution = 0x05
}
