namespace Spice86.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

using Spice86.Core.Emulator.Devices.Sound;
using Spice86.Core.Emulator.VM;
using Spice86.ViewModels.Services;

/// <summary>
/// ViewModel for observing Gravis UltraSound state in the debugger.
/// Displays information about the GUS sound card.
/// </summary>
public partial class GravisUltraSoundViewModel : DebuggerTabViewModel {
    private readonly GravisUltraSound _gravisUltraSound;

    /// <inheritdoc />
    public override string Header => "Gravis UltraSound";

    /// <inheritdoc />
    public override string? IconKey => "Speaker";

    // GUS Status
    [ObservableProperty]
    private string _cardDescription = "Gravis UltraSound (GUS)";

    [ObservableProperty]
    private string _basePort = "0x220 (default)";

    /// <summary>
    /// Initializes a new instance of the <see cref="GravisUltraSoundViewModel"/> class.
    /// </summary>
    /// <param name="gravisUltraSound">The Gravis UltraSound instance to observe.</param>
    /// <param name="pauseHandler">The pause handler for tracking emulator pause state.</param>
    /// <param name="uiDispatcher">The UI dispatcher for thread-safe UI updates.</param>
    public GravisUltraSoundViewModel(GravisUltraSound gravisUltraSound, IPauseHandler pauseHandler, IUIDispatcher uiDispatcher)
        : base(pauseHandler, uiDispatcher) {
        _gravisUltraSound = gravisUltraSound;
    }

    /// <inheritdoc />
    public override void UpdateValues(object? sender, EventArgs e) {
        if (!IsVisible) {
            return;
        }

        // GUS configuration is mostly static
    }
}
