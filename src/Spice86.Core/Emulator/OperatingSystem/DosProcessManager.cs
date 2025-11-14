namespace Spice86.Core.Emulator.OperatingSystem;

using Serilog.Events;

using Spice86.Core.CLI;
using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.LoadableFile.Dos;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.Memory.ReaderWriter;
using Spice86.Core.Emulator.OperatingSystem.Structures;
using Spice86.Shared.Emulator.Errors;
using Spice86.Shared.Emulator.Memory;
using Spice86.Shared.Interfaces;
using Spice86.Shared.Utils;

using System.Text;

/// <summary>
/// Setups the loading and execution of DOS programs and maintains the DOS PSP chains in memory.
/// </summary>
public class DosProcessManager : DosFileLoader {
    private const ushort ComOffset = 0x100;
    private readonly DosProgramSegmentPrefixTracker _pspTracker;
    private readonly DosMemoryManager _memoryManager;
    private readonly DosFileManager _fileManager;
    private readonly DosDriveManager _driveManager;

    /// <summary>
    /// The master environment block that all DOS PSPs inherit.
    /// </summary>
    /// <remarks>
    /// Not stored in emulated memory, so no one can modify it.
    /// </remarks>
    private readonly EnvironmentVariables _environmentVariables;

    public DosProcessManager(IMemory memory, State state,
        DosProgramSegmentPrefixTracker dosPspTracker, DosMemoryManager dosMemoryManager,
        DosFileManager dosFileManager, DosDriveManager dosDriveManager,
        IDictionary<string, string> envVars, ILoggerService loggerService)
        : base(memory, state, loggerService) {
        _pspTracker = dosPspTracker;
        _memoryManager = dosMemoryManager;
        _fileManager = dosFileManager;
        _driveManager = dosDriveManager;
        _environmentVariables = new();

        envVars.Add("PATH", $"{_driveManager.CurrentDrive.DosVolume}{DosPathResolver.DirectorySeparatorChar}");

        foreach (KeyValuePair<string, string> envVar in envVars) {
            _environmentVariables.Add(envVar.Key, envVar.Value);
        }
    }

    /// <summary>
    /// Converts the specified command-line arguments string into the format used by DOS.
    /// </summary>
    /// <param name="arguments">The command-line arguments string.</param>
    /// <returns>The command-line arguments in the format used by DOS.</returns>
    private static byte[] ArgumentsToDosBytes(string? arguments) {
        byte[] res = new byte[128];
        string correctLengthArguments = "";
        if (string.IsNullOrWhiteSpace(arguments) == false) {
            // Cut strings longer than 127 characters.
            correctLengthArguments = arguments.Length > 127 ? arguments[..127] : arguments;
        }

        // Set the command line size.
        res[0] = (byte)correctLengthArguments.Length;

        byte[] argumentsBytes = Encoding.ASCII.GetBytes(correctLengthArguments);

        // Copy the actual characters.
        int index = 0;
        for (; index < correctLengthArguments.Length; index++) {
            res[index + 1] = argumentsBytes[index];
        }

        res[index + 1] = 0x0D; // Carriage return.
        int endIndex = index + 1;
        return res[0..endIndex];
    }

    public override byte[] LoadFile(string file, string? arguments) {
        // TODO: We should be asking DosMemoryManager for a new block for the PSP, program, its
        // stack, and its requested extra space first. We shouldn't always assume that this is the
        // first program to be loaded and that we have enough space for it like we do right now.
        // This will need to be fixed for DOS program load/exec support.
        DosProgramSegmentPrefix psp = _pspTracker.PushPspSegment(_pspTracker.InitialPspSegment);
        ushort pspSegment = MemoryUtils.ToSegment(psp.BaseAddress);

        // Set the PSP's first 2 bytes to INT 20h.
        psp.Exit[0] = 0xCD;
        psp.Exit[1] = 0x20;

        psp.NextSegment = DosMemoryManager.LastFreeSegment;

        // Load the command-line arguments into the PSP's command tail.
        byte[] commandLineBytes = ArgumentsToDosBytes(arguments);
        byte length = commandLineBytes[0];
        string asciiCommandLine = Encoding.ASCII.GetString(commandLineBytes, 1, length);
        psp.DosCommandTail.Length = (byte)(asciiCommandLine.Length + 1);
        psp.DosCommandTail.Command = asciiCommandLine;

        byte[] environmentBlock = CreateEnvironmentBlock(file);

        // Allocate memory for the environment block with an MCB
        ushort envBlockSegment = AllocateEnvironmentBlock(environmentBlock, pspSegment);
        if (envBlockSegment == 0) {
            // Fallback: use the old method if allocation fails
            envBlockSegment = (ushort)(pspSegment + 1);
            _memory.LoadData(MemoryUtils.ToPhysicalAddress(envBlockSegment, 0), environmentBlock);
            if (_loggerService.IsEnabled(LogEventLevel.Warning)) {
                _loggerService.Warning("Failed to allocate environment block with MCB, using fallback method");
            }
        }

        // Point the PSP's environment segment to the environment block.
        psp.EnvironmentTableSegment = envBlockSegment;

        // Set the disk transfer area address to the command-line offset in the PSP.
        _fileManager.SetDiskTransferAreaAddress(
            pspSegment, DosCommandTail.OffsetInPspSegment);

        return LoadExeOrComFile(file, pspSegment);
    }

