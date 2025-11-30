namespace Spice86.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

using Spice86.Core.Emulator.VM;
using Spice86.ViewModels.Services;

/// <summary>
/// Base class for debugger tab view models that provides common functionality
/// including visibility tracking, pause state handling, and periodic updates.
/// </summary>
public abstract partial class DebuggerTabViewModel : ViewModelBase, IDebuggerTab {
    private readonly IUIDispatcher _uiDispatcher;

    /// <summary>
    /// Gets or sets whether this tab is currently visible in the UI.
    /// Used to optimize updates - tabs that aren't visible don't update their data.
    /// </summary>
    public bool IsVisible { get; set; }

    /// <inheritdoc />
    public abstract string Header { get; }

    /// <inheritdoc />
    public virtual string? IconKey => null;

    /// <inheritdoc />
    [ObservableProperty]
    private bool _isEnabled = true;

    /// <summary>
    /// Gets whether the emulator is currently paused.
    /// </summary>
    [ObservableProperty]
    private bool _isPaused;

    /// <summary>
    /// Initializes a new instance of the <see cref="DebuggerTabViewModel"/> class.
    /// </summary>
    /// <param name="pauseHandler">The pause handler for tracking emulator pause state.</param>
    /// <param name="uiDispatcher">The UI dispatcher for thread-safe UI updates.</param>
    protected DebuggerTabViewModel(IPauseHandler pauseHandler, IUIDispatcher uiDispatcher) {
        _uiDispatcher = uiDispatcher;
        IsPaused = pauseHandler.IsPaused;

        pauseHandler.Paused += () => uiDispatcher.Post(() => IsPaused = true);
        pauseHandler.Resumed += () => uiDispatcher.Post(() => IsPaused = false);
        // Timer is started by the View code-behind when the View is attached to visual tree
        // This follows the same pattern as CpuView/CpuViewModel
    }

    /// <inheritdoc />
    public abstract void UpdateValues(object? sender, EventArgs e);
}
