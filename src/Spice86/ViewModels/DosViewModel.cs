namespace Spice86.ViewModels;

using Avalonia.Collections;

using CommunityToolkit.Mvvm.ComponentModel;

using Spice86.Core.Emulator.OperatingSystem;
using Spice86.Core.Emulator.OperatingSystem.Devices;
using Spice86.Core.Emulator.OperatingSystem.Structures;
using Spice86.Core.Emulator.VM;
using Spice86.ViewModels.Services;

/// <summary>
/// ViewModel for observing DOS kernel state in the debugger.
/// Displays information about the DOS file manager, memory manager, process manager,
/// and registered virtual devices.
/// </summary>
public partial class DosViewModel : DebuggerTabViewModel {
    private readonly Dos _dos;

    /// <inheritdoc />
    public override string Header => "DOS";

    /// <inheritdoc />
    public override string? IconKey => "Terminal";

    // File Manager state
    [ObservableProperty]
    private int _openFileCount;

    [ObservableProperty]
    private AvaloniaList<DosFileInfo> _openFiles = new();

    // Memory Manager state
    [ObservableProperty]
    private int _allocatedMemoryBlocks;

    [ObservableProperty]
    private string _memoryStatus = string.Empty;

    // Process Manager state
    [ObservableProperty]
    private ushort _currentPspSegment;

    [ObservableProperty]
    private ushort _lastChildReturnCode;

    // Drive Manager state
    [ObservableProperty]
    private string? _currentDrive;

    [ObservableProperty]
    private AvaloniaList<DosDriveInfo> _drives = new();

    // Virtual Devices state
    [ObservableProperty]
    private AvaloniaList<DosDeviceInfo> _devices = new();

    // DOS Version info
    [ObservableProperty]
    private string _dosVersion = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="DosViewModel"/> class.
    /// </summary>
    /// <param name="dos">The DOS kernel instance to observe.</param>
    /// <param name="pauseHandler">The pause handler for tracking emulator pause state.</param>
    /// <param name="uiDispatcher">The UI dispatcher for thread-safe UI updates.</param>
    public DosViewModel(Dos dos, IPauseHandler pauseHandler, IUIDispatcher uiDispatcher)
        : base(pauseHandler, uiDispatcher) {
        _dos = dos;
        DosVersion = "DOS 5.0"; // Standard version emulated
    }

    /// <inheritdoc />
    public override void UpdateValues(object? sender, EventArgs e) {
        if (!IsVisible) {
            return;
        }

        UpdateFileManagerState();
        UpdateMemoryManagerState();
        UpdateProcessManagerState();
        UpdateDriveManagerState();
        UpdateDevicesState();
    }

    private void UpdateFileManagerState() {
        DosFileManager fileManager = _dos.FileManager;
        VirtualFileBase?[] openFilesArray = fileManager.OpenFiles;
        int count = 0;
        
        // Count non-null files
        for (int i = 0; i < openFilesArray.Length; i++) {
            if (openFilesArray[i] != null) {
                count++;
            }
        }
        
        OpenFileCount = count;

        // Only update if count changed to avoid UI flicker
        if (OpenFiles.Count != OpenFileCount) {
            OpenFiles.Clear();
            for (ushort i = 0; i < openFilesArray.Length; i++) {
                VirtualFileBase? file = openFilesArray[i];
                if (file != null) {
                    string name = file.Name;
                    OpenFiles.Add(new DosFileInfo {
                        Handle = i,
                        Name = name,
                        Type = file is CharacterDevice ? "Device" : "File"
                    });
                }
            }
        }
    }

    private void UpdateMemoryManagerState() {
        DosMemoryManager memoryManager = _dos.MemoryManager;
        // Get the largest free block info
        DosMemoryControlBlock largestFree = memoryManager.FindLargestFree();
        MemoryStatus = $"Largest free: {largestFree.Size * 16:N0} bytes ({largestFree.Size} paragraphs)";
    }

    private void UpdateProcessManagerState() {
        DosProcessManager processManager = _dos.ProcessManager;
        // The CurrentPspSegment is available via the tracker - we can get it from the process manager
        // but there's no direct property, so we'll just display the return code
        LastChildReturnCode = processManager.LastChildReturnCode;
        // PSP segment comes from the DOS SDA (Swappable Data Area)
        CurrentPspSegment = _dos.DosSwappableDataArea.CurrentProgramSegmentPrefix;
    }

    private void UpdateDriveManagerState() {
        DosDriveManager driveManager = _dos.DosDriveManager;
        CurrentDrive = $"{driveManager.CurrentDrive.DriveLetter}:";

        if (Drives.Count != driveManager.Count) {
            Drives.Clear();
            foreach (VirtualDrive drive in driveManager.GetDrives()) {
                Drives.Add(new DosDriveInfo {
                    Letter = $"{drive.DriveLetter}:",
                    IsCurrent = drive.DriveLetter == driveManager.CurrentDrive.DriveLetter
                });
            }
        }
    }

    private void UpdateDevicesState() {
        if (Devices.Count != _dos.Devices.Count) {
            Devices.Clear();
            foreach (IVirtualDevice device in _dos.Devices) {
                Devices.Add(new DosDeviceInfo {
                    Name = device.Name,
                    DeviceNumber = device.DeviceNumber,
                    BaseAddress = $"0x{device.Header.BaseAddress:X6}"
                });
            }
        }
    }

    /// <summary>
    /// Information about an open DOS file.
    /// </summary>
    public class DosFileInfo {
        /// <summary>
        /// Gets or sets the file handle.
        /// </summary>
        public ushort Handle { get; set; }

        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file type (File or Device).
        /// </summary>
        public string Type { get; set; } = string.Empty;
    }

    /// <summary>
    /// Information about a DOS drive.
    /// </summary>
    public class DosDriveInfo {
        /// <summary>
        /// Gets or sets the drive letter.
        /// </summary>
        public string Letter { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this is the current drive.
        /// </summary>
        public bool IsCurrent { get; set; }
    }

    /// <summary>
    /// Information about a DOS device.
    /// </summary>
    public class DosDeviceInfo {
        /// <summary>
        /// Gets or sets the device name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the device number.
        /// </summary>
        public uint DeviceNumber { get; set; }

        /// <summary>
        /// Gets or sets the device base address.
        /// </summary>
        public string BaseAddress { get; set; } = string.Empty;
    }
}