    /// <summary>
    /// Creates a DOS environment block from the current environment variables.
    /// </summary>
    /// <param name="programPath">The path to the program being executed.</param>
    /// <returns>A byte array containing the DOS environment block.</returns>
    private byte[] CreateEnvironmentBlock(string programPath) {
        using MemoryStream ms = new();
    
        // Add each environment variable as NAME=VALUE followed by a null terminator
        foreach (KeyValuePair<string, string> envVar in _environmentVariables) {
            string envString = $"{envVar.Key}={envVar.Value}";
            byte[] envBytes = Encoding.ASCII.GetBytes(envString);
            ms.Write(envBytes, 0, envBytes.Length);
            ms.WriteByte(0); // Null terminator for this variable
        }
    
        // Add final null byte to mark end of environment block
        ms.WriteByte(0);
    
        // Write a word with value 1 after the environment variables
        // This is required by DOS
        ms.WriteByte(1);
        ms.WriteByte(0);
    
        // Get the DOS path for the program (not the host path)
        string dosPath = _fileManager.GetDosProgramPath(programPath);
    
        // Write the DOS path to the environment block
        byte[] programPathBytes = Encoding.ASCII.GetBytes(dosPath);
        ms.Write(programPathBytes, 0, programPathBytes.Length);
        ms.WriteByte(0); // Null terminator for program path
    
        return ms.ToArray();
    }

    /// <summary>
    /// Allocates memory for an environment block and creates an MCB for it.
    /// </summary>
    /// <param name="environmentData">The environment block data to allocate.</param>
    /// <param name="pspSegment">The PSP segment that owns this environment block.</param>
    /// <returns>The segment address where the environment block was allocated, or 0 if allocation failed.</returns>
    private ushort AllocateEnvironmentBlock(byte[] environmentData, ushort pspSegment) {
        // Calculate size in paragraphs (round up)
        ushort sizeInParagraphs = (ushort)((environmentData.Length + 15) / 16);
        
        // Allocate memory block for environment
        DosMemoryControlBlock? envMcb = _memoryManager.AllocateMemoryBlock(sizeInParagraphs);
        if (envMcb is null) {
            if (_loggerService.IsEnabled(LogEventLevel.Error)) {
                _loggerService.Error("Failed to allocate environment block of {Size} paragraphs", sizeInParagraphs);
            }
            return 0;
        }

        // Set the MCB owner to the PSP segment
        envMcb.PspSegment = pspSegment;
        envMcb.Owner = "ENV";

        // Copy environment data to the allocated memory
        uint envBlockAddress = MemoryUtils.ToPhysicalAddress(envMcb.DataBlockSegment, 0);
        _memory.LoadData(envBlockAddress, environmentData);

        if (_loggerService.IsEnabled(LogEventLevel.Verbose)) {
            _loggerService.Verbose("Allocated environment block at segment {Segment} ({Size} paragraphs) for PSP {Psp}",
                ConvertUtils.ToHex16(envMcb.DataBlockSegment), sizeInParagraphs, ConvertUtils.ToHex16(pspSegment));
        }

        return envMcb.DataBlockSegment;
    }

