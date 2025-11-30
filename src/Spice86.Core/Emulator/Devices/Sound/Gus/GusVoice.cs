namespace Spice86.Core.Emulator.Devices.Sound.Gus;

/// <summary>
/// Represents a single GUS voice channel capable of wavetable synthesis.
/// Each voice is a mono audio stream with its own sample data, volume, pan position,
/// start/stop/loop controls, and IRQ capabilities.
/// </summary>
internal sealed class GusVoice {
    private readonly byte _voiceNumber;
    private readonly uint _irqMask;
    private readonly VoiceIrq _sharedIrq;

    /// <summary>
    /// Volume control state for this voice.
    /// </summary>
    public VoiceCtrl VolCtrl { get; }

    /// <summary>
    /// Wave (sample position) control state for this voice.
    /// </summary>
    public VoiceCtrl WaveCtrl { get; }

    /// <summary>
    /// Pan position (0 = full left, 7 = center, 15 = full right).
    /// </summary>
    public byte PanPosition { get; private set; } = GusConstants.PanDefaultPosition;

    /// <summary>
    /// Accumulated milliseconds of 8-bit sample generation (for statistics).
    /// </summary>
    public uint Generated8BitMs { get; set; }

    /// <summary>
    /// Accumulated milliseconds of 16-bit sample generation (for statistics).
    /// </summary>
    public uint Generated16BitMs { get; set; }

    /// <summary>
    /// Initializes a new GUS voice.
    /// </summary>
    /// <param name="voiceNumber">The voice index (0-31).</param>
    /// <param name="voiceIrq">Shared IRQ state across all voices.</param>
    public GusVoice(byte voiceNumber, VoiceIrq voiceIrq) {
        _voiceNumber = voiceNumber;
        _irqMask = 1u << voiceNumber;
        _sharedIrq = voiceIrq;

        VolCtrl = new VoiceCtrl(voiceIrq, v => v.VolState, (v, val) => v.VolState = val);
        WaveCtrl = new VoiceCtrl(voiceIrq, v => v.WaveState, (v, val) => v.WaveState = val);
    }

    /// <summary>
    /// Renders audio frames from this voice into the provided buffer.
    /// </summary>
    /// <param name="ram">GUS RAM containing sample data.</param>
    /// <param name="volScalars">Volume scalar lookup table.</param>
    /// <param name="panScalars">Pan scalar lookup table (left/right pairs).</param>
    /// <param name="frames">Output buffer for stereo audio frames.</param>
    public void RenderFrames(
        byte[] ram,
        float[] volScalars,
        (float Left, float Right)[] panScalars,
        Span<(float Left, float Right)> frames) {
        // Skip if voice is disabled
        if (((VoiceCtrlFlags)VolCtrl.State & (VoiceCtrlFlags)WaveCtrl.State & VoiceCtrlFlags.Disabled) != 0) {
            return;
        }

        (float Left, float Right) panScalar = panScalars[PanPosition];

        // Render each frame
        for (int i = 0; i < frames.Length; i++) {
            float sample = GetSample(ram);
            sample *= PopVolScalar(volScalars);
            frames[i].Left += sample * panScalar.Left;
            frames[i].Right += sample * panScalar.Right;
        }

        // Track statistics
        if (Is16Bit) {
            Generated16BitMs++;
        } else {
            Generated8BitMs++;
        }
    }

    /// <summary>
    /// Gets whether this voice is configured for 16-bit samples.
    /// </summary>
    public bool Is16Bit => ((VoiceCtrlFlags)WaveCtrl.State & VoiceCtrlFlags.Bit16) != 0;

    /// <summary>
    /// Reads the volume control state including IRQ pending flag.
    /// </summary>
    public byte ReadVolState() {
        return ReadCtrlState(VolCtrl);
    }

    /// <summary>
    /// Reads the wave control state including IRQ pending flag.
    /// </summary>
    public byte ReadWaveState() {
        return ReadCtrlState(WaveCtrl);
    }

