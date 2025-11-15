// SPDX-FileCopyrightText: 2013-2025 Nuked-OPL3 by nukeykt
// SPDX-License-Identifier: LGPL-2.1

namespace Spice86.Libs.Sound.Devices.NukedOpl3;

/// <summary>
/// Represents opl 3 chip.
/// </summary>
public sealed partial class Opl3Chip {
    private const int WriteBufferSize = 1024;
    private const int WriteBufferDelay = 2;
    private const int ResampleFractionBits = 10;

    private Opl3Channel[] Channels { get; } = new Opl3Channel[18];
    /// <summary>
    /// Gets slots.
    /// </summary>
    public Opl3Operator[] Slots { get; } = new Opl3Operator[36];
    /// <summary>
    /// The timer.
    /// </summary>
    internal ushort Timer;
    /// <summary>
    /// The eg timer.
    /// </summary>
    internal ulong EgTimer;
    /// <summary>
    /// The eg timer rem.
    /// </summary>
    internal byte EgTimerRem;
    /// <summary>
    /// The eg state.
    /// </summary>
    internal byte EgState;
    /// <summary>
    /// The eg add.
    /// </summary>
    internal byte EgAdd;
    /// <summary>
    /// The eg timer low.
    /// </summary>
    internal byte EgTimerLow;
    /// <summary>
    /// The newm.
    /// </summary>
    internal byte NewM;
    /// <summary>
    /// The nts.
    /// </summary>
    internal byte Nts;
    /// <summary>
    /// The rhythm.
    /// </summary>
    internal byte Rhythm;
    /// <summary>
    /// The vibrato position.
    /// </summary>
    internal byte VibratoPosition;
    /// <summary>
    /// The vibrato shift.
    /// </summary>
    internal byte VibratoShift;
    /// <summary>
    /// The tremolo.
    /// </summary>
    internal byte Tremolo;
    /// <summary>
    /// The tremolo position.
    /// </summary>
    internal byte TremoloPosition;
    /// <summary>
    /// The tremolo shift.
    /// </summary>
    internal byte TremoloShift;
    /// <summary>
    /// The noise.
    /// </summary>
    internal uint Noise;
    /// <summary>
    /// The zero mod.
    /// </summary>
    internal short ZeroMod;
    private int[] MixBuffer { get; } = new int[4];
    /// <summary>
    /// The rhythm hihat bit 2.
    /// </summary>
    internal byte RhythmHihatBit2;
    /// <summary>
    /// The rhythm hihat bit 3.
    /// </summary>
    internal byte RhythmHihatBit3;
    /// <summary>
    /// The rhythm hihat bit 7.
    /// </summary>
    internal byte RhythmHihatBit7;
    /// <summary>
    /// The rhythm hihat bit 8.
    /// </summary>
    internal byte RhythmHihatBit8;
    /// <summary>
    /// The rhythm tom bit 3.
    /// </summary>
    internal byte RhythmTomBit3;
    /// <summary>
    /// The rhythm tom bit 5.
    /// </summary>
    internal byte RhythmTomBit5;
#if OPL_ENABLE_STEREOEXT
    /// <summary>
    /// The stereo extension.
    /// </summary>
    internal byte StereoExtension;
#endif
    /// <summary>
    /// The rate ratio.
    /// </summary>
    internal int RateRatio;
    /// <summary>
    /// The sample counter.
    /// </summary>
    internal int SampleCounter;
    private short[] OldSamples { get; } = new short[4];
    private short[] Samples { get; } = new short[4];
    /// <summary>
    /// The write buffer sample counter.
    /// </summary>
    internal ulong WriteBufferSampleCounter;
    /// <summary>
    /// The write buffer current.
    /// </summary>
    internal uint WriteBufferCurrent;
    /// <summary>
    /// The write buffer last.
    /// </summary>
    internal uint WriteBufferLast;
    /// <summary>
    /// The write buffer last time.
    /// </summary>
    internal ulong WriteBufferLastTime;
    private Opl3WriteBufferEntry[] WriteBuffer { get; } = new Opl3WriteBufferEntry[WriteBufferSize];

    /// <summary>
    /// Performs the opl 3 chip operation.
    /// </summary>
    public Opl3Chip() {
        for (int i = 0; i < Channels.Length; i++) {
            Channels[i] = new Opl3Channel {
                Chip = this,
                ChannelNumber = (byte)i,
                ChannelType = ChannelType.TwoOp
            };
            for (int j = 0; j < Channels[i].Out.Length; j++) {
                Channels[i].Out[j] = ShortSignalSource.Zero;
            }
        }

        for (int i = 0; i < Slots.Length; i++) {
            Slots[i] = new Opl3Operator {
                Chip = this,
                SlotIndex = (byte)i,
                ModulationSource = ShortSignalSource.Zero,
                TremoloEnabled = false
            };
        }

        for (int i = 0; i < WriteBuffer.Length; i++) {
            WriteBuffer[i] = new Opl3WriteBufferEntry();
        }
    }


    /* Original C: void OPL3_Reset(opl3_chip *chip, uint32_t samplerate); */
    public void Reset(uint sampleRate) {
        ResetInternal(sampleRate);
    }

