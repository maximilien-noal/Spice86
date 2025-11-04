namespace Spice86.ViewModels;

using Avalonia.Collections;

/// <summary>
/// Represents a debugger plugin for devices that contains multiple device-related tabs.
/// Used for the "Devices" tab which contains Video Card, Palette, MIDI, etc.
/// </summary>
public class DebuggerDevicesPlugin : ViewModelBase, IDebuggerPlugin {
    /// <summary>
    /// Initializes a new instance of the DebuggerDevicesPlugin class.
    /// </summary>
    /// <param name="tabHeader">The header text for the main tab.</param>
    /// <param name="devicePlugins">The collection of device plugins to display as sub-tabs.</param>
    /// <param name="tabTooltip">Optional tooltip text for the tab.</param>
    /// <param name="hotKey">Optional hotkey for quick navigation.</param>
    /// <param name="isVisible">Whether the tab should be visible.</param>
    public DebuggerDevicesPlugin(string tabHeader, AvaloniaList<IDebuggerPlugin> devicePlugins,
        string? tabTooltip = null, string? hotKey = null, bool isVisible = true) {
        TabHeader = tabHeader;
        DevicePlugins = devicePlugins;
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
    /// Gets the collection of device plugins to display as sub-tabs.
    /// </summary>
    public AvaloniaList<IDebuggerPlugin> DevicePlugins { get; }
}
