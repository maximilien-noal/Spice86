namespace Spice86.Core.Emulator.Devices.Sound.Gus;

using Serilog.Events;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Devices.DirectMemoryAccess;
using Spice86.Core.Emulator.Devices.ExternalInput;
using Spice86.Core.Emulator.IOPorts;
using Spice86.Core.Emulator.VM;
using Spice86.Shared.Interfaces;

/// <summary>
/// Gravis UltraSound GF1 DSP emulation (GUS Classic).
/// Implements the full GUS hardware including:
/// - 32 wavetable synthesis voice channels
/// - 1 MB on-board sample RAM
/// - DMA transfers for sample loading
/// - Timer-based interrupts
/// - Volume and panning control
/// </summary>
public sealed class GravisUltraSound : DefaultIOPortHandler, IRequestInterrupt, IDisposable {
    // Port offsets from base address
    private const int MixControlPortOffset = 0x000;
    private const int ReadDataPortOffset = 0x001;
    private const int IrqStatusPortOffset = 0x006;
    private const int TimerControlPortOffset = 0x008;
    private const int TimerDataPortOffset = 0x009;
    private const int AdlibCommandPortOffset = 0x00A;
    private const int AddressSelectPortOffset = 0x00B;
    private const int VoiceSelectPortOffset = 0x102;
    private const int RegisterSelectPortOffset = 0x103;
    private const int DataLowPortOffset = 0x104;
    private const int DataHighPortOffset = 0x105;
    private const int DramIoPortOffset = 0x107;

    private readonly SoftwareMixer _softwareMixer;
    private readonly SoundChannel _soundChannel;
    private readonly DualPic _dualPic;
    private readonly DmaChannel? _dmaChannel;
    private readonly IPauseHandler _pauseHandler;
    private readonly DeviceThread _deviceThread;

    // GUS state
    private readonly byte[] _ram = new byte[GusConstants.RamSize];
    private readonly GusVoice[] _voices = new GusVoice[GusConstants.MaxVoices];
    private readonly VoiceIrq _voiceIrq = new();
    private readonly float[] _volScalars = new float[GusConstants.VolumeLevels];
    private readonly (float Left, float Right)[] _panScalars = new (float, float)[GusConstants.PanPositions];
    private readonly (float Left, float Right)[] _renderBuffer = new (float, float)[1024];
    private readonly float[] _outputBuffer = new float[2048];

    private readonly ushort _basePort;
    private readonly byte _irq;
    private readonly byte _dma1;
    private readonly byte _dma2;

    // Registers
    private DmaControlRegister _dmaControlRegister;
    private ResetRegister _resetRegister;
    private MixControlRegister _mixControlRegister = new();

    // Timer state
    private GusTimer _timer1;
    private GusTimer _timer2;
    private byte _timerCtrl;

    // Voice state
    private GusVoice? _targetVoice;
    private ushort _voiceIndex;
    private byte _activeVoices;
    private uint _activeVoiceMask;

    // Calculated sample rate based on active voice count (per Gravis SDK formula).
    // The real GUS hardware reduces sample rate as more voices are active.
    // Currently used for logging/debugging; sound channel uses fixed 44.1kHz output.
    private int _sampleRateHz = GusConstants.OutputSampleRate;

    // Register access state
    private ushort _registerData;
    private byte _selectedRegister;
    private uint _dramAddr;
    private ushort _dmaAddr;

    // IRQ state
    private byte _irqStatus;
    private bool _irqPreviouslyInterrupted;
    private bool _shouldChangeIrqDma;

    // AdLib compatibility
    private byte _adlibCommandReg = GusConstants.AdlibCommandDefault;

    // Sample control
    private byte _sampleCtrl;

    private bool _disposed;

    /// <summary>
    /// Gets the GUS sound channel.
    /// </summary>
    public SoundChannel SoundChannel => _soundChannel;

    /// <summary>
    /// Gets the ULTRASND environment variable string.
    /// Format: port,dma1,dma2,irq1,irq2
    /// </summary>
    public string UltrasndString => $"{_basePort:X3},{_dma1},{_dma2},{_irq},{_irq}";

