namespace Spice86.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

using Spice86.Core.Emulator.InterruptHandlers.Bios.Structures;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.VM;
using Spice86.Shared.Emulator.Memory;
using Spice86.Shared.Utils;
using Spice86.ViewModels.Services;

/// <summary>
/// ViewModel for observing BIOS data area state in the debugger.
/// Displays comprehensive information from the BIOS Data Area (BDA) at segment 0x40 for reverse engineering.
/// </summary>
public partial class BiosViewModel : DebuggerTabViewModel {
    private readonly BiosDataArea _biosDataArea;

    /// <inheritdoc />
    public override string Header => "BIOS";

    /// <inheritdoc />
    public override string? IconKey => "Chip";

    // BDA Location
    [ObservableProperty]
    private string _bdaSegment = string.Empty;

    [ObservableProperty]
    private string _bdaLinearAddress = string.Empty;

    // Equipment
    [ObservableProperty]
    private ushort _equipmentWord;

    [ObservableProperty]
    private string _equipmentWordBinary = string.Empty;

    [ObservableProperty]
    private ushort _conventionalMemorySizeKb;

    [ObservableProperty]
    private string _ebdaSegment = string.Empty;

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

    [ObservableProperty]
    private string _cursorType = string.Empty;

    [ObservableProperty]
    private string _crtControllerBase = string.Empty;

    [ObservableProperty]
    private ushort _videoPageSize;

    [ObservableProperty]
    private ushort _videoPageStart;

    [ObservableProperty]
    private ushort _characterHeight;

    [ObservableProperty]
    private string _videoSaveTableAddress = string.Empty;

    // Keyboard
    [ObservableProperty]
    private byte _keyboardShiftFlags1;

    [ObservableProperty]
    private byte _keyboardShiftFlags2;

    [ObservableProperty]
    private byte _keyboardShiftFlags3;

    [ObservableProperty]
    private byte _keyboardLedStatus;

    [ObservableProperty]
    private ushort _keyboardBufferHead;

    [ObservableProperty]
    private ushort _keyboardBufferTail;

    [ObservableProperty]
    private ushort _keyboardBufferStart;

    [ObservableProperty]
    private ushort _keyboardBufferEnd;

    [ObservableProperty]
    private int _keyboardBufferCount;

    [ObservableProperty]
    private byte _altKeypadData;

    // Timer
    [ObservableProperty]
    private uint _ticksSinceMidnight;

    [ObservableProperty]
    private string _estimatedTime = string.Empty;

    [ObservableProperty]
    private byte _timerOverflowFlag;

    [ObservableProperty]
    private byte _breakFlag;

    // Disk
    [ObservableProperty]
    private byte _numberOfDiskDrives;

    [ObservableProperty]
    private byte _lastDiskStatus;

    [ObservableProperty]
    private byte _floppyMotorStatus;

    [ObservableProperty]
    private byte _floppyMotorCounter;

    [ObservableProperty]
    private byte _floppyLastStatus;

    [ObservableProperty]
    private byte _floppyLastDataRate;

    // Serial/Parallel ports
    [ObservableProperty]
    private string _com1Address = string.Empty;

    [ObservableProperty]
    private string _com2Address = string.Empty;

    [ObservableProperty]
    private string _com3Address = string.Empty;

    [ObservableProperty]
    private string _com4Address = string.Empty;

    [ObservableProperty]
    private string _lpt1Address = string.Empty;

    [ObservableProperty]
    private string _lpt2Address = string.Empty;

    [ObservableProperty]
    private string _lpt3Address = string.Empty;

    // Reset/Misc
    [ObservableProperty]
    private string _softResetFlag = string.Empty;

    [ObservableProperty]
    private string _jumpAddress = string.Empty;

    [ObservableProperty]
    private byte _lastUnexpectedIrq;

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

        // BDA Location - read from Core MemoryMap constants
        BdaSegment = $"0x{MemoryMap.BiosDataSegment:X4}";
        BdaLinearAddress = $"0x{MemoryUtils.ToPhysicalAddress(MemoryMap.BiosDataSegment, 0):X5}";

