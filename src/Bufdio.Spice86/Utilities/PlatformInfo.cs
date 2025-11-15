namespace Bufdio.Spice86.Utilities;

using System.Runtime.InteropServices;

/// <summary>
/// Represents platform info.
/// </summary>
internal static class PlatformInfo {
    /// <summary>
    /// The is windows.
    /// </summary>
    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    /// <summary>
    /// The is linux.
    /// </summary>
    public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    /// <summary>
    /// The isosx.
    /// </summary>
    public static bool IsOSX => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
}