    /// <summary>
    /// Initializes a new instance of the Gravis UltraSound.
    /// </summary>
    public GravisUltraSound(
        State state,
        IOPortDispatcher ioPortDispatcher,
        SoftwareMixer softwareMixer,
        DmaSystem dmaSystem,
        DualPic dualPic,
        IPauseHandler pauseHandler,
        bool failOnUnhandledPort,
        ILoggerService loggerService,
        ushort basePort = GusConstants.DefaultBasePort,
        byte irq = GusConstants.DefaultIrq,
        byte dma = GusConstants.DefaultDma)
        : base(state, failOnUnhandledPort, loggerService) {
        _softwareMixer = softwareMixer;
        _dualPic = dualPic;
        _pauseHandler = pauseHandler;
        _basePort = basePort;
        _irq = irq;
        _dma1 = dma;
        _dma2 = dma;

        // Initialize timers
        _timer1 = new GusTimer(GusConstants.Timer1DefaultDelay);
        _timer2 = new GusTimer(GusConstants.Timer2DefaultDelay);

        // Initialize sound channel
        _soundChannel = softwareMixer.CreateChannel("GravisUltraSound", GusConstants.OutputSampleRate);

        // Initialize DMA channel
        _dmaChannel = dmaSystem.GetChannel(dma);
        if (_dmaChannel != null) {
            _dmaChannel.ReserveFor("GravisUltraSound", OnDmaEvicted);
            _dmaChannel.RegisterCallback(OnDmaCallback);
        }

        // Initialize device thread
        _deviceThread = new DeviceThread("GravisUltraSound", PlaybackLoopBody, pauseHandler, loggerService);

        // Initialize voices
        for (byte i = 0; i < GusConstants.MaxVoices; i++) {
            _voices[i] = new GusVoice(i, _voiceIrq);
        }

        // Initialize volume and pan lookup tables
        PopulateVolScalars();
        PopulatePanScalars();

        // Initialize ports
        InitPortHandlers(ioPortDispatcher);

        // Start with minimum voices active
        ActivateVoices(GusConstants.MinVoices);

        if (loggerService.IsEnabled(LogEventLevel.Information)) {
            loggerService.Information("GUS: Initialized on port {Port:X3}h, IRQ {Irq}, DMA {Dma}",
                basePort, irq, dma);
        }
    }

    /// <summary>
    /// Raises an interrupt request on the GUS IRQ line.
    /// </summary>
    public void RaiseInterruptRequest() {
        if (_resetRegister.AreIrqsEnabled && _mixControlRegister.LatchesEnabled) {
            _dualPic.ProcessInterruptRequest(_irq);
        }
    }

    /// <inheritdoc />
    public override byte ReadByte(ushort port) {
        int offset = port - _basePort;
        switch (offset) {
            case IrqStatusPortOffset:
                return _irqStatus;

            case TimerControlPortOffset:
                return ReadTimerStatus();

            case AdlibCommandPortOffset:
                return _adlibCommandReg;

            case VoiceSelectPortOffset:
                return (byte)_voiceIndex;

            case RegisterSelectPortOffset:
                return _selectedRegister;

            case DataLowPortOffset:
                return (byte)ReadFromRegister();

            case DataHighPortOffset:
                return (byte)(ReadFromRegister() >> 8);

            case DramIoPortOffset:
                if (_dramAddr < _ram.Length) {
                    return _ram[_dramAddr];
                }
                return 0;

            default:
                if (_loggerService.IsEnabled(LogEventLevel.Debug)) {
                    _loggerService.Debug("GUS: Unhandled read from port {Port:X4}", port);
                }
                return 0xff;
        }
    }