    /// <summary>
    /// Resets both wave and volume controls to their default states.
    /// </summary>
    public void ResetCtrls() {
        VolCtrl.Pos = 0;
        UpdateVolState(0x1);
        UpdateWaveState(0x1);
        WritePanPot(GusConstants.PanDefaultPosition);
    }

    /// <summary>
    /// Sets the pan pot register (0-15, clamped).
    /// </summary>
    public void WritePanPot(byte pos) {
        PanPosition = Math.Min(pos, (byte)(GusConstants.PanPositions - 1));
    }

    /// <summary>
    /// Sets the volume rate register and calculates the increment value.
    /// Four volume-index-rate "banks" are available (per GUS SDK):
    /// - Bank 0 (0-63): single index increments
    /// - Bank 1 (64-127): 1/8th fractional increments
    /// - Bank 2 (128-191): 1/64th fractional increments
    /// - Bank 3 (192-255): 1/512th fractional increments
    /// </summary>
    public void WriteVolRate(ushort val) {
        VolCtrl.Rate = val;
        // Bank length is 63 positions per bank (0-63 in each bank)
        const int volumeRateBankLength = 63;
        int posInBank = val & volumeRateBankLength;
        // Clamp shift amount to prevent integer overflow (max 30 for int32)
        int shiftAmount = Math.Min(3 * (val >> 6), 30);
        int decimator = 1 << shiftAmount;
        VolCtrl.Inc = CeilDivide(posInBank * GusConstants.VolumeIncScalar, decimator);
    }

    /// <summary>
    /// Sets the wave rate register and calculates the increment value.
    /// </summary>
    public void WriteWaveRate(ushort val) {
        WaveCtrl.Rate = val;
        WaveCtrl.Inc = (int)CeilDivideUnsigned((uint)val, 2u);
    }

    /// <summary>
    /// Updates the volume control state.
    /// </summary>
    /// <returns>True if the IRQ state changed.</returns>
    public bool UpdateVolState(byte state) {
        return UpdateCtrlState(VolCtrl, state);
    }

    /// <summary>
    /// Updates the wave control state.
    /// </summary>
    /// <returns>True if the IRQ state changed.</returns>
    public bool UpdateWaveState(byte state) {
        return UpdateCtrlState(WaveCtrl, state);
    }

    private byte ReadCtrlState(VoiceCtrl ctrl) {
        byte state = ctrl.State;
        if ((ctrl.IrqState & _irqMask) != 0) {
            state |= 0x80;
        }
        return state;
    }

    private bool UpdateCtrlState(VoiceCtrl ctrl, byte state) {
        uint origIrqState = ctrl.IrqState;

        // Manually set the IRQ if bits 7 and 5 are both set
        if ((state & 0xa0) == 0xa0) {
            ctrl.IrqState |= _irqMask;
        } else {
            ctrl.IrqState &= ~_irqMask;
        }

        // Always update the state (clear bit 7)
        ctrl.State = (byte)(state & 0x7f);

        return origIrqState != ctrl.IrqState;
    }

    /// <summary>
    /// Checks if wave rollover condition is met.
    /// Rollover means fire IRQ but continue playing (no loop/restart).
    /// </summary>
    private bool CheckWaveRolloverCondition() {
        return ((VoiceCtrlFlags)VolCtrl.State & VoiceCtrlFlags.Bit16) != 0 &&
               ((VoiceCtrlFlags)WaveCtrl.State & VoiceCtrlFlags.Loop) == 0;
    }

    private float GetSample(byte[] ram) {
        int pos = PopWavePos();
        int addr = pos / GusConstants.WaveWidth;
        int fraction = pos & (GusConstants.WaveWidth - 1);
        bool shouldInterpolate = WaveCtrl.Inc < GusConstants.WaveWidth && fraction != 0;

        float sample = Is16Bit ? Read16BitSample(ram, addr) : Read8BitSample(ram, addr);

        if (shouldInterpolate) {
            int nextAddr = addr + 1;
            // Ensure nextAddr does not exceed the end of the sample data
            int endAddr = WaveCtrl.End / GusConstants.WaveWidth;
            // Use current sample to avoid discontinuity at sample end
            float nextSample = nextAddr > endAddr
                ? sample
                : (Is16Bit ? Read16BitSample(ram, nextAddr) : Read8BitSample(ram, nextAddr));
            const float waveWidthInv = 1.0f / GusConstants.WaveWidth;
            sample += (nextSample - sample) * fraction * waveWidthInv;
        }

        return sample;
    }

