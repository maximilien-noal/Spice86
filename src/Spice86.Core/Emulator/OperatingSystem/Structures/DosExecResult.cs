namespace Spice86.Core.Emulator.OperatingSystem.Structures;

using Spice86.Core.Emulator.OperatingSystem.Enums;

/// <summary>
/// Represents the result of a DOS EXEC (INT 21h, AH=4Bh) operation.
/// </summary>
/// <remarks>
/// Based on MS-DOS 4.0 EXEC.ASM error handling.
/// </remarks>
public readonly struct DosExecResult {
    /// <summary>
    /// Whether the EXEC operation succeeded.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// The DOS error code if the operation failed.
    /// </summary>
    public DosErrorCode ErrorCode { get; }

    /// <summary>
    /// The PSP segment of the loaded program (for LoadOnly mode).
    /// </summary>
    public ushort ChildPspSegment { get; }

    /// <summary>
    /// The initial CS value (for LoadOnly mode).
    /// </summary>
    public ushort InitialCS { get; }

    /// <summary>
    /// The initial IP value (for LoadOnly mode).
    /// </summary>
    public ushort InitialIP { get; }

    /// <summary>
    /// The initial SS value (for LoadOnly mode).
    /// </summary>
    public ushort InitialSS { get; }

    /// <summary>
    /// The initial SP value (for LoadOnly mode).
    /// </summary>
    public ushort InitialSP { get; }

    /// <summary>
    /// Creates a successful EXEC result.
    /// </summary>
    public static DosExecResult Succeeded() => new(
        success: true,
        errorCode: DosErrorCode.NoError,
        childPspSegment: 0,
        cs: 0, ip: 0, ss: 0, sp: 0);

    /// <summary>
    /// Creates a successful EXEC result with child process information (for LoadOnly).
    /// </summary>
    public static DosExecResult Succeeded(ushort childPspSegment, ushort cs, ushort ip, ushort ss, ushort sp) =>
        new(success: true, errorCode: DosErrorCode.NoError, childPspSegment, cs, ip, ss, sp);

    /// <summary>
    /// Creates a failed EXEC result.
    /// </summary>
    public static DosExecResult Failed(DosErrorCode errorCode) => new(
        success: false,
        errorCode: errorCode,
        childPspSegment: 0,
        cs: 0, ip: 0, ss: 0, sp: 0);

    private DosExecResult(bool success, DosErrorCode errorCode, ushort childPspSegment, 
        ushort cs, ushort ip, ushort ss, ushort sp) {
        Success = success;
        ErrorCode = errorCode;
        ChildPspSegment = childPspSegment;
        InitialCS = cs;
        InitialIP = ip;
        InitialSS = ss;
        InitialSP = sp;
    }
}
