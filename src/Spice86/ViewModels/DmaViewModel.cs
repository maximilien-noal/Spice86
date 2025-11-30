namespace Spice86.ViewModels;

using Avalonia.Collections;

using CommunityToolkit.Mvvm.ComponentModel;

using Spice86.Core.Emulator.Devices.DirectMemoryAccess;
using Spice86.Core.Emulator.VM;
using Spice86.ViewModels.Services;

/// <summary>
/// ViewModel for observing DMA (Direct Memory Access) system state in the debugger.
/// Displays comprehensive information about the DMA controllers and channels for reverse engineering.
/// </summary>
public partial class DmaViewModel : DebuggerTabViewModel {
    private readonly DmaSystem _dmaSystem;

    /// <inheritdoc />
    public override string Header => "DMA";

    /// <inheritdoc />
    public override string? IconKey => "ArrowSwap";

    // Controller Info
    [ObservableProperty]
    private string _controller1Description = "Controller 1 (8-bit, Channels 0-3)";

    [ObservableProperty]
    private string _controller2Description = "Controller 2 (16-bit, Channels 4-7)";

    // DMA Channels
    [ObservableProperty]
    private AvaloniaList<DmaChannelInfo> _channels = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DmaViewModel"/> class.
    /// </summary>
    /// <param name="dmaSystem">The DMA system instance to observe.</param>
    /// <param name="pauseHandler">The pause handler for tracking emulator pause state.</param>
    /// <param name="uiDispatcher">The UI dispatcher for thread-safe UI updates.</param>
    public DmaViewModel(DmaSystem dmaSystem, IPauseHandler pauseHandler, IUIDispatcher uiDispatcher)
        : base(pauseHandler, uiDispatcher) {
        _dmaSystem = dmaSystem;

        // Initialize 8 DMA channels (0-3 for 8-bit, 4-7 for 16-bit)
        for (int i = 0; i < 8; i++) {
            string channelType = i < 4 ? "8-bit" : "16-bit";
            string channelDescription = i switch {
                0 => "Memory Refresh (typically)",
                1 => "Sound Blaster (typical)",
                2 => "Floppy Disk Controller",
                3 => "Available",
                4 => "Cascade (to controller 1)",
                5 => "Sound Blaster 16 (typical)",
                6 => "Available",
                7 => "Available",
                _ => "Unknown"
            };
            Channels.Add(new DmaChannelInfo {
                ChannelNumber = i,
                Type = channelType,
                TypicalUsage = channelDescription,
                ChannelNumberDisplay = $"Channel {i}"
            });
        }
    }

    /// <inheritdoc />
    public override void UpdateValues(object? sender, EventArgs e) {
        if (!IsVisible) {
            return;
        }

        for (int i = 0; i < 8; i++) {
            DmaChannel? channel = _dmaSystem.GetChannel((byte)i);
            if (channel != null) {
                Channels[i].IsConfigured = true;
                Channels[i].ChannelNumberDisplay = $"Channel {channel.ChannelNumber}";
                Channels[i].Status = "Configured";
            } else {
                Channels[i].IsConfigured = false;
                Channels[i].Status = "Not Configured";
            }
        }
    }

    /// <summary>
    /// Information about a DMA channel.
    /// </summary>
    public partial class DmaChannelInfo : ObservableObject {
        /// <summary>
        /// Gets or sets the channel number.
        /// </summary>
        [ObservableProperty]
        private int _channelNumber;

        /// <summary>
        /// Gets or sets the channel number display string.
        /// </summary>
        [ObservableProperty]
        private string _channelNumberDisplay = string.Empty;

        /// <summary>
        /// Gets or sets the channel type (8-bit or 16-bit).
        /// </summary>
        [ObservableProperty]
        private string _type = string.Empty;

        /// <summary>
        /// Gets or sets the typical usage description.
        /// </summary>
        [ObservableProperty]
        private string _typicalUsage = string.Empty;

        /// <summary>
        /// Gets or sets whether the channel is configured.
        /// </summary>
        [ObservableProperty]
        private bool _isConfigured;

        /// <summary>
        /// Gets or sets the channel status.
        /// </summary>
        [ObservableProperty]
        private string _status = string.Empty;
    }
}