    /* Original C: void OPL3_WriteReg(opl3_chip *chip, uint16_t reg, uint8_t v); */
    /// <summary>
    /// Writes register.
    /// </summary>
    /// <param name="register">The register.</param>
    /// <param name="value">The value.</param>
    public void WriteRegister(ushort register, byte value) {
        WriteRegisterInternal(register, value);
    }

    /* Original C: void OPL3_WriteRegBuffered(opl3_chip *chip, uint16_t reg, uint8_t v); */
    /// <summary>
    /// Writes register buffered.
    /// </summary>
    /// <param name="register">The register.</param>
    /// <param name="value">The value.</param>
    public void WriteRegisterBuffered(ushort register, byte value) {
        WriteRegisterBufferedInternal(register, value);
    }

    /* Original C: void OPL3_Generate4Ch(opl3_chip *chip, int16_t *buf4); */
    /// <summary>
    /// Performs the generate 4 channels operation.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    public void Generate4Channels(Span<short> buffer) {
        Generate4ChCore(buffer);
    }

    /* Original C: void OPL3_Generate(opl3_chip *chip, int16_t *buf); */
    /// <summary>
    /// Performs the generate operation.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    public void Generate(Span<short> buffer) {
        GenerateCore(buffer);
    }

    /* Original C: void OPL3_Generate4ChResampled(opl3_chip *chip, int16_t *buf4); */
    /// <summary>
    /// Performs the generate 4 channels resampled operation.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    public void Generate4ChannelsResampled(Span<short> buffer) {
        Generate4ChResampledCore(buffer);
    }

    /* Original C: void OPL3_GenerateResampled(opl3_chip *chip, int16_t *buf); */
    /// <summary>
    /// Performs the generate resampled operation.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    public void GenerateResampled(Span<short> buffer) {
        GenerateResampledCore(buffer);
    }

    /* Original C: void OPL3_Generate4ChStream(opl3_chip *chip, int16_t *sndptr1, int16_t *sndptr2, uint32_t numsamples); */
    /// <summary>
    /// Performs the generate 4 channel stream operation.
    /// </summary>
    /// <param name="stream1">The stream 1.</param>
    /// <param name="stream2">The stream 2.</param>
    public void Generate4ChannelStream(Span<short> stream1, Span<short> stream2) {
        Generate4ChStreamCore(stream1, stream2);
    }

    /* Original C: void OPL3_GenerateStream(opl3_chip *chip, int16_t *sndptr, uint32_t numsamples); */
    /// <summary>
    /// Performs the generate stream operation.
    /// </summary>
    /// <param name="stream">The stream.</param>
    public void GenerateStream(Span<short> stream) {
        GenerateStreamCore(stream);
    }

    /// <summary>
    /// Gets write buffer sample counter.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    internal ulong GetWriteBufferSampleCounter() {
        return WriteBufferSampleCounter;
    }

    /// <summary>
    /// Performs the peek next buffered write sample operation.
    /// </summary>
    internal ulong? PeekNextBufferedWriteSample() {
        Opl3WriteBufferEntry entry = WriteBuffer[(int)WriteBufferCurrent];
        return (entry.Register & 0x200) != 0 ? entry.Time : null;
    }

    /// <summary>
    /// Processes write buffer until.
    /// </summary>
    /// <param name="inclusiveSampleIndex">The inclusive sample index.</param>
    internal void ProcessWriteBufferUntil(ulong inclusiveSampleIndex) {
        if (WriteBufferSampleCounter > inclusiveSampleIndex) {
            return;
        }

        do {
            while (true) {
                Opl3WriteBufferEntry entry = WriteBuffer[(int)WriteBufferCurrent];
                if (entry.Time > WriteBufferSampleCounter) {
                    break;
                }

                if ((entry.Register & 0x200) == 0) {
                    break;
                }

                ushort register = (ushort)(entry.Register & 0x1ff);
                entry.Register = register;
                WriteRegisterInternal(register, entry.Data);
                WriteBufferCurrent = (WriteBufferCurrent + 1) % WriteBufferSize;
            }

            WriteBufferSampleCounter++;
        } while (WriteBufferSampleCounter <= inclusiveSampleIndex);
    }
}

/// <summary>
/// Defines channel type values.
/// </summary>
internal enum ChannelType : byte {
    TwoOp = 0,
    FourOp = 1,
    FourOpPair = 2,
    Drum = 3
}

/// <summary>
/// Defines envelope key type values.
/// </summary>
internal enum EnvelopeKeyType : byte {
    Normal = 0x01,
    Drum = 0x02
}

/// <summary>
/// Defines envelope generator stage values.
/// </summary>
public enum EnvelopeGeneratorStage : byte {
    Attack = 0,
    Decay = 1,
    Sustain = 2,
    Release = 3
}

/// <summary>
/// Represents opl 3 write buffer entry.
/// </summary>
internal sealed class Opl3WriteBufferEntry {
    /// <summary>
    /// The data.
    /// </summary>
    internal byte Data;
    /// <summary>
    /// The register.
    /// </summary>
    internal ushort Register;
    /// <summary>
    /// The time.
    /// </summary>
    internal ulong Time;
}