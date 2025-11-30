namespace Spice86.ViewModels;

using Avalonia.Collections;

using CommunityToolkit.Mvvm.ComponentModel;

using Spice86.Core.Emulator.InterruptHandlers.Dos.Ems;
using Spice86.Core.Emulator.VM;
using Spice86.ViewModels.Services;

/// <summary>
/// ViewModel for observing Expanded Memory Manager (EMS) state in the debugger.
/// Displays information about EMS handles, page mappings, and memory usage.
/// </summary>
public partial class EmsViewModel : DebuggerTabViewModel {
    private readonly ExpandedMemoryManager? _ems;

    /// <inheritdoc />
    public override string Header => "EMS";

    /// <inheritdoc />
    public override string? IconKey => "Memory";

    // EMS Driver Information
    [ObservableProperty]
    private string _driverIdentifier = string.Empty;

    [ObservableProperty]
    private ushort _dosDeviceSegment;

    // EMS Version and Status - EMS 4.0 is the standard version (from EmsStatusCodes and LIM EMS 4.0 spec)
    [ObservableProperty]
    private byte _emsMajorVersion;

    [ObservableProperty]
    private byte _emsMinorVersion;

    [ObservableProperty]
    private ushort _pageFrameSegment;

    [ObservableProperty]
    private string _pageFrameAddress = string.Empty;

    [ObservableProperty]
    private uint _pageFrameSize;

    [ObservableProperty]
    private ushort _pageSize;

    [ObservableProperty]
    private byte _maxPhysicalPages;

    // Memory Statistics
    [ObservableProperty]
    private ushort _totalPages;

    [ObservableProperty]
    private ushort _freePages;

    [ObservableProperty]
    private ushort _usedPages;

    [ObservableProperty]
    private string _totalMemory = string.Empty;

    [ObservableProperty]
    private string _freeMemory = string.Empty;

    [ObservableProperty]
    private string _usedMemory = string.Empty;

    [ObservableProperty]
    private double _memoryUsagePercent;

    // Handle Information
    [ObservableProperty]
    private int _handleCount;

    [ObservableProperty]
    private int _totalAllocatedPages;

    [ObservableProperty]
    private AvaloniaList<EmsHandleInfo> _handles = new();

    // Page Frame Mappings
    [ObservableProperty]
    private AvaloniaList<EmsPageFrameInfo> _pageFrameMappings = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EmsViewModel"/> class.
    /// </summary>
    /// <param name="ems">The Expanded Memory Manager instance to observe. Can be null if EMS is disabled.</param>
    /// <param name="pauseHandler">The pause handler for tracking emulator pause state.</param>
    /// <param name="uiDispatcher">The UI dispatcher for thread-safe UI updates.</param>
    public EmsViewModel(ExpandedMemoryManager? ems, IPauseHandler pauseHandler, IUIDispatcher uiDispatcher)
        : base(pauseHandler, uiDispatcher) {
        _ems = ems;
        IsEnabled = ems != null;
    }

    /// <inheritdoc />
    public override void UpdateValues(object? sender, EventArgs e) {
        if (!IsVisible || _ems == null) {
            return;
        }

        // Update driver information from Core constants
        DriverIdentifier = ExpandedMemoryManager.EmsIdentifier;
        DosDeviceSegment = ExpandedMemoryManager.DosDeviceSegment;

        // Update EMS version from the Core constant (BCD format: 0x32 = 3.2)
        EmsMajorVersion = (byte)(ExpandedMemoryManager.EmsVersion >> 4);
        EmsMinorVersion = (byte)(ExpandedMemoryManager.EmsVersion & 0x0F);

        // Page frame configuration
        PageFrameSegment = ExpandedMemoryManager.EmmPageFrameSegment;
        PageFrameAddress = $"0x{(uint)(ExpandedMemoryManager.EmmPageFrameSegment * 16):X5}";
        PageFrameSize = ExpandedMemoryManager.EmmPageFrameSize;
        PageSize = ExpandedMemoryManager.EmmPageSize;
        MaxPhysicalPages = ExpandedMemoryManager.EmmMaxPhysicalPages;

        // Memory statistics
        TotalPages = EmmMemory.TotalPages;
        TotalMemory = FormatMemorySize(EmmMemory.EmmMemorySize);

        UpdateMemoryStatistics();
        UpdateHandles();
        UpdatePageFrameMappings();
    }