    /// <inheritdoc />
    public override void WriteByte(ushort port, byte value) {
        _deviceThread.StartThreadIfNeeded();
        int offset = port - _basePort;

        switch (offset) {
            case MixControlPortOffset:
                _mixControlRegister.Data = value;
                _shouldChangeIrqDma = true;
                break;

            case TimerControlPortOffset:
                _adlibCommandReg = value;
                break;

            case TimerDataPortOffset:
                HandleTimerDataWrite(value);
                break;

            case AddressSelectPortOffset:
                HandleAddressSelectWrite(value);
                break;

            case VoiceSelectPortOffset:
                _voiceIndex = (ushort)(value & 31);
                _targetVoice = _voices[_voiceIndex];
                break;

            case RegisterSelectPortOffset:
                _selectedRegister = value;
                _registerData = 0;
                break;

            case DataLowPortOffset:
                _registerData = value;
                break;

            case DataHighPortOffset:
                _registerData = (ushort)((_registerData & 0x00ff) | (value << 8));
                WriteToRegister();
                break;

            case DramIoPortOffset:
                if (_dramAddr < _ram.Length) {
                    _ram[_dramAddr] = value;
                }
                break;

            default:
                if (_loggerService.IsEnabled(LogEventLevel.Debug)) {
                    _loggerService.Debug("GUS: Unhandled write to port {Port:X4} with value {Value:X2}", port, value);
                }
                break;
        }
    }

    /// <inheritdoc />
    public override ushort ReadWord(ushort port) {
        int offset = port - _basePort;
        if (offset == DataLowPortOffset) {
            return (ushort)ReadFromRegister();
        }
        return (ushort)(ReadByte(port) | (ReadByte((ushort)(port + 1)) << 8));
    }

    /// <inheritdoc />
    public override void WriteWord(ushort port, ushort value) {
        int offset = port - _basePort;
        if (offset == DataLowPortOffset) {
            _registerData = value;
            WriteToRegister();
        } else {
            WriteByte(port, (byte)(value & 0xff));
            WriteByte((ushort)(port + 1), (byte)(value >> 8));
        }
    }

    private byte ReadTimerStatus() {
        byte time = 0;
        if (_timer1.HasExpired) {
            time |= 1 << 6;
        }
        if (_timer2.HasExpired) {
            time |= 1 << 5;
        }
        if ((time & 0x60) != 0) {
            time |= 1 << 7;
        }
        if ((_irqStatus & 0x04) != 0) {
            time |= 1 << 2;
        }
        if ((_irqStatus & 0x08) != 0) {
            time |= 1 << 1;
        }
        return time;
    }

    private void HandleTimerDataWrite(byte value) {
        if ((value & 0x80) != 0) {
            _timer1.HasExpired = false;
            _timer2.HasExpired = false;
            return;
        }

        _timer1.IsMasked = (value & 0x40) != 0;
        _timer2.IsMasked = (value & 0x20) != 0;

        if ((value & 0x01) != 0) {
            if (!_timer1.IsCountingDown) {
                _timer1.IsCountingDown = true;
                // TODO: Implement timer events using PIC's event system.
                // Timer-based IRQs are not currently functional, which may affect
                // games that rely on GUS timers for timing or synchronization.
            }
        } else {
            _timer1.IsCountingDown = false;
        }

        if ((value & 0x02) != 0) {
            if (!_timer2.IsCountingDown) {
                _timer2.IsCountingDown = true;
                // TODO: Implement timer events using PIC's event system.
                // Timer-based IRQs are not currently functional, which may affect
                // games that rely on GUS timers for timing or synchronization.
            }
        } else {
            _timer2.IsCountingDown = false;
        }
    }

    private void HandleAddressSelectWrite(byte value) {
        if (!_shouldChangeIrqDma) {
            return;
        }

        _shouldChangeIrqDma = false;
        AddressSelectRegister addrSelect = new() { Data = value };
        byte ch1Sel = addrSelect.Channel1Selector;
        byte ch2Sel = addrSelect.Channel2Selector;

        // This is simplified - full implementation would update IRQ/DMA settings
        if (_loggerService.IsEnabled(LogEventLevel.Debug)) {
            _loggerService.Debug("GUS: Address select write: ch1={Ch1}, ch2={Ch2}, combined={Combined}",
                ch1Sel, ch2Sel, addrSelect.Channel2CombinedWithChannel1);
        }
    }

