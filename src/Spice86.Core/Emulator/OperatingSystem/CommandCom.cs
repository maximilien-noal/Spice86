namespace Spice86.Core.Emulator.OperatingSystem;

using Serilog.Events;

using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.OperatingSystem.Structures;
using Spice86.Shared.Interfaces;
using Spice86.Shared.Utils;

/// <summary>
/// Simulates COMMAND.COM - the DOS command interpreter.
/// This is the root of the PSP (Program Segment Prefix) chain.
/// All DOS programs launched by Spice86 have COMMAND.COM as their ancestor.
/// </summary>
/// <remarks>
/// In real DOS, COMMAND.COM is loaded by the kernel and becomes the parent
/// of all user-launched programs. The PSP chain allows programs to trace
/// back to their parent processes. COMMAND.COM's PSP points to itself as
/// its own parent (marking it as the root).
/// <para>
/// This implementation is non-interactive. We don't support an interactive
/// shell since Spice86 is focused on reverse engineering specific DOS programs.
/// </para>
/// </remarks>
public class CommandCom {
    /// <summary>
    /// The segment where COMMAND.COM's PSP is located.
    /// This is positioned just before the initial program entry point segment.
    /// </summary>
    /// <remarks>
    /// COMMAND.COM occupies a small memory area. Its PSP starts at segment 0x60
    /// (after DOS internal structures) and takes minimal space since we don't
    /// load actual COMMAND.COM code - just simulate its PSP for the chain.
    /// </remarks>
    public const ushort CommandComSegment = 0x60;

    private readonly ILoggerService _loggerService;

    /// <summary>
    /// Gets the PSP (Program Segment Prefix) for COMMAND.COM.
    /// </summary>
    public DosProgramSegmentPrefix Psp { get; }

    /// <summary>
    /// Gets the segment address of COMMAND.COM's PSP.
    /// </summary>
    public ushort PspSegment => CommandComSegment;

    /// <summary>
    /// Initializes a new instance of COMMAND.COM simulation.
    /// Creates the PSP structure in memory at the designated segment.
    /// </summary>
    /// <param name="memory">The emulator memory.</param>
    /// <param name="loggerService">The logger service.</param>
    public CommandCom(IMemory memory, ILoggerService loggerService) {
        _loggerService = loggerService;

        uint pspAddress = MemoryUtils.ToPhysicalAddress(CommandComSegment, 0);
        Psp = new DosProgramSegmentPrefix(memory, pspAddress);
        InitializePsp();

        if (_loggerService.IsEnabled(LogEventLevel.Information)) {
            _loggerService.Information(
                "COMMAND.COM PSP initialized at segment {Segment:X4}",
                CommandComSegment);
        }
    }

    /// <summary>
    /// Initializes the COMMAND.COM PSP structure.
    /// </summary>
    private void InitializePsp() {
        // Set up INT 20h exit point (CD 20 = INT 20h)
        Psp.Exit[0] = 0xCD;
        Psp.Exit[1] = 0x20;

        // COMMAND.COM is its own parent (marks it as the root of the PSP chain)
        Psp.ParentProgramSegmentPrefix = CommandComSegment;

        // Set the next segment (end of program image)
        // Since we're simulating, just point past our minimal PSP
        Psp.NextSegment = (ushort)(CommandComSegment + 0x10);

        // Empty command tail for COMMAND.COM itself
        Psp.DosCommandTail.Length = 0;

        // DOS version compatibility (5.0)
        Psp.DosVersionMajor = 5;
        Psp.DosVersionMinor = 0;

        // Maximum open files (20 is standard)
        Psp.MaximumOpenFiles = 20;

        // Initialize the file handle table to all 0xFF (unused)
        for (int i = 0; i < 20; i++) {
            Psp.Files[i] = 0xFF;
        }

        // Set up standard handles (0=stdin, 1=stdout, 2=stderr, 3=stdaux, 4=stdprn)
        // These map to device handles 0-4
        Psp.Files[0] = 0; // STDIN
        Psp.Files[1] = 1; // STDOUT
        Psp.Files[2] = 2; // STDERR
        Psp.Files[3] = 3; // STDAUX
        Psp.Files[4] = 4; // STDPRN
    }
}
