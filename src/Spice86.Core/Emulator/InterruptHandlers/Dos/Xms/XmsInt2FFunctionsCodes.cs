namespace Spice86.Core.Emulator.InterruptHandlers.Dos.Xms;

/// <summary>
/// XmsInt2FFunctionsCodes enumeration.
/// </summary>
public enum XmsInt2FFunctionsCodes : byte {
    InstallationCheck = 0x00,
    GetCallbackAddress = 0x10
}