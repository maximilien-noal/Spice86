namespace Spice86.Core.Emulator.LoadableFile.Dos;

using Serilog.Events;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.OperatingSystem;
using Spice86.Shared.Interfaces;

using System;
using System.IO;

/// <summary>
/// Loader for DOS batch files (.BAT).
/// </summary>
/// <remarks>
/// <para>
/// Batch files are not loaded as executables but are interpreted line-by-line
/// by COMMAND.COM. This loader sets up the batch file for execution by
/// initializing the BatchProcessor in CommandCom.
/// </para>
/// <para>
/// The loader returns the batch file content as its "executable bytes" for
/// checksum verification, even though the batch file is not loaded into memory
/// as executable code.
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

        // Start the batch file via CommandCom's BatchProcessor
        CommandCom commandCom = _dos.ProcessManager.CommandCom;
        bool started = commandCom.BatchProcessor.StartBatch(hostPath, args);

        if (!started) {
            throw new InvalidOperationException($"Failed to start batch file: {file}");
        }

        // For batch files, we need a different execution model.
        // The batch processor will read and parse lines, but we need something
        // to actually execute the commands. Since batch files don't have
        // executable code, we need to handle this differently.
        //
        // The proper approach is to use an auto-generated AUTOEXEC.BAT approach
        // where we bootstrap through CommandCom. For now, we'll just load a
        // minimal stub that calls the batch file.
        //
        // Create an autoexec that calls the batch file
        string dosPath = _dos.FileManager.GetDosProgramPath(hostPath);
        AutoexecGenerator autoexec = CommandCom.CreateAutoexecForBatch(dosPath, arguments ?? "", exitAfter: true);
        
        // Exit the direct batch we started (preserves any parent contexts from nested CALLs)
        commandCom.BatchProcessor.ExitBatch();
        commandCom.LoadAutoexecBatch(autoexec, "STARTUP.BAT");

        if (_loggerService.IsEnabled(LogEventLevel.Debug)) {
            _loggerService.Debug(
                "BatchFileLoader: Loaded batch file via autoexec bootstrap. Lines: {Lines}",
                autoexec.Generate().Length);
        }

        // We need to execute a minimal program to drive the batch processing.
        // For now, we'll create a stub that halts (the batch commands would need
        // to be executed at a higher level).
        //
        // NOTE: This is a simplified implementation. Full batch execution would
        // require integration with the emulation loop to process batch commands
        // between executing external programs.
        
        // Set up a minimal execution state - the emulation loop will need to
        // check for batch processing after each instruction/program completion
        SetupMinimalExecutionState();

        return fileContent;
    }

    /// <summary>
    /// Sets up a minimal execution state for batch file processing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Current limitations of batch file execution:
    /// </para>
    /// <list type="bullet">
    /// <item>Internal commands (ECHO, SET, IF, etc.) are parsed but not automatically executed</item>
    /// <item>External program execution (e.g., "maupiti1.exe" in a batch) requires emulation loop integration</item>
    /// <item>The BatchProcessor.ReadNextLine() and ParseCommand() methods need to be called from the emulation loop</item>
    /// </list>
    /// <para>
    /// To complete the implementation, the emulation loop should:
    /// 1. Check CommandCom.BatchProcessor.IsProcessingBatch after each program exits
    /// 2. Call ReadNextLine() and ParseCommand() to get the next batch command
    /// 3. Execute the command (for ExecuteProgram commands, call Exec())
    /// 4. Repeat until the batch file is complete
    /// </para>
    /// </remarks>
    private void SetupMinimalExecutionState() {
        // For now, we'll just set up the state to point to a HLT instruction
        // The batch processing needs to be driven at a higher level
        // This is a temporary solution until full batch integration is complete
        
        // The emulation should check IsProcessingBatch on the BatchProcessor
        // and process batch lines accordingly

        if (_loggerService.IsEnabled(LogEventLevel.Warning)) {
            _loggerService.Warning(
                "BatchFileLoader: Batch file execution is partially implemented. " +
                "Internal batch commands are parsed but external program execution requires " +
                "emulation loop integration. Check BatchProcessor.IsProcessingBatch after each program exits.");
        }
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
