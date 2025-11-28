namespace Spice86.ViewModels;

using Avalonia.Collections;

using CommunityToolkit.Mvvm.ComponentModel;

using Spice86.Core.Emulator.Devices.Timer;
using Spice86.Core.Emulator.VM;
using Spice86.ViewModels.Services;

/// <summary>
/// ViewModel for observing the Programmable Interval Timer (PIT) state in the debugger.
/// Displays information about all three PIT channels and timing.
/// </summary>
public partial class TimerViewModel : DebuggerTabViewModel {
    private readonly PitTimer _pitTimer;

    /// <inheritdoc />
    public override string Header => "Timer";

    /// <inheritdoc />
    public override string? IconKey => "Clock";

    // System timing
    [ObservableProperty]
    private string _systemStartTime = string.Empty;

    [ObservableProperty]
    private string _currentTicks = string.Empty;

    [ObservableProperty]
    private string _ticksUs = string.Empty;

    // PIT constants
    [ObservableProperty]
    private int _pitTickRate = PitTimer.PitTickRate;

    // Channel states
    [ObservableProperty]
    private AvaloniaList<PitChannelInfo> _channels = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TimerViewModel"/> class.
    /// </summary>
    /// <param name="pitTimer">The PIT timer instance to observe.</param>
    /// <param name="pauseHandler">The pause handler for tracking emulator pause state.</param>
    /// <param name="uiDispatcher">The UI dispatcher for thread-safe UI updates.</param>
    public TimerViewModel(PitTimer pitTimer, IPauseHandler pauseHandler, IUIDispatcher uiDispatcher)
        : base(pauseHandler, uiDispatcher) {
        _pitTimer = pitTimer;
    }

    /// <inheritdoc />
    public override void UpdateValues(object? sender, EventArgs e) {
        if (!IsVisible) {
            return;
        }

        // Read values from the Core on each update
        SystemStartTime = _pitTimer.SystemStartTime.ToString("yyyy-MM-dd HH:mm:ss");
        CurrentTicks = $"{_pitTimer.GetTicks():N0}";
        TicksUs = $"{_pitTimer.GetTicksUs():N0} µs";

        UpdateChannels();
    }

    private void UpdateChannels() {
        if (Channels.Count == 0) {
            for (int i = 0; i < 3; i++) {
                Channels.Add(new PitChannelInfo { ChannelNumber = i });
            }
        }

        for (int i = 0; i < 3; i++) {
            PitChannelSnapshot snapshot = _pitTimer.GetChannelSnapshot(i);
            Channels[i].ReadLatch = $"0x{snapshot.ReadLatch:X4}";
            Channels[i].WriteLatch = $"0x{snapshot.WriteLatch:X4}";
            Channels[i].Mode = snapshot.Mode.ToString();
            Channels[i].IsCounting = snapshot.Counting;
            Channels[i].ModeChanged = snapshot.ModeChanged;
        }
    }

    /// <summary>
    /// Information about a PIT channel.
    /// </summary>
    public partial class PitChannelInfo : ObservableObject {
        /// <summary>
        /// Gets or sets the channel number (0-2).
        /// </summary>
        [ObservableProperty]
        private int _channelNumber;

        /// <summary>
        /// Gets or sets the read latch value.
        /// </summary>
        [ObservableProperty]
        private string _readLatch = string.Empty;

        /// <summary>
        /// Gets or sets the write latch value.
        /// </summary>
        [ObservableProperty]
        private string _writeLatch = string.Empty;

        /// <summary>
        /// Gets or sets the channel mode.
        /// </summary>
        [ObservableProperty]
        private string _mode = string.Empty;

        /// <summary>
        /// Gets or sets whether the channel is counting.
        /// </summary>
        [ObservableProperty]
        private bool _isCounting;

        /// <summary>
        /// Gets or sets whether mode was changed.
        /// </summary>
        [ObservableProperty]
        private bool _modeChanged;
    }
}
