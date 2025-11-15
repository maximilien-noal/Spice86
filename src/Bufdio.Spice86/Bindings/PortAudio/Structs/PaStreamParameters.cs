namespace Bufdio.Spice86.Bindings.PortAudio.Structs;

using Bufdio.Spice86.Bindings.PortAudio.Enums;

using System;
using System.Runtime.InteropServices;

/// <summary>
/// Represents pa stream parameters.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal readonly record struct PaStreamParameters {
    /// <summary>
    /// Gets device.
    /// </summary>
    public readonly int Device { get; init; }
    /// <summary>
    /// Gets channel count.
    /// </summary>
    public readonly int ChannelCount { get; init; }
    /// <summary>
    /// Gets sample format.
    /// </summary>
    public readonly PaSampleFormat SampleFormat { get; init; }
    /// <summary>
    /// Gets suggested latency.
    /// </summary>
    public readonly double SuggestedLatency { get; init; }
    /// <summary>
    /// Gets host api specific stream info.
    /// </summary>
    public readonly IntPtr HostApiSpecificStreamInfo { get; init; }
}