namespace Spice86.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

/// <summary>
/// The class.
/// </summary>
public partial class BreakpointTypeTabItemViewModel : ViewModelBase {
    [ObservableProperty]
    private string? _header;

    [ObservableProperty]
    private bool _isSelected;
}