    private void UpdateMemoryStatistics() {
        if (_ems == null) {
            return;
        }

        FreePages = _ems.GetFreePageCount();
        UsedPages = (ushort)(TotalPages - FreePages);
        FreeMemory = FormatMemorySize(FreePages * ExpandedMemoryManager.EmmPageSize);
        UsedMemory = FormatMemorySize(UsedPages * ExpandedMemoryManager.EmmPageSize);

        if (TotalPages > 0) {
            MemoryUsagePercent = (double)UsedPages / TotalPages * 100;
        }
    }

    private void UpdateHandles() {
        if (_ems == null) {
            return;
        }

        HandleCount = _ems.EmmHandles.Count;
        TotalAllocatedPages = 0;

        // Rebuild handles list
        Handles.Clear();
        foreach (KeyValuePair<int, EmmHandle> handle in _ems.EmmHandles) {
            int pageCount = handle.Value.LogicalPages.Count;
            TotalAllocatedPages += pageCount;

            Handles.Add(new EmsHandleInfo {
                HandleNumber = handle.Value.HandleNumber,
                Name = string.IsNullOrEmpty(handle.Value.Name) ? "(unnamed)" : handle.Value.Name,
                LogicalPagesCount = pageCount,
                TotalSize = FormatMemorySize(pageCount * ExpandedMemoryManager.EmmPageSize),
                TotalSizeBytes = pageCount * ExpandedMemoryManager.EmmPageSize,
                HasSavedPageMap = handle.Value.SavedPageMap
            });
        }
    }

    private void UpdatePageFrameMappings() {
        if (_ems == null) {
            return;
        }

        // Always update page frame mappings as they change frequently
        PageFrameMappings.Clear();
        foreach (KeyValuePair<ushort, EmmRegister> mapping in _ems.EmmPageFrame) {
            ushort segment = (ushort)(ExpandedMemoryManager.EmmPageFrameSegment + (mapping.Key * ExpandedMemoryManager.EmmPageSize / 16));
            uint linearAddress = (uint)(segment * 16);
            bool isMapped = mapping.Value.PhysicalPage.PageNumber != ExpandedMemoryManager.EmmNullPage;

            PageFrameMappings.Add(new EmsPageFrameInfo {
                PhysicalPage = mapping.Key,
                Segment = $"0x{segment:X4}",
                LinearAddress = $"0x{linearAddress:X5}",
                MappedLogicalPage = isMapped
                    ? $"Page {mapping.Value.PhysicalPage.PageNumber}"
                    : "Unmapped",
                IsMapped = isMapped
            });
        }
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
    /// Information about an EMS handle.
    /// </summary>
    public class EmsHandleInfo {
        /// <summary>
        /// Gets or sets the handle number.
        /// </summary>
        public ushort HandleNumber { get; set; }

        /// <summary>
        /// Gets or sets the handle name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of logical pages allocated to this handle.
        /// </summary>
        public int LogicalPagesCount { get; set; }

        /// <summary>
        /// Gets or sets the total memory size for this handle.
        /// </summary>
        public string TotalSize { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total size in bytes.
        /// </summary>
        public long TotalSizeBytes { get; set; }

        /// <summary>
        /// Gets or sets whether this handle has a saved page map.
        /// </summary>
        public bool HasSavedPageMap { get; set; }
    }

    /// <summary>
    /// Information about an EMS page frame mapping.
    /// </summary>
    public class EmsPageFrameInfo {
        /// <summary>
        /// Gets or sets the physical page number (0-3).
        /// </summary>
        public ushort PhysicalPage { get; set; }

        /// <summary>
        /// Gets or sets the segment address.
        /// </summary>
        public string Segment { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the linear address.
        /// </summary>
        public string LinearAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the mapped logical page description.
        /// </summary>
        public string MappedLogicalPage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this page is currently mapped.
        /// </summary>
        public bool IsMapped { get; set; }
    }
}
