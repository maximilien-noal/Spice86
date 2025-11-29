namespace Spice86.Core.Emulator.Devices.Sound.Gus;

/// <summary>
/// DMA Control Register (41h) for the GUS.
/// Based on UltraSound SDK Section 2.6.1.1.
/// </summary>
internal struct DmaControlRegister {
    private byte _data;

    /// <summary>
    /// Gets or sets the raw register value.
    /// </summary>
    public byte Data {
        readonly get => _data;
        set => _data = value;
    }

    /// <summary>
    /// Gets or sets whether DMA is enabled.
    /// </summary>
    public bool IsEnabled {
        readonly get => (_data & 0x01) != 0;
        set => _data = value ? (byte)(_data | 0x01) : (byte)(_data & ~0x01);
    }

    /// <summary>
    /// Gets or sets whether DMA direction is GUS to host (read from GUS RAM).
    /// </summary>
    public bool IsDirectionGusToHost {
        readonly get => (_data & 0x02) != 0;
        set => _data = value ? (byte)(_data | 0x02) : (byte)(_data & ~0x02);
    }

    /// <summary>
    /// Gets or sets whether the DMA channel is 16-bit.
    /// </summary>
    public bool IsChannel16Bit {
        readonly get => (_data & 0x04) != 0;
        set => _data = value ? (byte)(_data | 0x04) : (byte)(_data & ~0x04);
    }

    /// <summary>
    /// Gets or sets the rate divisor (bits 3-4).
    /// </summary>
    public byte RateDivisor {
        readonly get => (byte)((_data >> 3) & 0x03);
        set => _data = (byte)((_data & ~0x18) | ((value & 0x03) << 3));
    }

    /// <summary>
    /// Gets or sets whether IRQ should be raised on terminal count.
    /// </summary>
    public bool WantsIrqOnTerminalCount {
        readonly get => (_data & 0x20) != 0;
        set => _data = value ? (byte)(_data | 0x20) : (byte)(_data & ~0x20);
    }

    /// <summary>
    /// Sets whether samples are 16-bit (write-only).
    /// <para>
    /// <b>Warning:</b> Bit 6 (0x40) has different meanings for read vs write operations.
    /// Use this property <b>only</b> when writing to the register to set the sample format.
    /// </para>
    /// </summary>
    public bool AreSamples16Bit {
        set => _data = value ? (byte)(_data | 0x40) : (byte)(_data & ~0x40);
    }

    /// <summary>
    /// Gets whether terminal count IRQ is pending (read-only meaning of bit 6).
    /// <para>
    /// <b>Warning:</b> Bit 6 (0x40) has different meanings for read vs write operations.
    /// Use this property <b>only</b> when reading the register to check IRQ status.
    /// </para>
    /// </summary>
    public bool HasPendingTerminalCountIrq {
        readonly get => (_data & 0x40) != 0;
        set => _data = value ? (byte)(_data | 0x40) : (byte)(_data & ~0x40);
    }

    /// <summary>
    /// Gets or sets whether samples have their high bit inverted (sign conversion).
    /// </summary>
    public bool AreSamplesHighBitInverted {
        readonly get => (_data & 0x80) != 0;
        set => _data = value ? (byte)(_data | 0x80) : (byte)(_data & ~0x80);
    }
}

/// <summary>
/// Reset Register (4Ch) for the GUS.
/// Based on UltraSound SDK Section 2.6.1.9.
/// </summary>
internal struct ResetRegister {
    private byte _data;

    /// <summary>
    /// Gets or sets the raw register value.
    /// </summary>
    public byte Data {
        readonly get => _data;
        set => _data = value;
    }

    /// <summary>
    /// Gets or sets whether the GUS is running (0 = stop/reset, 1 = running).
    /// </summary>
    public bool IsRunning {
        readonly get => (_data & 0x01) != 0;
        set => _data = value ? (byte)(_data | 0x01) : (byte)(_data & ~0x01);
    }

    /// <summary>
    /// Gets or sets whether the DAC is enabled. DACs will not run unless this is set.
    /// </summary>
    public bool IsDacEnabled {
        readonly get => (_data & 0x02) != 0;
        set => _data = value ? (byte)(_data | 0x02) : (byte)(_data & ~0x02);
    }

    /// <summary>
    /// Gets or sets whether IRQs are enabled. Must be set to receive GF1-generated IRQs.
    /// </summary>
    public bool AreIrqsEnabled {
        readonly get => (_data & 0x04) != 0;
        set => _data = value ? (byte)(_data | 0x04) : (byte)(_data & ~0x04);
    }
}

/// <summary>
/// Mix Control Register (2X0) for the GUS.
/// Based on UltraSound SDK Section 2.13.
/// </summary>
internal struct MixControlRegister {
    private byte _data;

