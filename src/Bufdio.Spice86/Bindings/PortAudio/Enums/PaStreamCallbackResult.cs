namespace Bufdio.Spice86.Bindings.PortAudio.Enums;

/// <summary>
/// Defines pa stream callback result values.
/// </summary>
internal enum PaStreamCallbackResult {
    paContinue = 0,
    paComplete = 1,
    paAbort = 2
}