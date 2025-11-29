namespace Spice86.Core.Emulator.OperatingSystem.Command.BatchProcessing;

using Serilog.Events;

using Spice86.Core.Emulator.OperatingSystem.Enums;
using Spice86.Core.Emulator.OperatingSystem.Structures;
using Spice86.Shared.Interfaces;

using System;
using System.IO;

/// <summary>
/// Executes DOS batch files by processing commands and launching programs.
/// </summary>
/// <remarks>
/// <para>
/// This class handles the execution of batch files, separating the execution logic
/// from the file loading logic in BatchFileLoader.
/// </para>
/// <para>
/// Key responsibilities:
/// <list type="bullet">
/// <item>Process batch commands (ECHO, SET, GOTO, etc.)</item>
/// <item>Resolve and launch executable programs (EXE/COM)</item>
/// <item>Handle nested batch files via CALL</item>
/// <item>Track the launched executable for SHA256 verification</item>
/// </list>
/// </para>
/// </remarks>
public sealed class BatchExecutor {
    private readonly Dos _dos;
    private readonly ILoggerService _loggerService;
    private readonly BatchProcessor _batchProcessor;

    /// <summary>
    /// Gets the path of the executable that was launched from the batch file.
    /// </summary>
    /// <remarks>
    /// This is used for SHA256 hash verification - the hash should be checked
    /// against the actual executable, not the batch file.
    /// </remarks>
    public string? LaunchedExecutablePath { get; private set; }

