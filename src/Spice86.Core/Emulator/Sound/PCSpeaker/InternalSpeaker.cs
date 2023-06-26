﻿namespace Spice86.Core.Emulator.Sound.PCSpeaker;

using Spice86.Core.Backend;
using Spice86.Core.Backend.Audio;
using Spice86.Core.Emulator.Sound;

using System.Collections.Concurrent;
using System.Diagnostics;

/// <summary>
/// Emulates a PC speaker.
/// </summary>
public sealed class InternalSpeaker : IDisposable {
    /// <summary>
    /// Value into which the input frequency is divided to get the frequency in Hz.
    /// </summary>
    private const double FrequencyFactor = 1193180;

    private readonly int _outputSampleRate = 48000;
    private readonly int _ticksPerSample;
    private readonly LatchedUInt16 _frequencyRegister = new();
    private readonly Stopwatch _durationTimer = new();
    private readonly ConcurrentQueue<QueuedNote> _queuedNotes = new();
    private readonly object _threadStateLock = new();
    private SpeakerControl _controlRegister = SpeakerControl.UseTimer;
    private Task? _generateWaveformTask;
    private readonly CancellationTokenSource _cancelGenerateWaveform = new();
    private int _currentPeriod;

    private bool _disposed;

    private Configuration Configuration { get; init; }
    /// <summary>
    /// Initializes a new instance of the InternalSpeaker class.
    /// </summary>
    public InternalSpeaker(Configuration configuration) {
        Configuration = configuration;
        _frequencyRegister.ValueChanged += FrequencyChanged;
        _ticksPerSample = (int)(Stopwatch.Frequency / (double)_outputSampleRate);
    }

    /// <summary>
    /// Gets the current frequency in Hz.
    /// </summary>
    private double Frequency => FrequencyFactor / _frequencyRegister;
    
    /// <summary>
    /// Gets the current period in samples.
    /// </summary>
    private int PeriodInSamples => (int)(_outputSampleRate / Frequency);

    /// <summary>
    /// Reads a byte from the control register.
    /// </summary>
    /// <param name="port">The port number to read from</param>
    /// <returns>The value from the control register</returns>
    /// <exception cref="NotSupportedException">If the port number is not equal to 0x61</exception>
    public byte ReadByte(int port) {
        if (port == 0x61) {
            return (byte)_controlRegister;
        }

        throw new NotSupportedException();
    }
    
    /// <summary>
    /// Writes a byte either to the control register or the frequency register
    /// </summary>
    /// <param name="port">The port number to write to.</param>
    /// <param name="value">The value being written.</param>
    /// <exception cref="NotSupportedException">If the port number is not recognized.</exception>
    public void WriteByte(int port, byte value) {
        if (port == 0x61) {
            SpeakerControl oldValue = _controlRegister;
            _controlRegister = (SpeakerControl)value;
            if ((oldValue & SpeakerControl.SpeakerOn) != 0 && (_controlRegister & SpeakerControl.SpeakerOn) == 0) {
                SpeakerDisabled();
            }
        } else if (port == 0x42) {
            _frequencyRegister.WriteByte(value);
        } else {
            throw new NotSupportedException();
        }
    }

    /// <inheritdoc />
    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing) {
        if(!_disposed) {
            if(disposing) {
                _frequencyRegister.ValueChanged -= FrequencyChanged;
                lock (_threadStateLock) {
                    _cancelGenerateWaveform.Cancel();
                }
            }
            _disposed = true;
        }
    }

    /// <summary>
    /// Fills a buffer with silence.
    /// </summary>
    /// <param name="buffer">Buffer to fill.</param>
    private static void GenerateSilence(Span<byte> buffer) => buffer.Fill(127);

    /// <summary>
    /// Invoked when the speaker has been turned off.
    /// </summary>
    private void SpeakerDisabled() {
        EnqueueCurrentNote();
        _currentPeriod = 0;
    }
    /// <summary>
    /// Invoked when the frequency has changed.
    /// </summary>
    /// <param name="source">Source of the event.</param>
    /// <param name="e">Unused EventArgs instance.</param>
    private void FrequencyChanged(object? source, EventArgs e) {
        EnqueueCurrentNote();

        _durationTimer.Reset();
        _durationTimer.Start();
        _currentPeriod = PeriodInSamples;
    }
    /// <summary>
    /// Enqueues the current note.
    /// </summary>
    private void EnqueueCurrentNote() {
        if (_durationTimer.IsRunning && _currentPeriod != 0) {
            _durationTimer.Stop();

            int periodDuration = _ticksPerSample * _currentPeriod;
            int repetitions = (int)(_durationTimer.ElapsedTicks / periodDuration);
            _queuedNotes.Enqueue(new QueuedNote(_currentPeriod, repetitions));

            lock (_threadStateLock) {
                if (_generateWaveformTask == null || _generateWaveformTask.IsCompleted) {
                    _generateWaveformTask = Task.Run(GenerateWaveformAsync);
                }
            }
        }
    }
    /// <summary>
    /// Fills a buffer with a square wave of the current frequency.
    /// </summary>
    /// <param name="buffer">Buffer to fill.</param>
    /// <param name="period">The number of samples in the period.</param>
    /// <returns>Number of bytes written to the buffer.</returns>
    private int GenerateSquareWave(Span<byte> buffer, int period) {
        if (period < 2) {
            buffer[0] = 127;
            return 1;
        }

        int halfPeriod = period / 2;
        buffer[..halfPeriod].Fill(96);
        buffer.Slice(halfPeriod, halfPeriod).Fill(120);

        return period;
    }
    /// <summary>
    /// Generates the PC speaker waveform.
    /// </summary>
    private async Task GenerateWaveformAsync() {
        using AudioPlayer? player = AudioPlayerAccess.CreatePlayer();
        if (player is null) {
            return;
        }
        FillWithSilence(player);

        byte[] buffer = new byte[4096];
        byte[] writeBuffer = buffer;
        bool expandToStereo = false;
        if (player.Format.Channels == 2) {
            writeBuffer = new byte[buffer.Length * 2];
            expandToStereo = true;
        }

        int idleCount = 0;

        while (idleCount < 10000) {
            if (_queuedNotes.TryDequeue(out QueuedNote note)) {
                int samples = GenerateSquareWave(buffer, note.Period);
                int periods = note.PeriodCount;

                if (expandToStereo) {
                    ChannelAdapter.MonoToStereo(buffer.AsSpan(0, samples), writeBuffer.AsSpan(0, samples * 2));
                    samples *= 2;
                }

                while (periods > 0) {
                    AudioPlayerAccess.WriteFullBuffer(player, writeBuffer.AsSpan(0, samples));
                    periods--;
                }

                GenerateSilence(buffer);
                idleCount = 0;
            } else {
                float[] floatArray = new float[buffer.Length];

                for (int i = 0; i < buffer.Length; i++) {
                    floatArray[i] = buffer[i];
                }


                while (player.WriteData(floatArray.AsSpan()) > 0) {
                }

                await Task.Delay(5, _cancelGenerateWaveform.Token);
                idleCount++;
            }

            _cancelGenerateWaveform.Token.ThrowIfCancellationRequested();
        }
    }

    private static void FillWithSilence(AudioPlayer player) {
        float[] buffer = new float[4096];
        Span<float> span = buffer.AsSpan();

        while (player.WriteData(span) > 0) {
        }
    }
}
