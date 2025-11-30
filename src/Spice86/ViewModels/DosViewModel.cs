namespace Spice86.ViewModels;

using Avalonia.Collections;

using CommunityToolkit.Mvvm.ComponentModel;

using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.OperatingSystem;
using Spice86.Core.Emulator.OperatingSystem.Devices;
using Spice86.Core.Emulator.OperatingSystem.Enums;
using Spice86.Core.Emulator.OperatingSystem.Structures;
using Spice86.Core.Emulator.VM;
using Spice86.Shared.Utils;
using Spice86.ViewModels.Services;

/// <summary>
/// ViewModel for observing DOS kernel state in the debugger.
/// Displays comprehensive information about the DOS kernel including:
/// - File manager (open files, DTA)
/// - Memory manager (MCB chain, allocation statistics)
/// - Process manager (PSP chain, environment)
/// - SysVars (List of Lists)
/// - Virtual devices
/// </summary>
public partial class DosViewModel : DebuggerTabViewModel {
    private readonly Dos _dos;
    private readonly IMemory _memory;

    /// <inheritdoc />
    public override string Header => "DOS";

    /// <inheritdoc />
    public override string? IconKey => "Terminal";

    // DOS Version info - read from Core constants
    [ObservableProperty]
    private byte _dosMajorVersion;

    [ObservableProperty]
    private byte _dosMinorVersion;

    // File Manager state
    [ObservableProperty]
    private int _openFileCount;

    [ObservableProperty]
    private AvaloniaList<DosFileInfo> _openFiles = new();

    [ObservableProperty]
    private string _dtaAddress = string.Empty;

    // Memory Manager state - MCB Chain
    [ObservableProperty]
    private int _mcbCount;

    [ObservableProperty]
    private int _freeBlockCount;

    [ObservableProperty]
    private int _usedBlockCount;

    [ObservableProperty]
    private string _largestFreeBlock = string.Empty;

    [ObservableProperty]
    private string _totalFreeMemory = string.Empty;

    [ObservableProperty]
    private string _totalUsedMemory = string.Empty;

    [ObservableProperty]
    private string _allocationStrategy = string.Empty;

    [ObservableProperty]
    private AvaloniaList<McbInfo> _mcbChain = new();

    // Process Manager state - PSP Chain
    [ObservableProperty]
    private ushort _currentPspSegment;

    [ObservableProperty]
    private ushort _parentPspSegment;

    [ObservableProperty]
    private ushort _environmentSegment;

    [ObservableProperty]
    private ushort _lastChildReturnCode;

    [ObservableProperty]
    private int _pspCount;

    [ObservableProperty]
    private string _commandLine = string.Empty;

    // SysVars (List of Lists) state
    [ObservableProperty]
    private string _firstMcbSegment = string.Empty;

    [ObservableProperty]
    private string _bootDrive = string.Empty;

    [ObservableProperty]
    private ushort _numberOfBuffers;

    [ObservableProperty]
    private ushort _extendedMemorySize;

    [ObservableProperty]
    private byte _diskParameterBlockCount;

    [ObservableProperty]
    private byte _currentDirectoryStructureCount;

    [ObservableProperty]
    private ushort _maxSectorLength;

    // Drive Manager state
    [ObservableProperty]
    private string? _currentDrive;

    [ObservableProperty]
    private AvaloniaList<DosDriveInfo> _drives = new();

    // Virtual Devices state
    [ObservableProperty]
    private AvaloniaList<DosDeviceInfo> _devices = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DosViewModel"/> class.
    /// </summary>
    /// <param name="dos">The DOS kernel instance to observe.</param>
    /// <param name="memory">The emulator memory for reading MCB chain.</param>
    /// <param name="pauseHandler">The pause handler for tracking emulator pause state.</param>
    /// <param name="uiDispatcher">The UI dispatcher for thread-safe UI updates.</param>
    public DosViewModel(Dos dos, IMemory memory, IPauseHandler pauseHandler, IUIDispatcher uiDispatcher)
        : base(pauseHandler, uiDispatcher) {
        _dos = dos;
        _memory = memory;
    }

