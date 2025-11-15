namespace Spice86.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

/// <summary>
/// Represents breakpoint type tab item view model.
/// </summary>
public partial class BreakpointTypeTabItemViewModel : ViewModelBase {
    [ObservableProperty]
    private string? _header;

    [ObservableProperty]
    private bool _isSelected;
}