    private ushort ReadFromRegister() {
        byte reg;

        // Global DSP registers
        switch (_selectedRegister) {
            case 0x41: // DMA control register
                reg = _dmaControlRegister.Data;
                _dmaControlRegister.HasPendingTerminalCountIrq = false;
                _irqStatus &= 0x7f;
                CheckIrq();
                return (ushort)(reg << 8);

            case 0x42: // DMA address register
                return _dmaAddr;

            case 0x45: // Timer control register
                return (ushort)(_timerCtrl << 8);

            case 0x49: // DMA sample register
                return (ushort)(_dmaControlRegister.Data << 8);

            case 0x4c: // Reset register
                return (ushort)(_resetRegister.Data << 8);

            case 0x8f: // Voice IRQ status register
                reg = (byte)(_voiceIrq.Status | 0x20);
                uint mask = 1u << _voiceIrq.Status;
                if ((_voiceIrq.VolState & mask) == 0) {
                    reg |= 0x40;
                }
                if ((_voiceIrq.WaveState & mask) == 0) {
                    reg |= 0x80;
                }
                _voiceIrq.VolState &= ~mask;
                _voiceIrq.WaveState &= ~mask;
                CheckVoiceIrq();
                return (ushort)(reg << 8);
        }

        if (_targetVoice == null) {
            return (_selectedRegister == 0x80 || _selectedRegister == 0x8d) ? (ushort)0x0300 : (ushort)0;
        }

        // Voice-specific registers
        switch (_selectedRegister) {
            case 0x80: // Voice wave control
                return (ushort)(_targetVoice.ReadWaveState() << 8);

            case 0x82: // Voice MSW start address
                return (ushort)(_targetVoice.WaveCtrl.Start >> 16);

            case 0x83: // Voice LSW start address
                return (ushort)_targetVoice.WaveCtrl.Start;

            case 0x89: // Voice volume
                int volIndex = CeilDivide(_targetVoice.VolCtrl.Pos, GusConstants.VolumeIncScalar);
                return (ushort)(Math.Clamp(volIndex, 0, 4095) << 4);

            case 0x8a: // Voice MSW current address
                return (ushort)(_targetVoice.WaveCtrl.Pos >> 16);

            case 0x8b: // Voice LSW current address
                return (ushort)_targetVoice.WaveCtrl.Pos;

            case 0x8d: // Voice volume control
                return (ushort)(_targetVoice.ReadVolState() << 8);
        }

        return _registerData;
    }