    /// <summary>
    /// Gets the content of the executable that was launched.
    /// </summary>
    /// <remarks>
    /// Used for SHA256 verification of the actual program being executed.
    /// </remarks>
    public byte[]? LaunchedExecutableContent { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchExecutor"/> class.
    /// </summary>
    /// <param name="dos">The DOS kernel instance.</param>
    /// <param name="batchProcessor">The batch processor for parsing commands.</param>
    /// <param name="loggerService">The logger service.</param>
    public BatchExecutor(Dos dos, BatchProcessor batchProcessor, ILoggerService loggerService) {
        _dos = dos;
        _batchProcessor = batchProcessor;
        _loggerService = loggerService;
    }

    /// <summary>
    /// Executes the batch file until an executable program is found and launched.
    /// </summary>
    /// <param name="batchFilePath">The path to the batch file.</param>
    /// <param name="arguments">Arguments passed to the batch file.</param>
    /// <returns>True if a program was successfully launched, false otherwise.</returns>
    public bool ExecuteUntilProgram(string batchFilePath, string[] arguments) {
        string batchDir = Path.GetDirectoryName(Path.GetFullPath(batchFilePath)) ?? "";

        if (!_batchProcessor.StartBatch(batchFilePath, arguments)) {
            if (_loggerService.IsEnabled(LogEventLevel.Error)) {
                _loggerService.Error("BatchExecutor: Failed to start batch file: {Path}", batchFilePath);
            }
            return false;
        }

        // Process batch commands until we find an executable
        while (true) {
            string? line = _batchProcessor.ReadNextLine(out bool shouldEcho);
            if (line is null) {
                if (_loggerService.IsEnabled(LogEventLevel.Warning)) {
                    _loggerService.Warning("BatchExecutor: Batch file ended without launching a program");
                }
                return false;
            }

            BatchCommand command = _batchProcessor.ParseCommand(line);

            if (_loggerService.IsEnabled(LogEventLevel.Debug)) {
                _loggerService.Debug(
                    "BatchExecutor: Processing command type={Type}, value='{Value}'",
                    command.Type, command.Value);
            }

            bool shouldContinue = ProcessCommand(command, batchDir);
            if (!shouldContinue) {
                // A program was launched or EXIT was encountered
                return LaunchedExecutablePath is not null;
            }
        }
    }

    /// <summary>
    /// Processes a single batch command.
    /// </summary>
    /// <param name="command">The command to process.</param>
    /// <param name="batchDir">The directory containing the batch file.</param>
    /// <returns>True to continue processing, false to stop.</returns>
    private bool ProcessCommand(BatchCommand command, string batchDir) {
        switch (command.Type) {
            case BatchCommandType.Empty:
            case BatchCommandType.PrintMessage:
            case BatchCommandType.SetVariable:
            case BatchCommandType.ShowEchoState:
            case BatchCommandType.ShowVariables:
            case BatchCommandType.ShowVariable:
            case BatchCommandType.Shift:
                // Internal commands - continue processing
                return true;

            case BatchCommandType.Goto:
                if (!string.IsNullOrEmpty(command.Value)) {
                    bool found = _batchProcessor.GotoLabel(command.Value);
                    if (!found && _loggerService.IsEnabled(LogEventLevel.Warning)) {
                        _loggerService.Warning("BatchExecutor: Label not found: {Label}", command.Value);
                    }
                }
                return true;

            case BatchCommandType.Exit:
                if (_loggerService.IsEnabled(LogEventLevel.Information)) {
                    _loggerService.Information("BatchExecutor: EXIT command encountered");
                }
                return false;

            case BatchCommandType.Pause:
                // Skip PAUSE during initial execution
                return true;

            case BatchCommandType.If:
            case BatchCommandType.For:
                // TODO: Handle conditional and loop commands
                if (_loggerService.IsEnabled(LogEventLevel.Debug)) {
                    _loggerService.Debug("BatchExecutor: Skipping {Type} command (not yet implemented)", command.Type);
                }
                return true;

            case BatchCommandType.ExecuteProgram:
            case BatchCommandType.CallBatch:
                return HandleExecuteProgram(command, batchDir);

            default:
                return true;
        }
    }

    /// <summary>
    /// Handles program execution commands.
    /// </summary>
    /// <returns>True to continue, false if program was launched.</returns>
    private bool HandleExecuteProgram(BatchCommand command, string batchDir) {
        string programName = command.Value;
        string programArgs = command.Arguments;

        if (string.IsNullOrEmpty(programName)) {
            return true;
        }

        string? resolvedPath = ResolveProgram(programName, batchDir);
        if (resolvedPath is null) {
            if (_loggerService.IsEnabled(LogEventLevel.Warning)) {
                _loggerService.Warning(
                    "BatchExecutor: Could not find program '{Program}'",
                    programName);
            }
            return true; // Continue to next command
        }

        if (_loggerService.IsEnabled(LogEventLevel.Information)) {
            _loggerService.Information(
                "BatchExecutor: Launching program '{Program}' with args '{Args}'",
                resolvedPath, programArgs);
        }

        // Store the executable info for SHA256 verification
        LaunchedExecutablePath = resolvedPath;
        LaunchedExecutableContent = File.ReadAllBytes(resolvedPath);

        // Execute via DOS EXEC
        string dosPath = _dos.FileManager.GetDosProgramPath(resolvedPath);
        DosExecResult result = _dos.ProcessManager.Exec(
            dosPath, programArgs, DosExecLoadType.LoadAndExecute);

        if (!result.Success) {
            if (_loggerService.IsEnabled(LogEventLevel.Error)) {
                _loggerService.Error(
                    "BatchExecutor: Failed to launch program '{Program}': {Error}",
                    programName, result.ErrorCode);
            }
            return true; // Continue to next command
        }

        return false; // Program launched, stop processing
    }

    /// <summary>
    /// Resolves a program name to a full host path.
    /// </summary>
    private string? ResolveProgram(string programName, string batchDir) {
        string extension = Path.GetExtension(programName).ToUpperInvariant();
        if (extension == ".EXE" || extension == ".COM" || extension == ".BAT") {
            return FindProgramFile(programName, batchDir);
        }

        // Try with standard extensions
        string[] extensions = [".COM", ".EXE", ".BAT"];
        foreach (string ext in extensions) {
            string? path = FindProgramFile(programName + ext, batchDir);
            if (path is not null) {
                return path;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds a program file in the batch directory or through DOS path resolution.
    /// </summary>
    private string? FindProgramFile(string programName, string batchDir) {
        // First try in the batch file's directory
        string fullPath = Path.Combine(batchDir, programName);
        if (File.Exists(fullPath)) {
            return fullPath;
        }

        // Try the program name directly (absolute path)
        if (Path.IsPathRooted(programName) && File.Exists(programName)) {
            return programName;
        }

        // Try to resolve through DOS file manager
        string dosPath = programName.Replace('/', '\\');
        string? hostPath = _dos.FileManager.GetHostPath(dosPath);
        if (!string.IsNullOrEmpty(hostPath) && File.Exists(hostPath)) {
            return hostPath;
        }

        return null;
    }
}
