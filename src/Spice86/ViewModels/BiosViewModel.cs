namespace Spice86.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

using Spice86.Core.Emulator.InterruptHandlers.Bios.Structures;
using Spice86.Core.Emulator.VM;
using Spice86.ViewModels.Services;

/// <summary>
/// ViewModel for observing BIOS data area state in the debugger.
/// Displays information from the BIOS Data Area (BDA) at segment 0x40.
/// </summary>
public partial class BiosViewModel : DebuggerTabViewModel {
    private readonly BiosDataArea _biosDataArea;

    /// <inheritdoc />
    public override string Header => "BIOS";

    /// <inheritdoc />
    public override string? IconKey => "Chip";

    // Equipment
    [ObservableProperty]
    private ushort _equipmentWord;

    [ObservableProperty]
    private ushort _conventionalMemorySizeKb;

    // Video
    [ObservableProperty]
    private byte _currentVideoMode;

    [ObservableProperty]
    private ushort _screenColumnsCount;

    [ObservableProperty]
    private byte _screenRowsCount;

    [ObservableProperty]
    private byte _activePage;

    [ObservableProperty]
    private ushort _cursorPositionPage0;

    // Keyboard
    [ObservableProperty]
    private byte _keyboardShiftFlags1;

    [ObservableProperty]
    private byte _keyboardShiftFlags2;

    [ObservableProperty]
    private ushort _keyboardBufferHead;

    [ObservableProperty]
    private ushort _keyboardBufferTail;

    [ObservableProperty]
    private int _keyboardBufferCount;

    // Timer
    [ObservableProperty]
    private uint _ticksSinceMidnight;

    [ObservableProperty]
    private byte _timerOverflowFlag;

    // Disk
    [ObservableProperty]
    private byte _numberOfDiskDrives;

    [ObservableProperty]
    private byte _lastDiskStatus;

    // Serial/Parallel ports
    [ObservableProperty]
    private string _com1Address = string.Empty;

    [ObservableProperty]
    private string _com2Address = string.Empty;

    [ObservableProperty]
    private string _lpt1Address = string.Empty;

    [ObservableProperty]
    private string _lpt2Address = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="BiosViewModel"/> class.
    /// </summary>
    /// <param name="biosDataArea">The BIOS data area to observe.</param>
    /// <param name="pauseHandler">The pause handler for tracking emulator pause state.</param>
    /// <param name="uiDispatcher">The UI dispatcher for thread-safe UI updates.</param>
    public BiosViewModel(BiosDataArea biosDataArea, IPauseHandler pauseHandler, IUIDispatcher uiDispatcher)
        : base(pauseHandler, uiDispatcher) {
        _biosDataArea = biosDataArea;
    }

    /// <inheritdoc />
    public override void UpdateValues(object? sender, EventArgs e) {
        if (!IsVisible) {
            return;
        }

        // Equipment
        EquipmentWord = _biosDataArea.EquipmentListFlags;
        ConventionalMemorySizeKb = _biosDataArea.ConventionalMemorySizeKb;

        // Video
        CurrentVideoMode = _biosDataArea.VideoMode;
        ScreenColumnsCount = _biosDataArea.ScreenColumns;
        ScreenRowsCount = _biosDataArea.ScreenRows;
        ActivePage = _biosDataArea.CurrentVideoPage;
        CursorPositionPage0 = _biosDataArea.CursorPosition[0];

        // Keyboard
        KeyboardShiftFlags1 = _biosDataArea.KeyboardStatusFlag;
        KeyboardShiftFlags2 = _biosDataArea.KeyboardStatusFlag2;
        KeyboardBufferHead = _biosDataArea.KbdBufHead;
        KeyboardBufferTail = _biosDataArea.KbdBufTail;
        
        // Calculate keyboard buffer count
        ushort bufferStart = _biosDataArea.KbdBufStartOffset;
        ushort bufferEnd = _biosDataArea.KbdBufEndOffset;
        int bufferSize = bufferEnd - bufferStart;
        if (bufferSize > 0) {
            int head = _biosDataArea.KbdBufHead - bufferStart;
            int tail = _biosDataArea.KbdBufTail - bufferStart;
            KeyboardBufferCount = tail >= head ? (tail - head) / 2 : (bufferSize - head + tail) / 2;
        }

        // Timer
        TicksSinceMidnight = _biosDataArea.TimerCounter;
        TimerOverflowFlag = _biosDataArea.TimerRollover;

        // Disk
        NumberOfDiskDrives = _biosDataArea.Hdcount;
        LastDiskStatus = _biosDataArea.DiskLastStatus;

        // Serial/Parallel ports
        Com1Address = $"0x{_biosDataArea.PortCom[0]:X4}";
        Com2Address = $"0x{_biosDataArea.PortCom[1]:X4}";
        Lpt1Address = $"0x{_biosDataArea.PortLpt[0]:X4}";
        Lpt2Address = $"0x{_biosDataArea.PortLpt[1]:X4}";
    }
}
