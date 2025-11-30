namespace Spice86.Core.Emulator.Devices.Sound.Gus;

/// <summary>
/// Control parameters for GUS voice wave and volume registers.
/// </summary>
internal sealed class VoiceCtrl {
    /// <summary>
    /// Reference to the shared IRQ state for this control type.
    /// </summary>
    private readonly Func<VoiceIrq, uint> _getIrqState;
    private readonly Action<VoiceIrq, uint> _setIrqState;
    private readonly VoiceIrq _voiceIrq;

    /// <summary>
    /// Initializes a new voice control with a reference to shared IRQ state.
    /// </summary>
    /// <param name="voiceIrq">The shared voice IRQ state.</param>
    /// <param name="getIrqState">Function to get the IRQ state.</param>
    /// <param name="setIrqState">Action to set the IRQ state.</param>
    public VoiceCtrl(VoiceIrq voiceIrq, Func<VoiceIrq, uint> getIrqState, Action<VoiceIrq, uint> setIrqState) {
        _voiceIrq = voiceIrq;
        _getIrqState = getIrqState;
        _setIrqState = setIrqState;
    }

    /// <summary>
    /// Gets or sets the shared IRQ state for this control type.
    /// </summary>
    public uint IrqState {
        get => _getIrqState(_voiceIrq);
        set => _setIrqState(_voiceIrq, value);
    }

    /// <summary>
    /// Start position (for wave) or start volume level (for volume).
    /// </summary>
    public int Start { get; set; }

    /// <summary>
    /// End position (for wave) or end volume level (for volume).
    /// </summary>
    public int End { get; set; }

    /// <summary>
    /// Current position (for wave) or current volume level (for volume).
    /// </summary>
    public int Pos { get; set; }

    /// <summary>
    /// Increment value for position updates.
    /// </summary>
    public int Inc { get; set; }

    /// <summary>
    /// Raw rate value as programmed by software.
    /// </summary>
    public ushort Rate { get; set; }

    /// <summary>
    /// Control state flags (reset, stopped, 16-bit, loop, etc.).
    /// </summary>
    public byte State { get; set; } = GusConstants.VoiceDefaultState;
}

/// <summary>
/// Voice control state flags.
/// </summary>
[Flags]
internal enum VoiceCtrlFlags : byte {
    /// <summary>
    /// Voice is in reset state.
    /// </summary>
    Reset = 0x01,

    /// <summary>
    /// Voice playback is stopped.
    /// </summary>
    Stopped = 0x02,

    /// <summary>
    /// Voice is disabled (reset + stopped).
    /// </summary>
    Disabled = Reset | Stopped,

    /// <summary>
    /// Voice uses 16-bit samples.
    /// </summary>
    Bit16 = 0x04,

    /// <summary>
    /// Voice loops at boundaries.
    /// </summary>
    Loop = 0x08,

    /// <summary>
    /// Voice uses bidirectional (ping-pong) looping.
    /// </summary>
    Bidirectional = 0x10,

    /// <summary>
    /// Voice should raise IRQ at boundaries.
    /// </summary>
    RaiseIrq = 0x20,

    /// <summary>
    /// Voice position is decreasing.
    /// </summary>
    Decreasing = 0x40
}