    /// <summary>
    /// Loads a child process for execution (used by INT 21h function 4Bh).
    /// This creates a new PSP for the child process and loads the program into memory.
    /// </summary>
    /// <param name="programPath">The path to the program to load.</param>
    /// <param name="commandLine">The command line arguments for the program.</param>
    /// <param name="environmentSegment">The segment of the environment block (0 to inherit parent's).</param>
    /// <returns>The PSP segment of the loaded child process, or 0 if loading failed.</returns>
    public ushort LoadChildProcess(string programPath, string? commandLine, ushort environmentSegment) {
        if (_loggerService.IsEnabled(LogEventLevel.Information)) {
            _loggerService.Information("Loading child process: {ProgramPath} with args: {CommandLine}", 
                programPath, commandLine ?? "(none)");
        }

        // Get the current PSP as the parent
        ushort parentPspSegment = _pspTracker.GetCurrentPspSegment();

        // Allocate a new PSP segment for the child
        // For now, use a simple allocation strategy - we should improve this
        // TODO: Properly allocate PSP memory block through memory manager
        ushort newPspSegment = FindFreePspSegment();
        if (newPspSegment == 0) {
            if (_loggerService.IsEnabled(LogEventLevel.Error)) {
                _loggerService.Error("Failed to find free PSP segment for child process");
            }
            return 0;
        }

        // Push the new PSP onto the tracker
        DosProgramSegmentPrefix childPsp = _pspTracker.PushPspSegment(newPspSegment);
        
        // Set up the child PSP
        childPsp.Exit[0] = 0xCD;
        childPsp.Exit[1] = 0x20;
        childPsp.ParentProgramSegmentPrefix = parentPspSegment;
        childPsp.NextSegment = DosMemoryManager.LastFreeSegment;

        // Set up command line
        byte[] commandLineBytes = ArgumentsToDosBytes(commandLine);
        byte length = commandLineBytes[0];
        string asciiCommandLine = Encoding.ASCII.GetString(commandLineBytes, 1, length);
        childPsp.DosCommandTail.Length = (byte)(asciiCommandLine.Length + 1);
        childPsp.DosCommandTail.Command = asciiCommandLine;

        // Set up environment
        if (environmentSegment == 0) {
            // Inherit parent's environment
            DosProgramSegmentPrefix parentPsp = new DosProgramSegmentPrefix(_memory, 
                MemoryUtils.ToPhysicalAddress(parentPspSegment, 0));
            childPsp.EnvironmentTableSegment = parentPsp.EnvironmentTableSegment;
        } else {
            childPsp.EnvironmentTableSegment = environmentSegment;
        }

        // Load the program
        try {
            LoadExeOrComFile(programPath, newPspSegment);
        } catch (Exception ex) {
            if (_loggerService.IsEnabled(LogEventLevel.Error)) {
                _loggerService.Error(ex, "Failed to load child process: {ProgramPath}", programPath);
            }
            // Clean up: pop the PSP we just pushed
            _pspTracker.PopPspSegment(newPspSegment);
            return 0;
        }

        if (_loggerService.IsEnabled(LogEventLevel.Information)) {
            _loggerService.Information("Successfully loaded child process at PSP segment {PspSegment}", 
                ConvertUtils.ToHex16(newPspSegment));
        }

        return newPspSegment;
    }

    /// <summary>
    /// Finds a free segment for a new PSP.
    /// This is a simple implementation that should be improved with proper memory management.
    /// </summary>
    private ushort FindFreePspSegment() {
        // For now, just use a segment after the initial PSP
        // This is a placeholder and should be replaced with proper allocation
        ushort currentPsp = _pspTracker.GetCurrentPspSegment();
        
        // Try to find a free segment by checking the MCB chain
        // This is a simplified approach
        DosMemoryControlBlock? freeMcb = _memoryManager.AllocateMemoryBlock(0x10); // Allocate 16 paragraphs for PSP
        if (freeMcb != null) {
            return freeMcb.DataBlockSegment;
        }

        return 0;
    }

    private void LoadComFile(byte[] com) {
        ushort programEntryPointSegment = _pspTracker.GetProgramEntryPointSegment();
        uint physicalStartAddress = MemoryUtils.ToPhysicalAddress(programEntryPointSegment, ComOffset);
        _memory.LoadData(physicalStartAddress, com);

        // Make DS and ES point to the PSP
        _state.DS = programEntryPointSegment;
        _state.ES = programEntryPointSegment;
        SetEntryPoint(programEntryPointSegment, ComOffset);
        _state.InterruptFlag = true;
    }