    /// <inheritdoc />
    public override void UpdateValues(object? sender, EventArgs e) {
        if (!IsVisible) {
            return;
        }

        UpdateProcessManagerState();
        
        // DOS version comes from the current PSP
        if (CurrentPspSegment != 0) {
            DosProgramSegmentPrefix psp = new(_memory, MemoryUtils.ToPhysicalAddress(CurrentPspSegment, 0));
            DosMajorVersion = psp.DosVersionMajor;
            DosMinorVersion = psp.DosVersionMinor;
        }

        UpdateFileManagerState();
        UpdateMemoryManagerState();
        UpdateSysVarsState();
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

        // DTA Address
        DtaAddress = $"{fileManager.DiskTransferAreaAddressSegment:X4}:{fileManager.DiskTransferAreaAddressOffset:X4}";

        // Only update if count changed to avoid UI flicker
        if (OpenFiles.Count != OpenFileCount) {
            OpenFiles.Clear();
            for (ushort i = 0; i < openFilesArray.Length; i++) {
                VirtualFileBase? file = openFilesArray[i];
                if (file != null) {
                    string name = file.Name;
                    string type = file is CharacterDevice ? "Device" : "File";
                    long position = 0;
                    long size = 0;
                    if (file is DosFile dosFile) {
                        try {
                            position = dosFile.Position;
                            size = dosFile.Length;
                        } catch {
                            // Ignore errors reading position/size
                        }
                    }
                    OpenFiles.Add(new DosFileInfo {
                        Handle = i,
                        Name = name,
                        Type = type,
                        Position = position,
                        Size = size
                    });
                }
            }
        }
    }

    private void UpdateMemoryManagerState() {
        DosMemoryManager memoryManager = _dos.MemoryManager;
        
        // Get the largest free block info
        DosMemoryControlBlock largestFree = memoryManager.FindLargestFree();
        LargestFreeBlock = $"{largestFree.Size * 16:N0} bytes ({largestFree.Size} paragraphs)";
        
        // Allocation strategy
        AllocationStrategy = memoryManager.AllocationStrategy.ToString();

        // Walk the MCB chain to gather statistics
        int mcbCount = 0;
        int freeCount = 0;
        int usedCount = 0;
        long totalFree = 0;
        long totalUsed = 0;
        
        List<McbInfo> mcbList = new();
        
        // Start from the first MCB segment from SysVars
        ushort mcbSegment = _dos.DosSysVars.FirstMCB;
        
        while (mcbSegment != 0 && mcbCount < 256) { // Safety limit
            DosMemoryControlBlock mcb = new(_memory, MemoryUtils.ToPhysicalAddress(mcbSegment, 0));
            
            if (!mcb.IsValid) {
                break;
            }
            
            mcbCount++;
            int sizeBytes = mcb.Size * 16;
            
            if (mcb.IsFree) {
                freeCount++;
                totalFree += sizeBytes;
            } else {
                usedCount++;
                totalUsed += sizeBytes;
            }
            
            mcbList.Add(new McbInfo {
                Segment = $"{mcbSegment:X4}",
                DataSegment = $"{mcb.DataBlockSegment:X4}",
                Size = $"{sizeBytes:N0}",
                SizeParagraphs = mcb.Size,
                PspSegment = $"{mcb.PspSegment:X4}",
                Owner = mcb.Owner,
                IsFree = mcb.IsFree,
                IsLast = mcb.IsLast,
                Type = mcb.IsFree ? "Free" : (mcb.IsLast ? "Last" : "Used")
            });
            
            if (mcb.IsLast) {
                break;
            }
            
            // Move to next MCB
            mcbSegment = (ushort)(mcbSegment + mcb.Size + 1);
        }
        
        McbCount = mcbCount;
        FreeBlockCount = freeCount;
        UsedBlockCount = usedCount;
        TotalFreeMemory = $"{totalFree:N0} bytes ({totalFree / 1024:N0} KB)";
        TotalUsedMemory = $"{totalUsed:N0} bytes ({totalUsed / 1024:N0} KB)";
        
        // Update MCB chain list
        if (McbChain.Count != mcbList.Count) {
            McbChain.Clear();
            foreach (McbInfo info in mcbList) {
                McbChain.Add(info);
            }
        }
    }

