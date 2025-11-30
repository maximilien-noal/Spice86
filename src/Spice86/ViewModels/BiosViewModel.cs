namespace Spice86.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

using Spice86.Core.Emulator.InterruptHandlers.Bios.Structures;
using Spice86.Core.Emulator.VM;
using Spice86.Shared.Emulator.Memory;
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
    private string _videoModeDescription = string.Empty;

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
    private string _keyboardShiftFlags1Description = string.Empty;

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

        // BDA Location (fixed addresses)
        BdaSegment = "0x0040";
        BdaLinearAddress = "0x00400";

        // Equipment
        EquipmentWord = _biosDataArea.EquipmentListFlags;
        EquipmentWordBinary = Convert.ToString(_biosDataArea.EquipmentListFlags, 2).PadLeft(16, '0');
        ConventionalMemorySizeKb = _biosDataArea.ConventionalMemorySizeKb;
        EbdaSegment = $"0x{_biosDataArea.EbdaSeg:X4}";

        // Video
        CurrentVideoMode = _biosDataArea.VideoMode;
        VideoModeDescription = GetVideoModeDescription(_biosDataArea.VideoMode);
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
        KeyboardShiftFlags1Description = GetKeyboardShiftDescription(_biosDataArea.KeyboardStatusFlag);
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

    private static string GetVideoModeDescription(byte mode) {
        return mode switch {
            0x00 => "40x25 Text (B/W)",
            0x01 => "40x25 Text (Color)",
            0x02 => "80x25 Text (B/W)",
            0x03 => "80x25 Text (Color)",
            0x04 => "320x200 4-Color CGA",
            0x05 => "320x200 4-Color CGA (B/W)",
            0x06 => "640x200 2-Color CGA",
            0x07 => "80x25 Mono Text",
            0x0D => "320x200 16-Color EGA",
            0x0E => "640x200 16-Color EGA",
            0x0F => "640x350 Mono EGA",
            0x10 => "640x350 16-Color EGA",
            0x11 => "640x480 2-Color VGA",
            0x12 => "640x480 16-Color VGA",
            0x13 => "320x200 256-Color VGA",
            _ => $"Mode 0x{mode:X2}"
        };
    }

    private static string GetKeyboardShiftDescription(byte flags) {
        System.Collections.Generic.List<string> parts = new();
        if ((flags & 0x01) != 0) {
            parts.Add("RShift");
        }

        if ((flags & 0x02) != 0) {
            parts.Add("LShift");
        }

        if ((flags & 0x04) != 0) {
            parts.Add("Ctrl");
        }

        if ((flags & 0x08) != 0) {
            parts.Add("Alt");
        }

        if ((flags & 0x10) != 0) {
            parts.Add("Scroll");
        }

        if ((flags & 0x20) != 0) {
            parts.Add("Num");
        }

        if ((flags & 0x40) != 0) {
            parts.Add("Caps");
        }

        if ((flags & 0x80) != 0) {
            parts.Add("Ins");
        }

        return parts.Count > 0 ? string.Join("+", parts) : "None";
    }
}