    private void LoadExeFile(DosExeFile exeFile, ushort pspSegment) {
        if (_loggerService.IsEnabled(LogEventLevel.Verbose)) {
            _loggerService.Verbose("Read header: {ReadHeader}", exeFile);
        }

        DosMemoryControlBlock? block = _memoryManager.ReserveSpaceForExe(exeFile, pspSegment);
        if (block is null) {
            throw new UnrecoverableException($"Failed to reserve space for EXE file at {pspSegment}");
        }
        // The program image is typically loaded immediately above the PSP, which is the start of
        // the memory block that we just allocated. Seek 16 paragraphs into the allocated block to
        // get our starting point.
        ushort programEntryPointSegment = (ushort)(block.DataBlockSegment + 0x10);
        // There is one special case that we need to account for: if the EXE doesn't have any extra
        // allocations, we need to load it as high as possible in the memory block rather than
        // immediately after the PSP like we normally do. This will give the program extra space
        // between the PSP and the start of the program image that it can use however it wants.
        if (exeFile.MinAlloc == 0 && exeFile.MaxAlloc == 0) {
            ushort programEntryPointOffset = (ushort)(block.Size - exeFile.ProgramSizeInParagraphsPerHeader);
            programEntryPointSegment = (ushort)(block.DataBlockSegment + programEntryPointOffset);
        }

        LoadExeFileInMemoryAndApplyRelocations(exeFile, programEntryPointSegment);
        SetupCpuForExe(exeFile, programEntryPointSegment, pspSegment);
    }

    private byte[] LoadExeOrComFile(string file, ushort pspSegment) {
        byte[] fileBytes = ReadFile(file);
        if (_loggerService.IsEnabled(LogEventLevel.Debug)) {
            _loggerService.Debug("Executable file size: {Size}", fileBytes.Length);
        }

        // Check if file size is at least EXE header size
        if (fileBytes.Length >= DosExeFile.MinExeSize) {
            // Try to read it as exe
            DosExeFile exeFile = new DosExeFile(new ByteArrayReaderWriter(fileBytes));
            if (exeFile.IsValid) {
                LoadExeFile(exeFile, pspSegment);
            } else {
                if (_loggerService.IsEnabled(LogEventLevel.Debug)) {
                    _loggerService.Debug("File {File} does not have a valid EXE header. Considering it a COM file.", file);
                }

                LoadComFile(fileBytes);
            }
        } else {
            if (_loggerService.IsEnabled(LogEventLevel.Warning)) {
                _loggerService.Warning("File {File} size is {Size} bytes, which is less than minimum allowed. Consider it a COM file.",
                    file, fileBytes.Length);
            }
            LoadComFile(fileBytes);
        }
        if (_loggerService.IsEnabled(LogEventLevel.Information)) {
            _loggerService.Information("Initial CPU State: {CpuState}", _state);
        }

        return fileBytes;
    }

    /// <summary>
    /// Loads the program image and applies any necessary relocations to it.
    /// </summary>
    /// <param name="exeFile">The EXE file to load.</param>
    /// <param name="startSegment">The starting segment for the program.</param>
    private void LoadExeFileInMemoryAndApplyRelocations(DosExeFile exeFile, ushort startSegment) {
        uint physicalStartAddress = MemoryUtils.ToPhysicalAddress(startSegment, 0);
        _memory.LoadData(physicalStartAddress, exeFile.ProgramImage, (int)exeFile.ProgramSize);
        foreach (SegmentedAddress address in exeFile.RelocationTable) {
            // Read value from memory, add the start segment offset and write back
            uint addressToEdit = MemoryUtils.ToPhysicalAddress(address.Segment, address.Offset)
                + physicalStartAddress;
            _memory.UInt16[addressToEdit] += startSegment;
        }
    }

    /// <summary>
    /// Sets up the CPU to execute the loaded program.
    /// </summary>
    /// <param name="exeFile">The EXE file that was loaded.</param>
    /// <param name="startSegment">The starting segment address of the program.</param>
    /// <param name="pspSegment">The segment address of the program's PSP (Program Segment Prefix).</param>
    private void SetupCpuForExe(DosExeFile exeFile, ushort startSegment, ushort pspSegment) {
        // MS-DOS uses the values in the file header to set the SP and SS registers and
        // adjusts the initial value of the SS register by adding the start-segment
        // address to it.
        _state.SS = (ushort)(exeFile.InitSS + startSegment);
        _state.SP = exeFile.InitSP;

        // Make DS and ES point to the PSP
        _state.DS = pspSegment;
        _state.ES = pspSegment;

        _state.InterruptFlag = true;

        // Finally, MS-DOS reads the initial CS and IP values from the program's file
        // header, adjusts the CS register value by adding the start-segment address to
        // it, and transfers control to the program at the adjusted address.
        SetEntryPoint((ushort)(exeFile.InitCS + startSegment), exeFile.InitIP);
    }
}