namespace Spice86.ViewModels;

/// <summary>
/// Wraps a ViewModel to make it a debugger plugin with tab metadata.
/// This allows existing ViewModels to be added as tabs without modification.
/// </summary>
public class DebuggerPluginWrapper : ViewModelBase, IDebuggerPlugin {
    private readonly ViewModelBase _viewModel;

    /// <summary>
    /// Initializes a new instance of the DebuggerPluginWrapper class.
    /// </summary>
    /// <param name="viewModel">The ViewModel to wrap as a plugin.</param>
    /// <param name="tabHeader">The header text for the tab.</param>
    /// <param name="tabTooltip">Optional tooltip text for the tab.</param>
    /// <param name="hotKey">Optional hotkey for quick navigation.</param>
    /// <param name="isVisible">Whether the tab should be visible.</param>
    public DebuggerPluginWrapper(ViewModelBase viewModel, string tabHeader, string? tabTooltip = null, 
        string? hotKey = null, bool isVisible = true) {
        _viewModel = viewModel;
        TabHeader = tabHeader;
        TabTooltip = tabTooltip;
        HotKey = hotKey;
        IsVisible = isVisible;
    }

    /// <inheritdoc/>
    public string TabHeader { get; }

    /// <inheritdoc/>
    public string? TabTooltip { get; }

    /// <inheritdoc/>
    public string? HotKey { get; }

    /// <inheritdoc/>
    public bool IsVisible { get; }

    /// <summary>
    /// Gets the wrapped ViewModel content.
    /// </summary>
    public ViewModelBase Content => _viewModel;
}
