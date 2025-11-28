namespace Spice86.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

using Spice86.Core.Emulator.Devices.Sound;
using Spice86.Core.Emulator.VM;
using Spice86.ViewModels.Services;

/// <summary>
/// ViewModel for observing OPL3 FM Synthesizer state in the debugger.
/// Displays information about the OPL3/AdLib FM synthesis chip.
/// </summary>
public partial class Opl3ViewModel : DebuggerTabViewModel {
    private readonly Opl3Fm _opl3Fm;

    /// <inheritdoc />
    public override string Header => "OPL3/FM";

    /// <inheritdoc />
    public override string? IconKey => "MusicNote";

    // OPL3 Status
    [ObservableProperty]
    private string _chipDescription = "OPL3 FM Synthesizer (Yamaha YMF262)";

    [ObservableProperty]
    private string _basePort = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="Opl3ViewModel"/> class.
    /// </summary>
    /// <param name="opl3Fm">The OPL3 FM instance to observe.</param>
    /// <param name="pauseHandler">The pause handler for tracking emulator pause state.</param>
    /// <param name="uiDispatcher">The UI dispatcher for thread-safe UI updates.</param>
    public Opl3ViewModel(Opl3Fm opl3Fm, IPauseHandler pauseHandler, IUIDispatcher uiDispatcher)
        : base(pauseHandler, uiDispatcher) {
        _opl3Fm = opl3Fm;
        BasePort = "0x388 (AdLib compatible)";
    }

    /// <inheritdoc />
    public override void UpdateValues(object? sender, EventArgs e) {
        if (!IsVisible) {
            return;
        }

        // OPL3 FM configuration is mostly static
    }
}
