namespace Spice86.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

using Spice86.Core.Emulator.Devices.Sound;
using Spice86.Core.Emulator.VM;
using Spice86.Libs.Sound.Devices.NukedOpl3;
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

    // OPL3 Status - read from Core constants
    [ObservableProperty]
    private ushort _primaryAddressPort;

    [ObservableProperty]
    private ushort _primaryDataPort;

    [ObservableProperty]
    private ushort _secondaryAddressPort;

    [ObservableProperty]
    private ushort _secondaryDataPort;

    [ObservableProperty]
    private ushort _adLibGoldAddressPort;

    [ObservableProperty]
    private ushort _adLibGoldDataPort;

    /// <summary>
    /// Initializes a new instance of the <see cref="Opl3ViewModel"/> class.
    /// </summary>
    /// <param name="opl3Fm">The OPL3 FM instance to observe.</param>
    /// <param name="pauseHandler">The pause handler for tracking emulator pause state.</param>
    /// <param name="uiDispatcher">The UI dispatcher for thread-safe UI updates.</param>
    public Opl3ViewModel(Opl3Fm opl3Fm, IPauseHandler pauseHandler, IUIDispatcher uiDispatcher)
        : base(pauseHandler, uiDispatcher) {
        _opl3Fm = opl3Fm;
    }

    /// <inheritdoc />
    public override void UpdateValues(object? sender, EventArgs e) {
        if (!IsVisible) {
            return;
        }

        // Read port values from the Core's OPL port constants
        PrimaryAddressPort = IOplPort.PrimaryAddressPortNumber;
        PrimaryDataPort = IOplPort.PrimaryDataPortNumber;
        SecondaryAddressPort = IOplPort.SecondaryAddressPortNumber;
        SecondaryDataPort = IOplPort.SecondaryDataPortNumber;
        AdLibGoldAddressPort = IOplPort.AdLibGoldAddressPortNumber;
        AdLibGoldDataPort = IOplPort.AdLibGoldDataPortNumber;
    }
}
