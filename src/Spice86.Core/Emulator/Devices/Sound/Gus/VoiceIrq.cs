namespace Spice86.Core.Emulator.Devices.Sound.Gus;

/// <summary>
/// Tracks the shared IRQ state across all GUS voices.
/// </summary>
internal sealed class VoiceIrq {
    /// <summary>
    /// Bit mask indicating which voices have triggered a volume IRQ.
    /// </summary>
    public uint VolState { get; set; }

    /// <summary>
    /// Bit mask indicating which voices have triggered a wave IRQ.
    /// </summary>
    public uint WaveState { get; set; }

    /// <summary>
    /// Index of the current voice being processed for IRQ status.
    /// </summary>
    public byte Status { get; set; }
}
