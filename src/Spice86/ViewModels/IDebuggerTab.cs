namespace Spice86.ViewModels;

/// <summary>
/// Contract for debugger tabs that can be dynamically registered in the debug window.
/// This interface enables plugin-based integration of new debugger tabs for observing
/// various emulator subsystems (DOS, BIOS, XMS, EMS, MCP Server, etc.).
/// </summary>
public interface IDebuggerTab : IEmulatorObjectViewModel {
    /// <summary>
    /// Gets the display header for this tab in the debugger UI.
    /// </summary>
    string Header { get; }

    /// <summary>
    /// Gets the icon identifier for this tab. Can be null if no icon is desired.
    /// </summary>
    string? IconKey { get; }

    /// <summary>
    /// Gets whether this tab should be enabled/visible based on emulator configuration.
    /// </summary>
    bool IsEnabled { get; }
}
