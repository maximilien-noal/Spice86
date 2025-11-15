namespace Spice86.ViewModels.ValueViewModels.Debugging;

using CommunityToolkit.Mvvm.ComponentModel;

using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents function info.
/// </summary>
public partial class FunctionInfo : ObservableObject {
    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    [ObservableProperty] private string? _name;
    [ObservableProperty] private SegmentedAddress _address;

    public override string ToString() {
        return $"{Address}: {Name}";
    }
}