    /// <summary>
    /// Initializes with default state.
    /// </summary>
    public MixControlRegister() {
        _data = GusConstants.MixControlRegisterDefaultState;
    }

    /// <summary>
    /// Gets or sets the raw register value.
    /// </summary>
    public byte Data {
        readonly get => _data;
        set => _data = value;
    }

    /// <summary>
    /// Gets or sets whether line-in is disabled.
    /// </summary>
    public bool LineInDisabled {
        readonly get => (_data & 0x01) != 0;
        set => _data = value ? (byte)(_data | 0x01) : (byte)(_data & ~0x01);
    }

    /// <summary>
    /// Gets or sets whether line-out is disabled.
    /// </summary>
    public bool LineOutDisabled {
        readonly get => (_data & 0x02) != 0;
        set => _data = value ? (byte)(_data | 0x02) : (byte)(_data & ~0x02);
    }

    /// <summary>
    /// Gets or sets whether microphone is enabled.
    /// </summary>
    public bool MicrophoneEnabled {
        readonly get => (_data & 0x04) != 0;
        set => _data = value ? (byte)(_data | 0x04) : (byte)(_data & ~0x04);
    }

    /// <summary>
    /// Gets or sets whether latches are enabled.
    /// </summary>
    public bool LatchesEnabled {
        readonly get => (_data & 0x08) != 0;
        set => _data = value ? (byte)(_data | 0x08) : (byte)(_data & ~0x08);
    }

    /// <summary>
    /// Gets or sets whether channel 1 IRQ is combined with channel 2.
    /// </summary>
    public bool Channel1IrqCombinedWithChannel2 {
        readonly get => (_data & 0x10) != 0;
        set => _data = value ? (byte)(_data | 0x10) : (byte)(_data & ~0x10);
    }

    /// <summary>
    /// Gets or sets whether MIDI loopback is enabled.
    /// </summary>
    public bool MidiLoopbackEnabled {
        readonly get => (_data & 0x20) != 0;
        set => _data = value ? (byte)(_data | 0x20) : (byte)(_data & ~0x20);
    }

    /// <summary>
    /// Gets or sets whether IRQ control is selected (vs DMA control).
    /// </summary>
    public bool IrqControlSelected {
        readonly get => (_data & 0x40) != 0;
        set => _data = value ? (byte)(_data | 0x40) : (byte)(_data & ~0x40);
    }
}

/// <summary>
/// Address Select Register (2XB) for IRQ and DMA selection.
/// Based on UltraSound SDK Section 2.14-2.15.
/// </summary>
internal struct AddressSelectRegister {
    private byte _data;

    /// <summary>
    /// Gets or sets the raw register value.
    /// </summary>
    public byte Data {
        readonly get => _data;
        set => _data = value;
    }

    /// <summary>
    /// Gets or sets the channel 1 selector (bits 0-2).
    /// </summary>
    public byte Channel1Selector {
        readonly get => (byte)(_data & 0x07);
        set => _data = (byte)((_data & ~0x07) | (value & 0x07));
    }

    /// <summary>
    /// Gets or sets the channel 2 selector (bits 3-5).
    /// </summary>
    public byte Channel2Selector {
        readonly get => (byte)((_data >> 3) & 0x07);
        set => _data = (byte)((_data & ~0x38) | ((value & 0x07) << 3));
    }

    /// <summary>
    /// Gets or sets whether channel 2 is combined with channel 1 (bit 6).
    /// </summary>
    public bool Channel2CombinedWithChannel1 {
        readonly get => (_data & 0x40) != 0;
        set => _data = value ? (byte)(_data | 0x40) : (byte)(_data & ~0x40);
    }
}

/// <summary>
/// GUS Timer state.
/// </summary>
internal sealed class GusTimer {
    /// <summary>
    /// Timer delay in milliseconds.
    /// </summary>
    public double Delay { get; set; }

    /// <summary>
    /// Timer value register.
    /// </summary>
    public byte Value { get; set; } = 0xff;

    /// <summary>
    /// Whether the timer has expired.
    /// </summary>
    public bool HasExpired { get; set; } = true;

    /// <summary>
    /// Whether the timer is actively counting down.
    /// </summary>
    public bool IsCountingDown { get; set; }

    /// <summary>
    /// Whether the timer is masked (won't set expired flag).
    /// </summary>
    public bool IsMasked { get; set; }

    /// <summary>
    /// Whether the timer should raise an IRQ when it expires.
    /// </summary>
    public bool ShouldRaiseIrq { get; set; }

    /// <summary>
    /// Initializes a new timer with the specified default delay.
    /// </summary>
    public GusTimer(double defaultDelay) {
        Delay = defaultDelay;
    }
}