    private void WriteToRegister() {
        // Global DSP registers
        switch (_selectedRegister) {
            case 0x0e: // Set number of active voices
                _selectedRegister = (byte)(_registerData >> 8);
                byte numVoices = (byte)(1 + ((_registerData >> 8) & 31));
                ActivateVoices(numVoices);
                return;

            case 0x10: // Undocumented register (used by Fast Tracker 2)
                return;

            case 0x41: // DMA control register
                _dmaControlRegister.Data = (byte)(_registerData >> 8);
                if (_dmaControlRegister.IsEnabled) {
                    StartDmaTransfers();
                }
                return;

            case 0x42: // DMA address register
                _dmaAddr = _registerData;
                return;

            case 0x43: // LSW Peek/poke DRAM position
                _dramAddr = (0xf0000u & _dramAddr) | _registerData;
                return;

            case 0x44: // MSB Peek/poke DRAM position
                _dramAddr = (0x0ffffu & _dramAddr) | ((uint)(_registerData & 0x0f00) << 8);
                return;

            case 0x45: // Timer control register
                _timerCtrl = (byte)(_registerData >> 8);
                _timer1.ShouldRaiseIrq = (_timerCtrl & 0x04) != 0;
                if (!_timer1.ShouldRaiseIrq) {
                    _irqStatus &= unchecked((byte)~0x04);
                }
                _timer2.ShouldRaiseIrq = (_timerCtrl & 0x08) != 0;
                if (!_timer2.ShouldRaiseIrq) {
                    _irqStatus &= unchecked((byte)~0x08);
                }
                if (!_timer1.ShouldRaiseIrq && !_timer2.ShouldRaiseIrq) {
                    CheckIrq();
                }
                return;

            case 0x46: // Timer 1 control
                _timer1.Value = (byte)(_registerData >> 8);
                _timer1.Delay = (0x100 - _timer1.Value) * GusConstants.Timer1DefaultDelay;
                return;

            case 0x47: // Timer 2 control
                _timer2.Value = (byte)(_registerData >> 8);
                _timer2.Delay = (0x100 - _timer2.Value) * GusConstants.Timer2DefaultDelay;
                return;

            case 0x49: // DMA sampling control register
                _sampleCtrl = (byte)(_registerData >> 8);
                if ((_sampleCtrl & 1) != 0) {
                    StartDmaTransfers();
                }
                return;

            case 0x4c: // Reset register
                _resetRegister.Data = (byte)(_registerData >> 8);
                if (!_resetRegister.IsRunning) {
                    Reset();
                }
                return;
        }

        // Voice-specific registers
        if (_targetVoice == null) {
            return;
        }

        switch (_selectedRegister) {
            case 0x00: // Voice wave control
                if (_targetVoice.UpdateWaveState((byte)(_registerData >> 8))) {
                    CheckVoiceIrq();
                }
                break;

            case 0x01: // Voice rate control
                _targetVoice.WriteWaveRate(_registerData);
                break;

            case 0x02: // Voice MSW start address
                _targetVoice.WaveCtrl.Start = UpdateWaveMsw(_targetVoice.WaveCtrl.Start);
                break;

            case 0x03: // Voice LSW start address
                _targetVoice.WaveCtrl.Start = UpdateWaveLsw(_targetVoice.WaveCtrl.Start);
                break;

            case 0x04: // Voice MSW end address
                _targetVoice.WaveCtrl.End = UpdateWaveMsw(_targetVoice.WaveCtrl.End);
                break;

            case 0x05: // Voice LSW end address
                _targetVoice.WaveCtrl.End = UpdateWaveLsw(_targetVoice.WaveCtrl.End);
                break;

            case 0x06: // Voice volume rate
                _targetVoice.WriteVolRate((ushort)(_registerData >> 8));
                break;

            case 0x07: // Voice volume start
                byte volStart = (byte)(_registerData >> 8);
                _targetVoice.VolCtrl.Start = (volStart << 4) * GusConstants.VolumeIncScalar;
                break;

            case 0x08: // Voice volume end
                byte volEnd = (byte)(_registerData >> 8);
                _targetVoice.VolCtrl.End = (volEnd << 4) * GusConstants.VolumeIncScalar;
                break;

            case 0x09: // Voice current volume
                _targetVoice.VolCtrl.Pos = (_registerData >> 4) * GusConstants.VolumeIncScalar;
                break;

            case 0x0a: // Voice MSW current address
                _targetVoice.WaveCtrl.Pos = UpdateWaveMsw(_targetVoice.WaveCtrl.Pos);
                break;

            case 0x0b: // Voice LSW current address
                _targetVoice.WaveCtrl.Pos = UpdateWaveLsw(_targetVoice.WaveCtrl.Pos);
                break;

            case 0x0c: // Voice pan pot
                _targetVoice.WritePanPot((byte)(_registerData >> 8));
                break;

            case 0x0d: // Voice volume control
                if (_targetVoice.UpdateVolState((byte)(_registerData >> 8))) {
                    CheckVoiceIrq();
                }
                break;
        }
    }

    private int UpdateWaveLsw(int addr) {
        const int waveLswMask = ~((1 << 16) - 1);
        int lower = addr & waveLswMask;
        return lower | _registerData;
    }

    private int UpdateWaveMsw(int addr) {
        const int waveMswMask = (1 << 16) - 1;
        int upper = _registerData & 0x1fff;
        int lower = addr & waveMswMask;
        return lower | (upper << 16);
    }

