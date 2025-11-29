namespace Spice86.Core.Emulator.LoadableFile.Dos;

using Serilog.Events;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.OperatingSystem;
using Spice86.Core.Emulator.OperatingSystem.Command;
using Spice86.Core.Emulator.OperatingSystem.Command.BatchProcessing;
using Spice86.Core.Emulator.OperatingSystem.Enums;
using Spice86.Core.Emulator.OperatingSystem.Structures;
using Spice86.Shared.Emulator.Errors;
using Spice86.Shared.Interfaces;

using System;
using System.IO;

/// <summary>
/// Loader for DOS batch files (.BAT).
/// </summary>
/// <remarks>
/// <para>
/// Batch files are interpreted line-by-line by COMMAND.COM. This loader:
/// 1. Parses the batch file to find the first executable command (EXE/COM)
/// 2. Loads and executes that program via DOS EXEC
/// 3. Stores remaining batch commands in CommandCom's BatchProcessor for later execution
/// </para>
/// <para>
/// Currently, only the first executable command is fully supported. Full batch file
/// execution with multiple programs requires emulation loop integration.
/// </para>
/// </remarks>
public sealed class BatchFileLoader : ExecutableFileLoader {
    private readonly Dos _dos;

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchFileLoader"/> class.
    /// </summary>
    /// <param name="memory">The memory bus.</param>
    /// <param name="state">The CPU state.</param>
    /// <param name="dos">The DOS kernel instance.</param>
    /// <param name="loggerService">The logger service.</param>
    public BatchFileLoader(IMemory memory, State state, Dos dos, ILoggerService loggerService)
        : base(memory, state, loggerService) {
        _dos = dos;
    }

    /// <inheritdoc/>
    public override bool DosInitializationNeeded => true;

    /// <inheritdoc/>
    public override byte[] LoadFile(string file, string? arguments) {
        if (_loggerService.IsEnabled(LogEventLevel.Information)) {
            _loggerService.Information(
                "BatchFileLoader: Loading batch file '{File}' with args '{Args}'",
                file, arguments ?? "");
        }

        // Read the batch file content for checksum verification
        byte[] fileContent = ReadFile(file);

        // Parse arguments into array
        string[] args = ParseArguments(arguments);

        // Get the host path to the batch file
        string hostPath = Path.GetFullPath(file);
        string batchDir = Path.GetDirectoryName(hostPath) ?? "";

        // Start the batch file in the processor
        CommandCom commandCom = _dos.ProcessManager.CommandCom;
        BatchProcessor batchProcessor = commandCom.BatchProcessor;
        
        if (!batchProcessor.StartBatch(hostPath, args)) {
            throw new UnrecoverableException($"Failed to start batch file: {file}");
        }

        // Read lines from the batch file until we find an executable command
        while (true) {
            string? line = batchProcessor.ReadNextLine(out bool shouldEcho);
            if (line is null) {
                // End of batch file with no executable command
                throw new UnrecoverableException(
                    $"Batch file '{file}' contains no executable commands (EXE/COM)");
            }

            // Parse the command
            BatchCommand command = batchProcessor.ParseCommand(line);
            
            if (_loggerService.IsEnabled(LogEventLevel.Debug)) {
                _loggerService.Debug(
                    "BatchFileLoader: Parsed command type={Type}, value='{Value}'",
                    command.Type, command.Value);
            }

            // Handle internal batch commands (ECHO, SET, etc.) silently
            switch (command.Type) {
                case BatchCommandType.Empty:
                case BatchCommandType.PrintMessage:
                case BatchCommandType.SetVariable:
                case BatchCommandType.ShowEchoState:
                case BatchCommandType.ShowVariables:
                case BatchCommandType.ShowVariable:
                case BatchCommandType.Shift:
                    // Skip internal commands that don't launch programs
                    continue;

                case BatchCommandType.Goto:
                    // Handle GOTO by seeking to label
                    if (!string.IsNullOrEmpty(command.Value)) {
                        batchProcessor.GotoLabel(command.Value);
                    }
                    continue;

                case BatchCommandType.Exit:
                    throw new UnrecoverableException(
                        $"Batch file '{file}' exited before launching any program");

                case BatchCommandType.Pause:
                    // Skip PAUSE in startup
                    continue;

                case BatchCommandType.If:
                case BatchCommandType.For:
                    // TODO: Handle conditional and loop commands
                    continue;

                case BatchCommandType.ExecuteProgram:
                case BatchCommandType.CallBatch:
                    // Found an executable command - launch it
                    string programName = command.Value;
                    string programArgs = command.Arguments;
                    
                    if (string.IsNullOrEmpty(programName)) {
                        continue;
                    }
                    
                    // Resolve the program path
                    string? resolvedPath = ResolveProgram(programName, batchDir);
                    if (resolvedPath is null) {
                        if (_loggerService.IsEnabled(LogEventLevel.Warning)) {
                            _loggerService.Warning(
                                "BatchFileLoader: Could not find program '{Program}' referenced in batch file",
                                programName);
                        }
                        continue; // Try next command
                    }

                    if (_loggerService.IsEnabled(LogEventLevel.Information)) {
                        _loggerService.Information(
                            "BatchFileLoader: Launching program '{Program}' with args '{Args}'",
                            resolvedPath, programArgs);
                    }

                    // Convert to DOS path and execute via DOS EXEC
                    string dosPath = _dos.FileManager.GetDosProgramPath(resolvedPath);
                    DosExecResult result = _dos.ProcessManager.Exec(
                        dosPath, programArgs, DosExecLoadType.LoadAndExecute);

                    if (!result.Success) {
                        throw new UnrecoverableException(
                            $"Failed to launch program '{programName}' from batch file: {result.ErrorCode}");
                    }

                    // Return the batch file content for checksum
                    // (the executable has been loaded and CPU state is set up)
                    return fileContent;
            }
        }
    }

    /// <summary>
    /// Resolves a program name to a full host path.
    /// </summary>
    /// <param name="programName">The program name (with or without extension).</param>
    /// <param name="batchDir">The directory containing the batch file.</param>
    /// <returns>The full host path to the program, or null if not found.</returns>
    private string? ResolveProgram(string programName, string batchDir) {
        // If program already has an extension, try it directly
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
        string? hostPath = null;
        try {
            hostPath = _dos.FileManager.GetHostPath(dosPath);
            if (!string.IsNullOrEmpty(hostPath) && File.Exists(hostPath)) {
                return hostPath;
            }
        } catch {
            // Path resolution failed
        }

        return null;
    }

    /// <summary>
    /// Parses command line arguments into an array.
    /// </summary>
    private static string[] ParseArguments(string? arguments) {
        if (string.IsNullOrWhiteSpace(arguments)) {
            return Array.Empty<string>();
        }

        // Simple space-separated parsing (doesn't handle quotes yet)
        return arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }
}