    private void UpdateProcessManagerState() {
        DosProcessManager processManager = _dos.ProcessManager;
        DosSwappableDataArea sda = _dos.DosSwappableDataArea;
        
        LastChildReturnCode = processManager.LastChildReturnCode;
        CurrentPspSegment = sda.CurrentProgramSegmentPrefix;
        
        // Try to read PSP information
        if (CurrentPspSegment != 0) {
            DosProgramSegmentPrefix psp = new(_memory, MemoryUtils.ToPhysicalAddress(CurrentPspSegment, 0));
            ParentPspSegment = psp.ParentProgramSegmentPrefix;
            EnvironmentSegment = psp.EnvironmentTableSegment;
            CommandLine = psp.DosCommandTail.Command;
        }
    }

    private void UpdateSysVarsState() {
        DosSysVars sysVars = _dos.DosSysVars;
        
        FirstMcbSegment = $"0x{sysVars.FirstMCB:X4}";
        BootDrive = $"{(char)('A' + sysVars.BootDrive)}:";
        NumberOfBuffers = sysVars.NumberOfBuffers;
        ExtendedMemorySize = sysVars.ExtendedMemorySize;
        DiskParameterBlockCount = sysVars.DiskParameterBlockCount;
        CurrentDirectoryStructureCount = sysVars.CurrentDirectoryStructureCount;
        MaxSectorLength = sysVars.MaxSectorLength;
    }

    private void UpdateDriveManagerState() {
        DosDriveManager driveManager = _dos.DosDriveManager;
        CurrentDrive = $"{driveManager.CurrentDrive.DriveLetter}:";

        if (Drives.Count != driveManager.Count) {
            Drives.Clear();
            foreach (VirtualDrive drive in driveManager.GetDrives()) {
                Drives.Add(new DosDriveInfo {
                    Letter = $"{drive.DriveLetter}:",
                    IsCurrent = drive.DriveLetter == driveManager.CurrentDrive.DriveLetter,
                    Label = drive.Label,
                    HostPath = drive.MountedHostDirectory
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
                    BaseAddress = $"0x{device.Header.BaseAddress:X6}",
                    Attributes = $"0x{(ushort)device.Header.Attributes:X4}",
                    IsCharacter = device.Header.Attributes.HasFlag(DeviceAttributes.Character)
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

        /// <summary>
        /// Gets or sets the current file position.
        /// </summary>
        public long Position { get; set; }

        /// <summary>
        /// Gets or sets the file size.
        /// </summary>
        public long Size { get; set; }
    }

    /// <summary>
    /// Information about a Memory Control Block.
    /// </summary>
    public class McbInfo {
        /// <summary>
        /// Gets or sets the MCB segment address.
        /// </summary>
        public string Segment { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the data block segment.
        /// </summary>
        public string DataSegment { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the size in bytes.
        /// </summary>
        public string Size { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the size in paragraphs.
        /// </summary>
        public ushort SizeParagraphs { get; set; }

        /// <summary>
        /// Gets or sets the PSP segment of the owner.
        /// </summary>
        public string PspSegment { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the owner name.
        /// </summary>
        public string Owner { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the block is free.
        /// </summary>
        public bool IsFree { get; set; }

        /// <summary>
        /// Gets or sets whether this is the last block.
        /// </summary>
        public bool IsLast { get; set; }

        /// <summary>
        /// Gets or sets the block type description.
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

        /// <summary>
        /// Gets or sets the volume label.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the mounted host path.
        /// </summary>
        public string HostPath { get; set; } = string.Empty;
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

        /// <summary>
        /// Gets or sets the device attributes.
        /// </summary>
        public string Attributes { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this is a character device.
        /// </summary>
        public bool IsCharacter { get; set; }
    }
}
