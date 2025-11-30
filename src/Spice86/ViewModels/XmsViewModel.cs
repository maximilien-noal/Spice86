namespace Spice86.ViewModels;

using Avalonia.Collections;

using CommunityToolkit.Mvvm.ComponentModel;

using Spice86.Core.Emulator.InterruptHandlers.Dos.Xms;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.VM;
using Spice86.ViewModels.Services;

/// <summary>
/// ViewModel for observing Extended Memory Manager (XMS/HIMEM.SYS) state in the debugger.
/// Displays information about XMS handles, memory blocks, A20 line state, and HMA.
/// </summary>
public partial class XmsViewModel : DebuggerTabViewModel {
    private readonly ExtendedMemoryManager? _xms;
    private readonly A20Gate? _a20Gate;

    /// <inheritdoc />
    public override string Header => "XMS";

    /// <inheritdoc />
    public override string? IconKey => "Memory";

    /// <summary>
    /// Default maximum number of XMS handles.
    /// </summary>
    private const int DefaultMaxHandles = 128;

    // XMS Version and Status
    [ObservableProperty]
    private byte _xmsMajorVersion;

    [ObservableProperty]
    private byte _xmsMinorVersion;

    [ObservableProperty]
    private ushort _xmsInternalVersion;

    [ObservableProperty]
    private string _driverName = string.Empty;

    [ObservableProperty]
    private ushort _dosDeviceSegment;

    [ObservableProperty]
    private string _callbackAddress = string.Empty;

    // Memory Statistics
    [ObservableProperty]
    private string _totalXmsMemory = string.Empty;

    [ObservableProperty]
    private long _totalXmsMemoryBytes;

    [ObservableProperty]
    private string _freeXmsMemory = string.Empty;

    [ObservableProperty]
    private long _freeXmsMemoryBytes;

    [ObservableProperty]
    private string _usedXmsMemory = string.Empty;

    [ObservableProperty]
    private long _usedXmsMemoryBytes;

    [ObservableProperty]
    private string _largestFreeBlock = string.Empty;

    [ObservableProperty]
    private uint _largestFreeBlockBytes;

    [ObservableProperty]
    private uint _xmsBaseAddress;

    [ObservableProperty]
    private double _memoryUsagePercent;

    // A20 Line State
    [ObservableProperty]
    private bool _isA20Enabled;

    [ObservableProperty]
    private string _a20Status = string.Empty;

    [ObservableProperty]
    private uint _hmaStartAddress;

    [ObservableProperty]
    private uint _hmaEndAddress;

    // Handle Information
    [ObservableProperty]
    private int _handleCount;

    [ObservableProperty]
    private int _freeHandles;

    [ObservableProperty]
    private int _maxHandles = DefaultMaxHandles;

    [ObservableProperty]
    private AvaloniaList<XmsHandleInfo> _handles = new();

    // Memory Blocks
    [ObservableProperty]
    private AvaloniaList<XmsBlockInfo> _memoryBlocks = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="XmsViewModel"/> class.
    /// </summary>
    /// <param name="xms">The Extended Memory Manager instance to observe. Can be null if XMS is disabled.</param>
    /// <param name="a20Gate">The A20 gate for observing A20 line state.</param>
    /// <param name="pauseHandler">The pause handler for tracking emulator pause state.</param>
    /// <param name="uiDispatcher">The UI dispatcher for thread-safe UI updates.</param>
    public XmsViewModel(ExtendedMemoryManager? xms, A20Gate? a20Gate, IPauseHandler pauseHandler, IUIDispatcher uiDispatcher)
        : base(pauseHandler, uiDispatcher) {
        _xms = xms;
        _a20Gate = a20Gate;
        IsEnabled = xms != null;
    }

    /// <inheritdoc />
    public override void UpdateValues(object? sender, EventArgs e) {
        if (!IsVisible || _xms == null) {
            return;
        }

        // Update XMS driver information from the Core
        XmsMajorVersion = (byte)((ExtendedMemoryManager.XmsVersion >> 8) & 0xFF);
        XmsMinorVersion = (byte)(ExtendedMemoryManager.XmsVersion & 0xFF);
        XmsInternalVersion = ExtendedMemoryManager.XmsInternalVersion;
        DriverName = ExtendedMemoryManager.XmsIdentifier;
        DosDeviceSegment = ExtendedMemoryManager.DosDeviceSegment;
        CallbackAddress = $"{_xms.CallbackAddress.Segment:X4}:{_xms.CallbackAddress.Offset:X4}";

        // Memory configuration
        XmsBaseAddress = ExtendedMemoryManager.XmsBaseAddress;
        TotalXmsMemoryBytes = ExtendedMemoryManager.XmsMemorySize * 1024L;
        TotalXmsMemory = FormatMemorySize(TotalXmsMemoryBytes);

        // HMA addresses
        HmaStartAddress = A20Gate.StartOfHighMemoryArea;
        HmaEndAddress = A20Gate.EndOfHighMemoryArea;

        UpdateMemoryStatistics();
        UpdateA20State();
        UpdateMemoryBlocks();
    }

