namespace Spice86.Core.Emulator.LoadableFile.Dos;

using Serilog.Events;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.OperatingSystem;
using Spice86.Core.Emulator.OperatingSystem.Command;
using Spice86.Core.Emulator.OperatingSystem.Command.BatchProcessing;
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
        string dosPath = _dos.FileManager.GetDosProgramPath(hostPath);

        // Use the autoexec bootstrap approach to execute the batch file
        // This follows DOSBox staging's pattern for program bootstrapping
        CommandCom commandCom = _dos.ProcessManager.CommandCom;
        AutoexecGenerator autoexec = CommandCom.CreateAutoexecForBatch(dosPath, arguments ?? "", exitAfter: true);
        commandCom.LoadAutoexecBatch(autoexec, "STARTUP.BAT");

        if (_loggerService.IsEnabled(LogEventLevel.Debug)) {
            _loggerService.Debug(
                "BatchFileLoader: Loaded batch file via autoexec bootstrap. Lines: {Lines}",
                autoexec.Generate().Length);
        }

        return fileContent;
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