    private int PopWavePos() {
        int currentPos = WaveCtrl.Pos;
        IncrementCtrlPos(WaveCtrl, CheckWaveRolloverCondition());
        return currentPos;
    }

    private float PopVolScalar(float[] volScalars) {
        int i = CeilDivide(VolCtrl.Pos, GusConstants.VolumeIncScalar);
        i = Math.Clamp(i, 0, volScalars.Length - 1);
        IncrementCtrlPos(VolCtrl, false);
        return volScalars[i];
    }

    private void IncrementCtrlPos(VoiceCtrl ctrl, bool dontLoopOrRestart) {
        VoiceCtrlFlags state = (VoiceCtrlFlags)ctrl.State;
        if ((state & VoiceCtrlFlags.Disabled) != 0) {
            return;
        }

        int remaining;
        if ((state & VoiceCtrlFlags.Decreasing) != 0) {
            ctrl.Pos -= ctrl.Inc;
            remaining = ctrl.Start - ctrl.Pos;
        } else {
            ctrl.Pos += ctrl.Inc;
            remaining = ctrl.Pos - ctrl.End;
        }

        // Not yet reaching a boundary
        if (remaining < 0) {
            return;
        }

        // Generate IRQ if requested
        if ((state & VoiceCtrlFlags.RaiseIrq) != 0) {
            ctrl.IrqState |= _irqMask;
        }

        // Allow position to move beyond limit (rollover)
        if (dontLoopOrRestart) {
            return;
        }

        // Should we loop?
        if ((state & VoiceCtrlFlags.Loop) != 0) {
            // Bidirectional looping
            if ((state & VoiceCtrlFlags.Bidirectional) != 0) {
                ctrl.State ^= (byte)VoiceCtrlFlags.Decreasing;
            }
            ctrl.Pos = ((VoiceCtrlFlags)ctrl.State & VoiceCtrlFlags.Decreasing) != 0
                ? ctrl.End - remaining
                : ctrl.Start + remaining;
        } else {
            // Stop the voice
            ctrl.State |= 1;
            ctrl.Pos = ((VoiceCtrlFlags)ctrl.State & VoiceCtrlFlags.Decreasing) != 0
                ? ctrl.Start
                : ctrl.End;
        }
    }

    // Sample conversion constant: scales 8-bit samples to 16-bit range
    private const float Sample8BitScalar = 256f;

    // 16-bit addressing layout masks for GUS memory
    private const int Addr16BitUpperMask = 0b1100_0000_0000_0000_0000;
    private const int Addr16BitLowerMask = 0b0001_1111_1111_1111_1111;

    private static float Read8BitSample(byte[] ram, int addr) {
        int i = addr & 0xfffff;
        if (i >= ram.Length) {
            return 0;
        }
        // Convert unsigned 8-bit to signed 16-bit range
        return (sbyte)ram[i] * Sample8BitScalar;
    }

    private static float Read16BitSample(byte[] ram, int addr) {
        // 16-bit addressing uses a different layout
        int upper = addr & Addr16BitUpperMask;
        int lower = addr & Addr16BitLowerMask;
        int i = upper | (lower << 1);

        if (i + 1 >= ram.Length) {
            return 0;
        }

        // Little-endian 16-bit sample
        return (short)(ram[i] | (ram[i + 1] << 8));
    }

    private static int CeilDivide(int numerator, int denominator) {
        if (denominator == 0) {
            return 0;
        }
        return (numerator + denominator - 1) / denominator;
    }

    private static uint CeilDivideUnsigned(uint numerator, uint denominator) {
        if (denominator == 0) {
            return 0;
        }
        return (numerator + denominator - 1) / denominator;
    }
}
