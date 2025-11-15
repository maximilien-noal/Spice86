namespace Bufdio.Spice86.Utilities.Extensions;

using Bufdio.Spice86.Bindings.PortAudio;
using Bufdio.Spice86.Bindings.PortAudio.Structs;
using Bufdio.Spice86.Exceptions;

using System.Runtime.InteropServices;

/// <summary>
/// Represents port audio extensions.
/// </summary>
internal static class PortAudioExtensions {
    /// <summary>
    /// Performs the pa is error operation.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <returns>A boolean value indicating the result.</returns>
    public static bool PaIsError(this int code) {
        return code < 0;
    }

    /// <summary>
    /// Performs the pa guard operation.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <returns>The result of the operation.</returns>
    public static int PaGuard(this int code) {
        if (!code.PaIsError()) {
            return code;
        }

        throw new PortAudioException(code);
    }

    /// <summary>
    /// Performs the pa error to text operation.
    /// </summary>
    /// <param name="code">The code.</param>
    public static string? PaErrorToText(this int code) {
        nint ptr = NativeMethods.PortAudioGetErrorText(code);
        return Marshal.PtrToStringAnsi(ptr);
    }

    /// <summary>
    /// Performs the pa get pa device info operation.
    /// </summary>
    /// <param name="device">The device.</param>
    /// <returns>The result of the operation.</returns>
    public static PaDeviceInfo PaGetPaDeviceInfo(this int device) {
        nint ptr = NativeMethods.PortAudioGetDeviceInfo(device);
        return Marshal.PtrToStructure<PaDeviceInfo>(ptr);
    }

    /// <summary>
    /// Performs the pa to audio device operation.
    /// </summary>
    /// <param name="device">The device.</param>
    /// <param name="deviceIndex">The device index.</param>
    /// <returns>The result of the operation.</returns>
    public static AudioDevice PaToAudioDevice(this PaDeviceInfo device, int deviceIndex) {
        return new AudioDevice(
            deviceIndex,
            device.name,
            device.maxOutputChannels,
            device.defaultLowOutputLatency,
            device.defaultHighOutputLatency,
            (int)device.defaultSampleRate);
    }
}