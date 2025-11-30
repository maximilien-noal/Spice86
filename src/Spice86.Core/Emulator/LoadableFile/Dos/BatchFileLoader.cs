namespace Spice86.Core.Emulator.LoadableFile.Dos;

using Serilog.Events;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.OperatingSystem;
using Spice86.Core.Emulator.OperatingSystem.Command;
using Spice86.Core.Emulator.OperatingSystem.Command.BatchProcessing;
using Spice86.Shared.Emulator.Errors;
using Spice86.Shared.Interfaces;

using System;
using System.IO;

/// <summary>
/// Loader for DOS batch files (.BAT).
/// </summary>
/// <remarks>
/// <para>
/// Batch files are interpreted line-by-line by COMMAND.COM. This loader uses
/// <see cref="BatchExecutor"/> to process the batch file and launch the first
/// executable program (EXE/COM).
/// </para>
/// <para>
/// For SHA256 hash verification, this loader returns the content of the launched
/// executable (not the batch file), ensuring that code overrides can be properly
/// verified against the actual program being executed.
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

        // Parse arguments into array
        string[] args = ParseArguments(arguments);

        // Get the host path to the batch file
        string hostPath = Path.GetFullPath(file);

        // Get the batch processor from CommandCom
        CommandCom commandCom = _dos.ProcessManager.CommandCom;
        BatchProcessor batchProcessor = commandCom.BatchProcessor;

        // Create the batch executor to process the batch file
        BatchExecutor executor = new(_dos, batchProcessor, _loggerService);

        // Execute the batch file until a program is launched
        bool launched = executor.ExecuteUntilProgram(hostPath, args);

        if (!launched || executor.LaunchedExecutableContent is null) {
            throw new UnrecoverableException(
                $"Batch file '{file}' did not launch any executable program");
        }

        if (_loggerService.IsEnabled(LogEventLevel.Information)) {
            _loggerService.Information(
                "BatchFileLoader: Batch file executed, launched program '{Program}'",
                executor.LaunchedExecutablePath);
        }

        // Return the EXECUTABLE content for SHA256 verification (not the batch file)
        // This ensures that code overrides can be verified against the actual program
        return executor.LaunchedExecutableContent;
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
