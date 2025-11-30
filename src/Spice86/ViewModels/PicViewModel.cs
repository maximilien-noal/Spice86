namespace Spice86.ViewModels;

using Avalonia.Collections;

using CommunityToolkit.Mvvm.ComponentModel;

using Spice86.Core.Emulator.Devices.ExternalInput;
using Spice86.Core.Emulator.VM;
using Spice86.ViewModels.Services;

/// <summary>
/// ViewModel for observing the Dual Programmable Interrupt Controller (PIC) state in the debugger.
/// Displays information about both PIC controllers (master and slave).
/// </summary>
public partial class PicViewModel : DebuggerTabViewModel {
    private readonly DualPic _dualPic;

    /// <inheritdoc />
    public override string Header => "PIC";

    /// <inheritdoc />
    public override string? IconKey => "Lightning";

    // General PIC state
    [ObservableProperty]
    private bool _irqCheck;

    [ObservableProperty]
    private string _ticks = string.Empty;

    // Master PIC (PIC1)
    [ObservableProperty]
    private PicControllerInfo _masterPic = new() { Name = "Master PIC (IRQ 0-7)" };

    // Slave PIC (PIC2)
    [ObservableProperty]
    private PicControllerInfo _slavePic = new() { Name = "Slave PIC (IRQ 8-15)" };

    // IRQ Status list
    [ObservableProperty]
    private AvaloniaList<IrqStatusInfo> _irqStatuses = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PicViewModel"/> class.
    /// </summary>
    /// <param name="dualPic">The dual PIC instance to observe.</param>
    /// <param name="pauseHandler">The pause handler for tracking emulator pause state.</param>
    /// <param name="uiDispatcher">The UI dispatcher for thread-safe UI updates.</param>
    public PicViewModel(DualPic dualPic, IPauseHandler pauseHandler, IUIDispatcher uiDispatcher)
        : base(pauseHandler, uiDispatcher) {
        _dualPic = dualPic;

        // Initialize IRQ statuses
        for (int i = 0; i < 16; i++) {
            IrqStatuses.Add(new IrqStatusInfo { IrqNumber = i });
        }
    }

    /// <inheritdoc />
    public override void UpdateValues(object? sender, EventArgs e) {
        if (!IsVisible) {
            return;
        }

        IrqCheck = _dualPic.IrqCheck;
        Ticks = $"{_dualPic.Ticks:N0}";

        UpdatePicController(MasterPic, DualPic.PicController.Primary);
        UpdatePicController(SlavePic, DualPic.PicController.Secondary);
        UpdateIrqStatuses();
    }

    private void UpdatePicController(PicControllerInfo info, DualPic.PicController controller) {
        PicSnapshot snapshot = _dualPic.GetPicSnapshot(controller);
        info.InterruptRequestRegister = $"0x{snapshot.InterruptRequestRegister:X2}";
        info.InterruptMaskRegister = $"0x{snapshot.InterruptMaskRegister:X2}";
        info.InServiceRegister = $"0x{snapshot.InServiceRegister:X2}";
        info.ActiveIrqLine = snapshot.ActiveIrqLine == 8 ? "Idle" : $"IRQ {snapshot.ActiveIrqLine}";
        info.InterruptVectorBase = $"0x{snapshot.InterruptVectorBase:X2}";
        info.IsAutoEoi = snapshot.IsAutoEndOfInterruptEnabled;
        info.IsSpecialMaskMode = snapshot.IsSpecialMaskModeEnabled;
    }

    private void UpdateIrqStatuses() {
        for (int i = 0; i < 16; i++) {
            IrqStatuses[i].IsMasked = _dualPic.IsInterruptMasked((byte)i);
        }
    }

    /// <summary>
    /// Information about a PIC controller.
    /// </summary>
    public partial class PicControllerInfo : ObservableObject {
        /// <summary>
        /// Gets or sets the controller name.
        /// </summary>
        [ObservableProperty]
        private string _name = string.Empty;

        /// <summary>
        /// Gets or sets the Interrupt Request Register (IRR).
        /// </summary>
        [ObservableProperty]
        private string _interruptRequestRegister = string.Empty;

        /// <summary>
        /// Gets or sets the Interrupt Mask Register (IMR).
        /// </summary>
        [ObservableProperty]
        private string _interruptMaskRegister = string.Empty;

        /// <summary>
        /// Gets or sets the In-Service Register (ISR).
        /// </summary>
        [ObservableProperty]
        private string _inServiceRegister = string.Empty;

        /// <summary>
        /// Gets or sets the active IRQ line.
        /// </summary>
        [ObservableProperty]
        private string _activeIrqLine = string.Empty;

        /// <summary>
        /// Gets or sets the interrupt vector base.
        /// </summary>
        [ObservableProperty]
        private string _interruptVectorBase = string.Empty;

        /// <summary>
        /// Gets or sets whether auto EOI is enabled.
        /// </summary>
        [ObservableProperty]
        private bool _isAutoEoi;

        /// <summary>
        /// Gets or sets whether special mask mode is enabled.
        /// </summary>
        [ObservableProperty]
        private bool _isSpecialMaskMode;
    }

    /// <summary>
    /// Information about an IRQ line status.
    /// </summary>
    public partial class IrqStatusInfo : ObservableObject {
        /// <summary>
        /// Gets or sets the IRQ number.
        /// </summary>
        [ObservableProperty]
        private int _irqNumber;

        /// <summary>
        /// Gets or sets whether this IRQ is masked.
        /// </summary>
        [ObservableProperty]
        private bool _isMasked;
    }
}