    private void UpdateMemoryStatistics() {
        if (_xms == null) {
            return;
        }

        FreeXmsMemoryBytes = _xms.TotalFreeMemory;
        FreeXmsMemory = FormatMemorySize(FreeXmsMemoryBytes);

        UsedXmsMemoryBytes = TotalXmsMemoryBytes - FreeXmsMemoryBytes;
        UsedXmsMemory = FormatMemorySize(UsedXmsMemoryBytes);

        LargestFreeBlockBytes = _xms.LargestFreeBlockLength;
        LargestFreeBlock = FormatMemorySize(LargestFreeBlockBytes);

        if (TotalXmsMemoryBytes > 0) {
            MemoryUsagePercent = (double)UsedXmsMemoryBytes / TotalXmsMemoryBytes * 100;
        }
    }

    private void UpdateA20State() {
        if (_a20Gate != null) {
            IsA20Enabled = _a20Gate.IsEnabled;
            A20Status = _a20Gate.IsEnabled ? "Enabled (access to memory above 1MB)" : "Disabled (memory wraps at 1MB)";
        } else {
            A20Status = "Not available";
        }
    }

    private void UpdateMemoryBlocks() {
        if (_xms == null) {
            return;
        }

        // Build memory block list showing used vs free
        MemoryBlocks.Clear();

        MemoryBlocks.Add(new XmsBlockInfo {
            Type = "Used",
            Size = UsedXmsMemory,
            SizeBytes = UsedXmsMemoryBytes,
            SizeKb = (uint)(UsedXmsMemoryBytes / 1024),
            Percentage = MemoryUsagePercent
        });

        MemoryBlocks.Add(new XmsBlockInfo {
            Type = "Free",
            Size = FreeXmsMemory,
            SizeBytes = FreeXmsMemoryBytes,
            SizeKb = (uint)(FreeXmsMemoryBytes / 1024),
            Percentage = 100 - MemoryUsagePercent
        });

        // Calculate handle count based on memory usage
        // Since we can't access internal handles directly, estimate based on fragmentation
        if (UsedXmsMemoryBytes > 0 && LargestFreeBlockBytes < FreeXmsMemoryBytes) {
            // Memory is fragmented, likely multiple allocations
            HandleCount = (int)((FreeXmsMemoryBytes - LargestFreeBlockBytes) / (16 * 1024)) + 1;
        } else if (UsedXmsMemoryBytes > 0) {
            HandleCount = 1; // At least one allocation
        } else {
            HandleCount = 0;
        }

        FreeHandles = MaxHandles - HandleCount;
    }

    private static string FormatMemorySize(long bytes) {
        if (bytes >= 1024 * 1024) {
            return $"{bytes / (1024 * 1024.0):F2} MB";
        }
        if (bytes >= 1024) {
            return $"{bytes / 1024.0:F2} KB";
        }
        return $"{bytes} bytes";
    }

    /// <summary>
    /// Information about an XMS handle.
    /// </summary>
    public class XmsHandleInfo {
        /// <summary>
        /// Gets or sets the handle number.
        /// </summary>
        public ushort Handle { get; set; }

        /// <summary>
        /// Gets or sets the block size.
        /// </summary>
        public string Size { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the lock count.
        /// </summary>
        public byte LockCount { get; set; }

        /// <summary>
        /// Gets or sets the linear address (if locked).
        /// </summary>
        public string LinearAddress { get; set; } = string.Empty;
    }

    /// <summary>
    /// Information about an XMS memory block.
    /// </summary>
    public class XmsBlockInfo {
        /// <summary>
        /// Gets or sets the block type (Used/Free).
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the formatted size.
        /// </summary>
        public string Size { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the size in bytes.
        /// </summary>
        public long SizeBytes { get; set; }

        /// <summary>
        /// Gets or sets the size in KB.
        /// </summary>
        public uint SizeKb { get; set; }

        /// <summary>
        /// Gets or sets the percentage of total memory.
        /// </summary>
        public double Percentage { get; set; }
    }
}