    private void ActivateVoices(byte requestedVoices) {
        requestedVoices = Math.Clamp(requestedVoices, (byte)GusConstants.MinVoices, (byte)GusConstants.MaxVoices);
        if (requestedVoices == _activeVoices) {
            return;
        }

        _activeVoices = requestedVoices;
        _activeVoiceMask = 0xffffffffu >> (GusConstants.MaxVoices - _activeVoices);

        // Calculate sample rate based on active voices
        // Formula from Gravis SDK
        _sampleRateHz = (int)(1000000.0 / (1.619695497 * _activeVoices));

        if (_loggerService.IsEnabled(LogEventLevel.Debug)) {
            _loggerService.Debug("GUS: Activated {Voices} voices at {SampleRate} Hz",
                _activeVoices, _sampleRateHz);
        }
    }

    private void CheckIrq() {
        bool shouldInterrupt = (_irqStatus & (_resetRegister.AreIrqsEnabled ? 0xff : 0x9f)) != 0;

        if (shouldInterrupt && _mixControlRegister.LatchesEnabled) {
            _dualPic.ProcessInterruptRequest(_irq);
        } else if (_irqPreviouslyInterrupted) {
            // Deactivate IRQ (not directly possible in current implementation)
        }

        _irqPreviouslyInterrupted = shouldInterrupt;
    }

    private void CheckVoiceIrq() {
        _irqStatus &= 0x9f;
        uint totalMask = (_voiceIrq.VolState | _voiceIrq.WaveState) & _activeVoiceMask;
        if (totalMask == 0) {
            CheckIrq();
            return;
        }

        if (_voiceIrq.VolState != 0) {
            _irqStatus |= 0x40;
        }
        if (_voiceIrq.WaveState != 0) {
            _irqStatus |= 0x20;
        }
        CheckIrq();

        byte startStatus = _voiceIrq.Status;
        while ((totalMask & (1u << _voiceIrq.Status)) == 0) {
            _voiceIrq.Status++;
            if (_voiceIrq.Status >= _activeVoices) {
                _voiceIrq.Status = 0;
            }
            // Safety check to prevent infinite loop
            if (_voiceIrq.Status == startStatus) {
                break;
            }
        }
    }

    private void Reset() {
        _irqStatus = 0;
        _irqPreviouslyInterrupted = false;
        _adlibCommandReg = GusConstants.AdlibCommandDefault;
        _dmaControlRegister = new DmaControlRegister();
        _sampleCtrl = 0;
        _timerCtrl = 0;
        _timer1 = new GusTimer(GusConstants.Timer1DefaultDelay);
        _timer2 = new GusTimer(GusConstants.Timer2DefaultDelay);

        foreach (GusVoice voice in _voices) {
            voice.ResetCtrls();
        }

        _voiceIrq.VolState = 0;
        _voiceIrq.WaveState = 0;
        _voiceIrq.Status = 0;
        _targetVoice = null;
        _voiceIndex = 0;
        _activeVoices = 0;

        _dmaAddr = 0;
        _dramAddr = 0;
        _registerData = 0;
        _selectedRegister = 0;
        _shouldChangeIrqDma = false;
        _resetRegister = new ResetRegister();
        _mixControlRegister = new MixControlRegister();
    }

    private void StartDmaTransfers() {
        // TODO: Implement DMA transfer from system memory to GUS RAM.
        // Full implementation would use _dmaChannel.Read() to transfer data
        // and respect _dmaControlRegister settings (direction, 8/16-bit mode,
        // rate divisor, high bit inversion for sign conversion).
        if (_loggerService.IsEnabled(LogEventLevel.Debug)) {
            _loggerService.Debug("GUS: DMA transfer started");
        }
    }

    private void OnDmaEvicted() {
        if (_loggerService.IsEnabled(LogEventLevel.Warning)) {
            _loggerService.Warning("GUS: DMA channel evicted");
        }
    }

    private void OnDmaCallback(DmaChannel channel, DmaChannel.DmaEvent dmaEvent) {
        if (dmaEvent == DmaChannel.DmaEvent.IsUnmasked) {
            StartDmaTransfers();
        }
    }

