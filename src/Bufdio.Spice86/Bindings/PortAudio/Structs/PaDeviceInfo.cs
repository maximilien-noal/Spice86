namespace Bufdio.Spice86.Bindings.PortAudio.Structs;

using System.Runtime.InteropServices;

/// <summary>
/// Represents pa device info.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal readonly record struct PaDeviceInfo {
    /// <summary>
    /// The struct version.
    /// </summary>
    public readonly int structVersion;

    /// <summary>
    /// The name.
    /// </summary>
    [MarshalAs(UnmanagedType.LPStr)]
    public readonly string name;

    /// <summary>
    /// The host api.
    /// </summary>
    public readonly int hostApi;
    /// <summary>
    /// The max input channels.
    /// </summary>
    public readonly int maxInputChannels;
    /// <summary>
    /// The max output channels.
    /// </summary>
    public readonly int maxOutputChannels;
    /// <summary>
    /// The default low input latency.
    /// </summary>
    public readonly double defaultLowInputLatency;
    /// <summary>
    /// The default low output latency.
    /// </summary>
    public readonly double defaultLowOutputLatency;
    /// <summary>
    /// The default high input latency.
    /// </summary>
    public readonly double defaultHighInputLatency;
    /// <summary>
    /// The default high output latency.
    /// </summary>
    public readonly double defaultHighOutputLatency;
    /// <summary>
    /// The default sample rate.
    /// </summary>
    public readonly double defaultSampleRate;
}