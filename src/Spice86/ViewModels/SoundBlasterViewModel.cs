namespace Spice86.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

using Spice86.Core.Emulator.Devices.Sound.Blaster;
using Spice86.Core.Emulator.VM;
using Spice86.ViewModels.Services;

using SbTypeEnum = Spice86.Core.Emulator.Devices.Sound.Blaster.SbType;

/// <summary>
/// ViewModel for observing Sound Blaster state in the debugger.
/// Displays comprehensive information about the Sound Blaster card for reverse engineering.
/// </summary>
public partial class SoundBlasterViewModel : DebuggerTabViewModel {
    private readonly SoundBlaster _soundBlaster;

    /// <inheritdoc />
    public override string Header => "Sound Blaster";

    /// <inheritdoc />
    public override string? IconKey => "Speaker";

    // Card Configuration
    [ObservableProperty]
    private string _blasterString = string.Empty;

    [ObservableProperty]
    private string _sbType = string.Empty;

    [ObservableProperty]
    private string _sbTypeDescription = string.Empty;

    [ObservableProperty]
    private byte _irq;

    [ObservableProperty]
    private string _baseAddress = string.Empty;

    [ObservableProperty]
    private byte _lowDma;

    [ObservableProperty]
    private byte _highDma;

    // DSP Status
    [ObservableProperty]
    private string _dspVersion = string.Empty;

    [ObservableProperty]
    private int _sampleRate;

    [ObservableProperty]
    private int _blockTransferSize;

    [ObservableProperty]
    private bool _is16Bit;

    [ObservableProperty]
    private bool _isStereo;

    [ObservableProperty]
    private bool _isDmaTransferActive;

    // I/O Port Addresses
    [ObservableProperty]
    private string _resetPort = string.Empty;

    [ObservableProperty]
    private string _readDataPort = string.Empty;

    [ObservableProperty]
    private string _writeDataPort = string.Empty;

    [ObservableProperty]
    private string _readStatusPort = string.Empty;

    [ObservableProperty]
    private string _mixerAddressPort = string.Empty;

    [ObservableProperty]
    private string _mixerDataPort = string.Empty;

    [ObservableProperty]
    private string _mpu401DataPort = string.Empty;

    [ObservableProperty]
    private string _mpu401StatusPort = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="SoundBlasterViewModel"/> class.
    /// </summary>
    /// <param name="soundBlaster">The Sound Blaster instance to observe.</param>
    /// <param name="pauseHandler">The pause handler for tracking emulator pause state.</param>
    /// <param name="uiDispatcher">The UI dispatcher for thread-safe UI updates.</param>
    public SoundBlasterViewModel(SoundBlaster soundBlaster, IPauseHandler pauseHandler, IUIDispatcher uiDispatcher)
        : base(pauseHandler, uiDispatcher) {
        _soundBlaster = soundBlaster;
    }

    /// <inheritdoc />
    public override void UpdateValues(object? sender, EventArgs e) {
        if (!IsVisible) {
            return;
        }

        // Card Configuration
        BlasterString = _soundBlaster.BlasterString;
        SbType = _soundBlaster.SbType.ToString();
        SbTypeDescription = GetSbTypeDescription(_soundBlaster.SbType);
        Irq = _soundBlaster.IRQ;

        // Parse BLASTER string for additional info
        string blaster = _soundBlaster.BlasterString;
        if (!string.IsNullOrEmpty(blaster)) {
            // Parse A (base address), D (low DMA), H (high DMA) from BLASTER string
            foreach (string part in blaster.Split(' ')) {
                if (part.Length > 0) {
                    char prefix = part[0];
                    string value = part[1..];
                    switch (prefix) {
                        case 'A':
                            BaseAddress = $"0x{value}";
                            break;
                        case 'D':
                            if (byte.TryParse(value, out byte lowDma)) {
                                LowDma = lowDma;
                            }

                            break;
                        case 'H':
                            if (byte.TryParse(value, out byte highDma)) {
                                HighDma = highDma;
                            }

                            break;
                    }
                }
            }
        }

        // DSP Version based on SB type
        DspVersion = GetDspVersion(_soundBlaster.SbType);

        // I/O Port Addresses (based on standard Sound Blaster layout)
        // Base address + offset
        if (ushort.TryParse(BaseAddress.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber, null, out ushort baseAddr)) {
            ResetPort = $"0x{baseAddr + 0x06:X3}";
            ReadDataPort = $"0x{baseAddr + 0x0A:X3}";
            WriteDataPort = $"0x{baseAddr + 0x0C:X3}";
            ReadStatusPort = $"0x{baseAddr + 0x0E:X3}";
            MixerAddressPort = $"0x{baseAddr + 0x04:X3}";
            MixerDataPort = $"0x{baseAddr + 0x05:X3}";
            Mpu401DataPort = $"0x{baseAddr + 0xE0:X3}";
            Mpu401StatusPort = $"0x{baseAddr + 0xE1:X3}";
        }
    }

    private static string GetSbTypeDescription(SbTypeEnum sbType) {
        return sbType switch {
            SbTypeEnum.SbPro1 => "Sound Blaster Pro (DSP 3.00)",
            SbTypeEnum.SbPro2 => "Sound Blaster Pro 2.0 (DSP 3.02)",
            SbTypeEnum.Sb16 => "Sound Blaster 16 (DSP 4.05)",
            _ => "Unknown"
        };
    }

    private static string GetDspVersion(SbTypeEnum sbType) {
        return sbType switch {
            SbTypeEnum.SbPro1 => "3.00",
            SbTypeEnum.SbPro2 => "3.02",
            SbTypeEnum.Sb16 => "4.05",
            _ => "Unknown"
        };
    }
}
