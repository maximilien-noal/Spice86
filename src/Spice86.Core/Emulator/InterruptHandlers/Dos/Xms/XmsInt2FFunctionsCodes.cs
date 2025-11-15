namespace Spice86.Core.Emulator.InterruptHandlers.Dos.Xms;

/// <summary>
/// Defines xms int 2 f functions codes values.
/// </summary>
public enum XmsInt2FFunctionsCodes : byte {
    InstallationCheck = 0x00,
    GetCallbackAddress = 0x10
}