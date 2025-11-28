namespace Spice86.ViewModels;

using Avalonia.Threading;

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
    /// <param name="updateInterval">The interval between updates in milliseconds. Default is 400ms.</param>
    protected DebuggerTabViewModel(IPauseHandler pauseHandler, IUIDispatcher uiDispatcher, int updateInterval = 400) {
        _uiDispatcher = uiDispatcher;
        IsPaused = pauseHandler.IsPaused;

        pauseHandler.Paused += () => uiDispatcher.Post(() => IsPaused = true);
        pauseHandler.Resumed += () => uiDispatcher.Post(() => IsPaused = false);

        DispatcherTimerStarter.StartNewDispatcherTimer(
            TimeSpan.FromMilliseconds(updateInterval),
            DispatcherPriority.Background,
            UpdateValues);
    }

    /// <inheritdoc />
    public abstract void UpdateValues(object? sender, EventArgs e);
}
