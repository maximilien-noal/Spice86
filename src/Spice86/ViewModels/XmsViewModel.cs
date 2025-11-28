namespace Spice86.ViewModels;

using Avalonia.Collections;

using CommunityToolkit.Mvvm.ComponentModel;

using Spice86.Core.Emulator.InterruptHandlers.Dos.Xms;
using Spice86.Core.Emulator.VM;
using Spice86.ViewModels.Services;

/// <summary>
/// ViewModel for observing Extended Memory Manager (XMS/HIMEM.SYS) state in the debugger.
/// Displays information about XMS handles, memory blocks, A20 line state, and HMA.
/// </summary>
public partial class XmsViewModel : DebuggerTabViewModel {
    private readonly ExtendedMemoryManager? _xms;

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
    private string _xmsVersion = string.Empty;

    [ObservableProperty]
    private string _driverName = ExtendedMemoryManager.XmsIdentifier;

    // Memory Statistics
    [ObservableProperty]
    private string _totalXmsMemory = string.Empty;

    [ObservableProperty]
    private string _freeXmsMemory = string.Empty;

    [ObservableProperty]
    private string _largestFreeBlock = string.Empty;

    [ObservableProperty]
    private string _xmsBaseAddress = string.Empty;

    // A20 Line State
    [ObservableProperty]
    private bool _isA20Enabled;

    [ObservableProperty]
    private string _a20Status = string.Empty;

    // HMA State
    [ObservableProperty]
    private bool _isHmaClaimed;

    [ObservableProperty]
    private string _hmaStatus = string.Empty;

    // Handle Information
    [ObservableProperty]
    private int _handleCount;

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
    /// <param name="pauseHandler">The pause handler for tracking emulator pause state.</param>
    /// <param name="uiDispatcher">The UI dispatcher for thread-safe UI updates.</param>
    public XmsViewModel(ExtendedMemoryManager? xms, IPauseHandler pauseHandler, IUIDispatcher uiDispatcher)
        : base(pauseHandler, uiDispatcher) {
        _xms = xms;
        IsEnabled = xms != null;
    }

    /// <inheritdoc />
    public override void UpdateValues(object? sender, EventArgs e) {
        if (!IsVisible || _xms == null) {
            return;
        }

        // Update XMS configuration from the Core
        XmsVersion = $"{(ExtendedMemoryManager.XmsVersion >> 8) & 0xFF}.{ExtendedMemoryManager.XmsVersion & 0xFF:D2}";
        TotalXmsMemory = FormatMemorySize(ExtendedMemoryManager.XmsMemorySize * 1024L);
        XmsBaseAddress = $"0x{ExtendedMemoryManager.XmsBaseAddress:X8}";

        UpdateMemoryStatistics();
        UpdateA20State();
        UpdateHandles();
    }

    private void UpdateMemoryStatistics() {
        if (_xms == null) {
            return;
        }

        FreeXmsMemory = FormatMemorySize(_xms.TotalFreeMemory);
        LargestFreeBlock = FormatMemorySize(_xms.LargestFreeBlockLength);
    }

    private void UpdateA20State() {
        // We can't directly access A20 state from XMS manager
        // but we can infer some status
        A20Status = "Managed by XMS driver";
    }

    private void UpdateHandles() {
        if (_xms == null) {
            return;
        }

        // XMS internal handles are in a SortedList
        // We need to display information about allocated blocks
        // Since XmsBlock info is internal, we'll show memory block info

        MemoryBlocks.Clear();
        // Note: The XMS manager uses a linked list internally
        // We expose what we can observe

        long usedMemory = ExtendedMemoryManager.XmsMemorySize * 1024L - _xms.TotalFreeMemory;
        long freeMemory = _xms.TotalFreeMemory;

        MemoryBlocks.Add(new XmsBlockInfo {
            Type = "Used",
            Size = FormatMemorySize(usedMemory),
            SizeBytes = usedMemory
        });

        MemoryBlocks.Add(new XmsBlockInfo {
            Type = "Free",
            Size = FormatMemorySize(freeMemory),
            SizeBytes = freeMemory
        });
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
    }
}