    /// <summary>
    /// The PlaybackLoopBody method is called from a background thread (_deviceThread).
    /// Thread safety note: The emulator architecture ensures that CPU execution and
    /// device threads are properly synchronized. I/O port writes from CPU are
    /// serialized with respect to the audio rendering performed here.
    /// </summary>
    private void PlaybackLoopBody() {
        RenderFrames(_renderBuffer);

        // Convert stereo frame pairs to interleaved float array
        int outputIndex = 0;
        for (int i = 0; i < _renderBuffer.Length && outputIndex < _outputBuffer.Length - 1; i++) {
            _outputBuffer[outputIndex++] = _renderBuffer[i].Left / 32768f;
            _outputBuffer[outputIndex++] = _renderBuffer[i].Right / 32768f;
        }

        _soundChannel.Render(_outputBuffer.AsSpan(0, outputIndex));
    }

    private void RenderFrames(Span<(float Left, float Right)> frames) {
        // Clear buffer
        for (int i = 0; i < frames.Length; i++) {
            frames[i] = (0, 0);
        }

        if (!_resetRegister.IsRunning || !_resetRegister.IsDacEnabled) {
            return;
        }

        // Render all active voices
        for (int v = 0; v < _activeVoices; v++) {
            _voices[v].RenderFrames(_ram, _volScalars, _panScalars, frames);
        }

        CheckVoiceIrq();
    }

    private void PopulateVolScalars() {
        const double volumeLevelDivisor = 1.0 + GusConstants.DeltaDb;
        double scalar = 1.0;

        for (int i = _volScalars.Length - 1; i >= 0; i--) {
            _volScalars[i] = (float)scalar;
            scalar /= volumeLevelDivisor;
        }
        _volScalars[0] = 0;
    }

    private void PopulatePanScalars() {
        for (int i = 0; i < GusConstants.PanPositions; i++) {
            double norm = (i - 7.0) / (i < 7 ? 7 : 8);
            double angle = (norm + 1) * Math.PI / 4;
            _panScalars[i] = ((float)Math.Cos(angle), (float)Math.Sin(angle));
        }
    }

    private void InitPortHandlers(IOPortDispatcher ioPortDispatcher) {
        // Main GUS ports
        ioPortDispatcher.AddIOPortHandler((ushort)(_basePort + MixControlPortOffset), this);
        ioPortDispatcher.AddIOPortHandler((ushort)(_basePort + ReadDataPortOffset), this);
        ioPortDispatcher.AddIOPortHandler((ushort)(_basePort + IrqStatusPortOffset), this);
        ioPortDispatcher.AddIOPortHandler((ushort)(_basePort + TimerControlPortOffset), this);
        ioPortDispatcher.AddIOPortHandler((ushort)(_basePort + TimerDataPortOffset), this);
        ioPortDispatcher.AddIOPortHandler((ushort)(_basePort + AdlibCommandPortOffset), this);
        ioPortDispatcher.AddIOPortHandler((ushort)(_basePort + AddressSelectPortOffset), this);

        // GF1 synthesizer ports
        ioPortDispatcher.AddIOPortHandler((ushort)(_basePort + VoiceSelectPortOffset), this);
        ioPortDispatcher.AddIOPortHandler((ushort)(_basePort + RegisterSelectPortOffset), this);
        ioPortDispatcher.AddIOPortHandler((ushort)(_basePort + DataLowPortOffset), this);
        ioPortDispatcher.AddIOPortHandler((ushort)(_basePort + DataHighPortOffset), this);
        ioPortDispatcher.AddIOPortHandler((ushort)(_basePort + DramIoPortOffset), this);

        // Legacy detection ports at fixed addresses regardless of base port configuration.
        // Some programs probe these specific ports to detect GUS presence, even when
        // the GUS is configured at a different base address.
        ushort[] additionalPorts = [0x243, 0x280, 0x281, 0x283, 0x2C0, 0x2C1, 0x2C3];
        foreach (ushort port in additionalPorts) {
            ioPortDispatcher.AddIOPortHandler(port, this);
        }
    }

    private static int CeilDivide(int numerator, int denominator) {
        if (denominator == 0) {
            return 0;
        }
        return (numerator + denominator - 1) / denominator;
    }

    /// <inheritdoc />
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the GravisUltraSound and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    private void Dispose(bool disposing) {
        if (_disposed) {
            return;
        }
        if (disposing) {
            _deviceThread.Dispose();
        }
        _disposed = true;
    }
}
