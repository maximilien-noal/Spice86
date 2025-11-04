namespace Spice86.ViewModels;

/// <summary>
/// Interface for plugins that can be added as tabs to the DebugWindow.
/// Implementing this interface allows view models to be automatically 
/// displayed as rotated tabs in the debug window's tab control.
/// </summary>
public interface IDebuggerPlugin {
    /// <summary>
    /// Gets the header text displayed on the tab (will be rotated -90 degrees).
    /// </summary>
    string TabHeader { get; }

    /// <summary>
    /// Gets the tooltip text for the tab.
    /// </summary>
    string? TabTooltip { get; }

    /// <summary>
    /// Gets the hotkey string for quick navigation to this tab (e.g., "Alt+F1").
    /// </summary>
    string? HotKey { get; }

    /// <summary>
    /// Gets a value indicating whether this plugin tab should be visible.
    /// Can be used to conditionally show/hide tabs based on configuration.
    /// </summary>
    bool IsVisible { get; }
}
