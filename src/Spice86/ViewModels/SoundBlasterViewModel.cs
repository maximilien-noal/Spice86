namespace Spice86.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

using Spice86.Core.Emulator.Devices.Sound.Blaster;
using Spice86.Core.Emulator.VM;
using Spice86.ViewModels.Services;

/// <summary>
/// ViewModel for observing Sound Blaster state in the debugger.
/// Displays information about the Sound Blaster card configuration and status.
/// </summary>
public partial class SoundBlasterViewModel : DebuggerTabViewModel {
    private readonly SoundBlaster _soundBlaster;

    /// <inheritdoc />
    public override string Header => "Sound Blaster";

    /// <inheritdoc />
    public override string? IconKey => "Speaker";

    // Card info
    [ObservableProperty]
    private string _blasterString = string.Empty;

    [ObservableProperty]
    private string _sbType = string.Empty;

    [ObservableProperty]
    private byte _irq;

    // DMA info
    [ObservableProperty]
    private string _dmaPlaybackMode = string.Empty;

    [ObservableProperty]
    private bool _autoInit;

    [ObservableProperty]
    private bool _stereo;

    [ObservableProperty]
    private string _sampleRate = string.Empty;

    [ObservableProperty]
    private string _remainingBytes = string.Empty;

    [ObservableProperty]
    private bool _speakerEnabled;

    [ObservableProperty]
    private bool _dmaMasked;

    /// <summary>
    /// Initializes a new instance of the <see cref="SoundBlasterViewModel"/> class.
    /// </summary>
    /// <param name="soundBlaster">The Sound Blaster instance to observe.</param>
    /// <param name="pauseHandler">The pause handler for tracking emulator pause state.</param>
    /// <param name="uiDispatcher">The UI dispatcher for thread-safe UI updates.</param>
    public SoundBlasterViewModel(SoundBlaster soundBlaster, IPauseHandler pauseHandler, IUIDispatcher uiDispatcher)
        : base(pauseHandler, uiDispatcher) {
        _soundBlaster = soundBlaster;
        BlasterString = soundBlaster.BlasterString;
        SbType = soundBlaster.SbType.ToString();
        Irq = soundBlaster.IRQ;
    }

    /// <inheritdoc />
    public override void UpdateValues(object? sender, EventArgs e) {
        if (!IsVisible) {
            return;
        }

        // The DMA state properties are internal, so we can only show the static config
        // In a real implementation, we would expose more internal state
        BlasterString = _soundBlaster.BlasterString;
    }
}
