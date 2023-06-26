﻿namespace Bufdio.Spice86.PortAudio.Engines;

using Bufdio.Spice86.PortAudio;
using Bufdio.Spice86.PortAudio.Engines.Extensions;
using Bufdio.Spice86.PortAudio.Exceptions;

using System;
using System.Runtime.InteropServices;

using PaBinding = Bufdio.Spice86.PortAudio.Bindings.PortAudio.PaBinding;

/// <summary>
/// Interact with output audio device by using PortAudio library.
/// This class cannot be inherited.
/// <para>Implements: <see cref="IAudioEngine"/>.</para>
/// </summary>
public sealed class PortAudioEngine : IAudioEngine {
    private const PaBinding.PaStreamFlags StreamFlags = PaBinding.PaStreamFlags.paNoFlag;
    private readonly AudioEngineOptions _options;
    private readonly IntPtr _stream;
    private bool _disposed;

    /// <summary>
    /// Initializes <see cref="PortAudioEngine"/> object.
    /// </summary>
    /// <param name="framesPerBuffer">Must be a power of 2. Can be 0 for undefined.</param>
    /// <param name="options">Optional audio engine options.</param>
    /// <exception cref="PortAudioException">
    /// Might be thrown when errors occured during PortAudio stream initialization.
    /// </exception>
    public PortAudioEngine(int framesPerBuffer, AudioEngineOptions? options = default) {
        _options = options ?? new AudioEngineOptions();

        PaBinding.PaStreamParameters parameters = new PaBinding.PaStreamParameters {
            channelCount = _options.Channels,
            device = _options.Device.DeviceIndex,
            hostApiSpecificStreamInfo = IntPtr.Zero,
            sampleFormat = BufdioLib.Constants.PaSampleFormat,
            suggestedLatency = _options.Latency
        };

        IntPtr stream;

        unsafe {
            PaBinding.PaStreamParameters tempParameters;
            IntPtr parametersPtr = new(&tempParameters);
            Marshal.StructureToPtr(parameters, parametersPtr, false);

            int code = PaBinding.Pa_OpenStream(
                new IntPtr(&stream),
                IntPtr.Zero,
                parametersPtr,
                _options.SampleRate,
                framesPerBuffer,
                StreamFlags,
                null,
                IntPtr.Zero);

            code.PaGuard();
        }

        _stream = stream;

        PaBinding.Pa_StartStream(_stream).PaGuard();
    }

    /// <inheritdoc />
    public void Send(Span<float> samples) {
        unsafe {
            fixed (float* buffer = samples) {
                int frames = samples.Length / _options.Channels;
                PaBinding.Pa_WriteStream(_stream, (IntPtr)buffer, frames);
            }
        }
    }

    /// <summary>
    /// Releases the native library (PortAudio)
    /// </summary>
    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing) {
        if(!_disposed) {
            if(disposing) {
                PaBinding.Pa_AbortStream(_stream);
                PaBinding.Pa_CloseStream(_stream);
            }
            _disposed = true;
        }
    }
}
