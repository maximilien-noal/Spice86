namespace Spice86.ViewModels.ValueViewModels.Debugging;

using CommunityToolkit.Mvvm.ComponentModel;

using System.ComponentModel;

/// <summary>
/// Represents midi info.
/// </summary>
public partial class MidiInfo : ObservableObject {
    [ObservableProperty, ReadOnly(true)] private int _lastPortRead;
    [ObservableProperty, ReadOnly(true)] private int _lastPortWritten;
    [ObservableProperty, ReadOnly(true)] private uint _lastPortWrittenValue;
}