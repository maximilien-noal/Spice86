namespace Spice86.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

using Spice86.Core.Emulator.VM;
using Spice86.ViewModels.Services;

/// <summary>
/// ViewModel for observing GDB Server state in the debugger.
/// Displays information about the GDB remote debugging server.
/// </summary>
public partial class GdbServerViewModel : DebuggerTabViewModel {
    private readonly int _gdbPort;
    private readonly bool _isEnabled;

    /// <inheritdoc />
    public override string Header => "GDB Server";

    /// <inheritdoc />
    public override string? IconKey => "Bug";

    // GDB Server Status
    [ObservableProperty]
    private bool _isServerEnabled;

    [ObservableProperty]
    private int _port;

    [ObservableProperty]
    private string _serverStatus = string.Empty;

    [ObservableProperty]
    private string _connectionInfo = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="GdbServerViewModel"/> class.
    /// </summary>
    /// <param name="gdbPort">The GDB server port (0 if disabled).</param>
    /// <param name="pauseHandler">The pause handler for tracking emulator pause state.</param>
    /// <param name="uiDispatcher">The UI dispatcher for thread-safe UI updates.</param>
    public GdbServerViewModel(int gdbPort, IPauseHandler pauseHandler, IUIDispatcher uiDispatcher)
        : base(pauseHandler, uiDispatcher) {
        _gdbPort = gdbPort;
        _isEnabled = gdbPort > 0;
    }

    /// <inheritdoc />
    public override void UpdateValues(object? sender, EventArgs e) {
        if (!IsVisible) {
            return;
        }

        // Read values from configuration on each update
        IsServerEnabled = _isEnabled;
        Port = _gdbPort;
        ServerStatus = _isEnabled ? "Running" : "Disabled";
        ConnectionInfo = _isEnabled ? $"Connect with: target remote localhost:{_gdbPort}" : "GDB server is disabled";
    }
}