        // Equipment
        EquipmentWord = _biosDataArea.EquipmentListFlags;
        EquipmentWordBinary = Convert.ToString(_biosDataArea.EquipmentListFlags, 2).PadLeft(16, '0');
        ConventionalMemorySizeKb = _biosDataArea.ConventionalMemorySizeKb;
        EbdaSegment = $"0x{_biosDataArea.EbdaSeg:X4}";

        // Video
        CurrentVideoMode = _biosDataArea.VideoMode;
        ScreenColumnsCount = _biosDataArea.ScreenColumns;
        ScreenRowsCount = _biosDataArea.ScreenRows;
        ActivePage = _biosDataArea.CurrentVideoPage;
        CursorPositionPage0 = _biosDataArea.CursorPosition[0];
        CursorType = $"0x{_biosDataArea.CursorType:X4}";
        CrtControllerBase = $"0x{_biosDataArea.CrtControllerBaseAddress:X4}";
        VideoPageSize = _biosDataArea.VideoPageSize;
        VideoPageStart = _biosDataArea.VideoPageStart;
        CharacterHeight = _biosDataArea.CharacterHeight;
        SegmentedAddress videoSaveTable = _biosDataArea.VideoSaveTable;
        VideoSaveTableAddress = $"{videoSaveTable.Segment:X4}:{videoSaveTable.Offset:X4}";

        // Keyboard
        KeyboardShiftFlags1 = _biosDataArea.KeyboardStatusFlag;
        KeyboardShiftFlags2 = _biosDataArea.KeyboardStatusFlag2;
        KeyboardShiftFlags3 = _biosDataArea.KeyboardStatusFlag3;
        KeyboardLedStatus = _biosDataArea.KeyboardLedStatus;
        KeyboardBufferHead = _biosDataArea.KbdBufHead;
        KeyboardBufferTail = _biosDataArea.KbdBufTail;
        KeyboardBufferStart = _biosDataArea.KbdBufStartOffset;
        KeyboardBufferEnd = _biosDataArea.KbdBufEndOffset;
        AltKeypadData = _biosDataArea.AltKeypad;

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
        // Convert ticks to estimated time (18.2 ticks per second)
        double seconds = _biosDataArea.TimerCounter / 18.2;
        TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
        EstimatedTime = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        TimerOverflowFlag = _biosDataArea.TimerRollover;
        BreakFlag = _biosDataArea.BreakFlag;

        // Disk
        NumberOfDiskDrives = _biosDataArea.Hdcount;
        LastDiskStatus = _biosDataArea.DiskLastStatus;
        FloppyMotorStatus = _biosDataArea.FloppyMotorStatus;
        FloppyMotorCounter = _biosDataArea.FloppyMotorCounter;
        FloppyLastStatus = _biosDataArea.FloppyLastStatus;
        FloppyLastDataRate = _biosDataArea.FloppyLastDataRate;

        // Serial/Parallel ports
        Com1Address = $"0x{_biosDataArea.PortCom[0]:X4}";
        Com2Address = $"0x{_biosDataArea.PortCom[1]:X4}";
        Com3Address = $"0x{_biosDataArea.PortCom[2]:X4}";
        Com4Address = $"0x{_biosDataArea.PortCom[3]:X4}";
        Lpt1Address = $"0x{_biosDataArea.PortLpt[0]:X4}";
        Lpt2Address = $"0x{_biosDataArea.PortLpt[1]:X4}";
        Lpt3Address = $"0x{_biosDataArea.PortLpt[2]:X4}";

        // Reset/Misc
        SoftResetFlag = $"0x{_biosDataArea.SoftResetFlag:X4}";
        SegmentedAddress jumpAddr = _biosDataArea.Jump;
        JumpAddress = $"{jumpAddr.Segment:X4}:{jumpAddr.Offset:X4}";
        LastUnexpectedIrq = _biosDataArea.LastUnexpectedIrq;
    }

}
