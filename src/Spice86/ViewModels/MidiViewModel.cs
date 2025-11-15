namespace Spice86.ViewModels;

using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;

using Spice86.Core.Emulator.Devices.Sound.Midi;
using Spice86.ViewModels.PropertiesMappers;
using Spice86.ViewModels.Services;
using Spice86.ViewModels.ValueViewModels.Debugging;

/// <summary>
/// Represents midi view model.
/// </summary>
public partial class MidiViewModel : ViewModelBase, IEmulatorObjectViewModel {
    [ObservableProperty]
    private MidiInfo _midi = new();

    private readonly Midi _externalMidiDevice;

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="externalMidiDevice">The external midi device.</param>
    public MidiViewModel(Midi externalMidiDevice) {
        _externalMidiDevice = externalMidiDevice;
        DispatcherTimerStarter.StartNewDispatcherTimer(TimeSpan.FromMilliseconds(400), DispatcherPriority.Background, UpdateValues);
    }

    /// <summary>
    /// Gets or sets is visible.
    /// </summary>
    public bool IsVisible { get; set; }


    /// <summary>
    /// Updates values.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The e.</param>
    public void UpdateValues(object? sender, EventArgs e) {
        if (!IsVisible) {
            return;
        }
        _externalMidiDevice.CopyToMidiInfo(Midi);
    }
}