namespace Spice86.Core.Emulator.Devices.Video.Registers.CrtController;

/// <summary>
/// Represents the 8 bit Overflow register, which is used to store additional bits that wouldn't fit
/// into other CRT Controller registers.
/// </summary>
public class OverflowRegister : Register8 {
    /// <summary>
    /// VerticalTotal89 method.
    /// </summary>
    public int VerticalTotal89 => (GetBit(0) ? 1 << 8 : 0) | (GetBit(5) ? 1 << 9 : 0);
    /// <summary>
    /// VerticalDisplayEnd89 method.
    /// </summary>
    public int VerticalDisplayEnd89 => (GetBit(1) ? 1 << 8 : 0) | (GetBit(6) ? 1 << 9 : 0);
    /// <summary>
    /// VerticalSyncStart89 method.
    /// </summary>
    public int VerticalSyncStart89 => (GetBit(2) ? 1 << 8 : 0) | (GetBit(7) ? 1 << 9 : 0);
    /// <summary>
    /// VerticalBlankingStart8 method.
    /// </summary>
    public int VerticalBlankingStart8 => GetBit(3) ? 1 << 8 : 0;
    /// <summary>
    /// LineCompare8 method.
    /// </summary>
    public int LineCompare8 => GetBit(4) ? 1 << 8 : 0;
}