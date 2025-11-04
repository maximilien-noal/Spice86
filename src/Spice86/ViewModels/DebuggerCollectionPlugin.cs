namespace Spice86.ViewModels;

using Avalonia.Collections;

/// <summary>
/// Represents a debugger plugin that contains a collection of related tabs.
/// Used for tabs like "Memory" and "Disassembly" that have multiple sub-tabs.
/// </summary>
public class DebuggerCollectionPlugin : ViewModelBase, IDebuggerPlugin {
    /// <summary>
    /// Initializes a new instance of the DebuggerCollectionPlugin class.
    /// </summary>
    /// <param name="tabHeader">The header text for the main tab.</param>
    /// <param name="items">The collection of items to display as sub-tabs.</param>
    /// <param name="tabTooltip">Optional tooltip text for the tab.</param>
    /// <param name="hotKey">Optional hotkey for quick navigation.</param>
    /// <param name="isVisible">Whether the tab should be visible.</param>
    public DebuggerCollectionPlugin(string tabHeader, AvaloniaList<ViewModelBase> items,
        string? tabTooltip = null, string? hotKey = null, bool isVisible = true) {
        TabHeader = tabHeader;
        Items = items;
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
    /// Gets the collection of items (ViewModels) to display as sub-tabs.
    /// </summary>
    public AvaloniaList<ViewModelBase> Items { get; }
}
