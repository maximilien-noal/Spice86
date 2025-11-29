namespace Spice86.Core.Emulator.OperatingSystem;

using Serilog.Events;

using Spice86.Shared.Interfaces;

using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// Processes DOS batch files (.BAT) for COMMAND.COM.
/// </summary>
/// <remarks>
/// <para>
/// This class implements DOS batch file processing as part of the COMMAND.COM emulation.
/// Based on FreeDOS FREECOM batch.c implementation.
/// </para>
/// <para>
/// Supported batch features:
/// <list type="bullet">
/// <item>ECHO ON/OFF command to control command echoing</item>
/// <item>@ prefix to suppress echoing of a single line</item>
/// <item>Parameter substitution (%0-%9)</item>
/// <item>Comment lines starting with REM or ::</item>
/// <item>Labels starting with :</item>
/// <item>Executing programs (.COM, .EXE) and other batch files</item>
/// </list>
/// </para>
/// <para>
/// Reference implementations:
/// <list type="bullet">
/// <item>FreeDOS FREECOM: https://github.com/FDOS/freecom</item>
/// <item>FreeDOS Kernel: https://github.com/FDOS/kernel</item>
/// <item>DOSBox Staging: https://github.com/dosbox-staging/dosbox-staging</item>
/// </list>
/// </para>
/// </remarks>
public sealed class BatchProcessor {
    private readonly ILoggerService _loggerService;

    /// <summary>
    /// Special separator characters for ECHO command.
    /// These characters can immediately follow ECHO to output the rest of the line.
    /// For example: ECHO. outputs an empty line, ECHO:hello outputs "hello".
    /// Based on FreeDOS FREECOM echo.c behavior.
    /// </summary>
    private static readonly char[] EchoSeparators = ['.', ',', ':', ';', '/', '[', '+', '(', '='];

    /// <summary>
    /// The ECHO state for the current batch context.
    /// When true, commands are echoed to stdout before execution.
    /// </summary>
    private bool _echoState = true;

    /// <summary>
    /// The current batch file being processed, or null if none.
    /// </summary>
    private BatchContext? _currentContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchProcessor"/> class.
    /// </summary>
    /// <param name="loggerService">The logger service for diagnostic output.</param>
    public BatchProcessor(ILoggerService loggerService) {
        _loggerService = loggerService;
    }

    /// <summary>
    /// Gets or sets the echo state.
    /// When true, commands are echoed to stdout before execution.
    /// </summary>
    public bool Echo {
        get => _echoState;
        set => _echoState = value;
    }

    /// <summary>
    /// Gets whether a batch file is currently being processed.
    /// </summary>
    public bool IsProcessingBatch => _currentContext is not null;

    /// <summary>
    /// Gets the current batch file path, or null if none.
    /// </summary>
    public string? CurrentBatchPath => _currentContext?.FilePath;

    /// <summary>
    /// Starts processing a batch file.
    /// </summary>
    /// <param name="batchFilePath">The full path to the batch file.</param>
    /// <param name="arguments">Command line arguments passed to the batch file.</param>
    /// <returns>True if the batch file was successfully opened, false otherwise.</returns>
    public bool StartBatch(string batchFilePath, string[] arguments) {
        if (string.IsNullOrWhiteSpace(batchFilePath)) {
            if (_loggerService.IsEnabled(LogEventLevel.Warning)) {
                _loggerService.Warning("BatchProcessor: Cannot start batch with empty path");
            }
            return false;
        }

        if (!File.Exists(batchFilePath)) {
            if (_loggerService.IsEnabled(LogEventLevel.Warning)) {
                _loggerService.Warning("BatchProcessor: Batch file not found: {Path}", batchFilePath);
            }
            return false;
        }

        if (_loggerService.IsEnabled(LogEventLevel.Information)) {
            _loggerService.Information(
                "BatchProcessor: Starting batch file '{Path}' with {ArgCount} arguments",
                batchFilePath, arguments.Length);
        }

        // Create a new batch context
        BatchContext newContext = new(batchFilePath, arguments, _echoState);

        // If there's an existing context, chain it (for nested batch files via CALL)
        if (_currentContext is not null) {
            newContext.Parent = _currentContext;
        }

        _currentContext = newContext;
        return true;
    }

    /// <summary>
    /// Reads the next line from the current batch file.
    /// </summary>
    /// <param name="shouldEcho">
    /// Set to true if this line should be echoed before execution,
    /// false if it should be executed silently (due to @ prefix or ECHO OFF).
    /// </param>
    /// <returns>
    /// The next command line to execute, or null if the batch file is complete or an error occurred.
    /// </returns>
    public string? ReadNextLine(out bool shouldEcho) {
        shouldEcho = false;

        if (_currentContext is null) {
            return null;
        }

        string? line = _currentContext.ReadLine();
        if (line is null) {
            // End of file or error - exit this batch context
            ExitBatch();
            return null;
        }

        // Trim whitespace
        line = line.Trim();

        // Skip empty lines
        if (string.IsNullOrEmpty(line)) {
            return ReadNextLine(out shouldEcho); // Recurse to get next line
        }

        // Check for @ prefix (suppress echo for this line)
        bool suppressEcho = false;
        if (line.StartsWith('@')) {
            suppressEcho = true;
            line = line[1..].TrimStart();
        }

        // Skip label lines (start with :)
        if (line.StartsWith(':')) {
            return ReadNextLine(out shouldEcho); // Recurse to get next line
        }

        // Skip REM comments
        if (IsRem(line)) {
            return ReadNextLine(out shouldEcho); // Recurse to get next line
        }

        // Determine if we should echo this line
        shouldEcho = _echoState && !suppressEcho;

        // Expand parameters (%0-%9)
        line = ExpandParameters(line, _currentContext);

        if (_loggerService.IsEnabled(LogEventLevel.Verbose)) {
            _loggerService.Verbose("BatchProcessor: Read line: '{Line}', shouldEcho={ShouldEcho}",
                line, shouldEcho);
        }

        return line;
    }

    /// <summary>
    /// Processes a batch command line and determines what action to take.
    /// </summary>
    /// <param name="commandLine">The command line to process.</param>
    /// <returns>A <see cref="BatchCommand"/> describing the action to take.</returns>
    public BatchCommand ParseCommand(string commandLine) {
        if (string.IsNullOrWhiteSpace(commandLine)) {
            return BatchCommand.Empty();
        }

        // Split the command line into command and arguments
        SplitCommand(commandLine, out string command, out string arguments);

        // Handle internal batch commands
        string upperCommand = command.ToUpperInvariant();

        // Special handling for ECHO with special separators (e.g., ECHO., ECHO:, etc.)
        // These must be handled before checking for exact "ECHO" match
        if (upperCommand.StartsWith("ECHO") && upperCommand.Length > 4) {
            char separator = command[4];
            if (IsEchoSeparator(separator)) {
                // ECHO<separator><message> - print the rest after the separator
                string message = command[5..];
                if (!string.IsNullOrEmpty(arguments)) {
                    message = message + " " + arguments;
                }
                return BatchCommand.PrintMessage(message.TrimStart());
            }
        }

        if (upperCommand == "ECHO") {
            return HandleEchoCommand(arguments);
        }

        if (upperCommand == "REM") {
            return BatchCommand.Empty(); // REM is a no-op
        }

        if (upperCommand == "GOTO") {
            return HandleGotoCommand(arguments);
        }

        if (upperCommand == "CALL") {
            return HandleCallCommand(arguments);
        }

        if (upperCommand == "SET") {
            return HandleSetCommand(arguments);
        }

        if (upperCommand == "IF") {
            return HandleIfCommand(arguments);
        }

        if (upperCommand == "SHIFT") {
            return HandleShiftCommand();
        }

        if (upperCommand == "PAUSE") {
            return BatchCommand.Pause();
        }

        if (upperCommand == "EXIT") {
            return BatchCommand.Exit();
        }

        // External command - execute program
        return BatchCommand.ExecuteProgram(command, arguments);
    }

    /// <summary>
    /// Exits the current batch file context.
    /// If there's a parent context (from CALL), returns to it.
    /// </summary>
    public void ExitBatch() {
        if (_currentContext is null) {
            return;
        }

        if (_loggerService.IsEnabled(LogEventLevel.Information)) {
            _loggerService.Information("BatchProcessor: Exiting batch file '{Path}'",
                _currentContext.FilePath);
        }

        // Restore echo state from the saved value
        _echoState = _currentContext.SavedEchoState;

        // If there's a parent context, return to it
        BatchContext? parent = _currentContext.Parent;
        _currentContext.Dispose();
        _currentContext = parent;
    }

    /// <summary>
    /// Seeks to a label in the current batch file for GOTO command.
    /// </summary>
    /// <param name="label">The label to seek to (without the leading colon).</param>
    /// <returns>True if the label was found, false otherwise.</returns>
    public bool GotoLabel(string label) {
        if (_currentContext is null || string.IsNullOrWhiteSpace(label)) {
            return false;
        }

        if (_loggerService.IsEnabled(LogEventLevel.Debug)) {
            _loggerService.Debug("BatchProcessor: GOTO searching for label '{Label}'", label);
        }

        return _currentContext.SeekToLabel(label);
    }

    /// <summary>
    /// Checks if the given line is a REM comment.
    /// </summary>
    private static bool IsRem(string line) {
        if (line.Length < 3) {
            return false;
        }

        string upper = line.ToUpperInvariant();
        
        // Check for "REM" followed by whitespace or end of line
        if (upper.StartsWith("REM") && (line.Length == 3 || char.IsWhiteSpace(line[3]))) {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Expands parameter placeholders (%0-%9) in the command line.
    /// </summary>
    private static string ExpandParameters(string line, BatchContext context) {
        StringBuilder result = new(line.Length);
        int i = 0;

        while (i < line.Length) {
            if (line[i] == '%' && i + 1 < line.Length) {
                char next = line[i + 1];
                
                // Check for %0-%9
                if (next >= '0' && next <= '9') {
                    int paramIndex = next - '0';
                    string value = context.GetParameter(paramIndex);
                    result.Append(value);
                    i += 2;
                    continue;
                }
                
                // Check for %% (escaped percent)
                if (next == '%') {
                    result.Append('%');
                    i += 2;
                    continue;
                }
            }

            result.Append(line[i]);
            i++;
        }

        return result.ToString();
    }

    /// <summary>
    /// Splits a command line into command and arguments.
    /// </summary>
    private static void SplitCommand(string commandLine, out string command, out string arguments) {
        commandLine = commandLine.Trim();
        
        int spaceIndex = -1;
        for (int i = 0; i < commandLine.Length; i++) {
            if (char.IsWhiteSpace(commandLine[i])) {
                spaceIndex = i;
                break;
            }
        }

        if (spaceIndex == -1) {
            command = commandLine;
            arguments = string.Empty;
        } else {
            command = commandLine[..spaceIndex];
            arguments = commandLine[(spaceIndex + 1)..].TrimStart();
        }
    }

    /// <summary>
    /// Handles the ECHO command.
    /// </summary>
    private BatchCommand HandleEchoCommand(string arguments) {
        // Trim arguments for comparison
        string upperArgs = arguments.Trim().ToUpperInvariant();

        // ECHO without arguments shows current state
        if (string.IsNullOrEmpty(upperArgs)) {
            return BatchCommand.ShowEchoState(_echoState);
        }

        // ECHO ON
        if (upperArgs == "ON") {
            _echoState = true;
            if (_loggerService.IsEnabled(LogEventLevel.Debug)) {
                _loggerService.Debug("BatchProcessor: ECHO ON");
            }
            return BatchCommand.Empty();
        }

        // ECHO OFF
        if (upperArgs == "OFF") {
            _echoState = false;
            if (_loggerService.IsEnabled(LogEventLevel.Debug)) {
                _loggerService.Debug("BatchProcessor: ECHO OFF");
            }
            return BatchCommand.Empty();
        }

        // Check for ECHO separators (display empty line or message)
        // This handles "ECHO " followed by a special separator character
        if (arguments.Length > 0 && IsEchoSeparator(arguments[0])) {
            // These are all valid separators that print the rest as-is
            return BatchCommand.PrintMessage(arguments[1..]);
        }

        // ECHO <message> - print the message
        return BatchCommand.PrintMessage(arguments);
    }

    /// <summary>
    /// Checks if a character is a valid ECHO separator.
    /// </summary>
    private static bool IsEchoSeparator(char c) {
        return Array.IndexOf(EchoSeparators, c) >= 0;
    }

    /// <summary>
    /// Handles the GOTO command.
    /// </summary>
    private BatchCommand HandleGotoCommand(string arguments) {
        string label = arguments.Trim();
        if (string.IsNullOrEmpty(label)) {
            if (_loggerService.IsEnabled(LogEventLevel.Warning)) {
                _loggerService.Warning("BatchProcessor: GOTO without label");
            }
            return BatchCommand.Empty();
        }

        // Remove leading colon if present
        if (label.StartsWith(':')) {
            label = label[1..];
        }

        return BatchCommand.Goto(label);
    }

    /// <summary>
    /// Handles the CALL command.
    /// </summary>
    private static BatchCommand HandleCallCommand(string arguments) {
        SplitCommand(arguments, out string batchFile, out string batchArgs);
        return BatchCommand.CallBatch(batchFile, batchArgs);
    }

    /// <summary>
    /// Handles the SET command.
    /// </summary>
    /// <remarks>
    /// SET without arguments shows all environment variables.
    /// SET NAME shows the value of NAME.
    /// SET NAME=VALUE sets NAME to VALUE.
    /// </remarks>
    private static BatchCommand HandleSetCommand(string arguments) {
        string trimmed = arguments.Trim();

        // SET without arguments - show all variables
        if (string.IsNullOrEmpty(trimmed)) {
            return BatchCommand.ShowVariables();
        }

        // Find the equals sign
        int equalsIndex = trimmed.IndexOf('=');
        if (equalsIndex == -1) {
            // SET NAME - show specific variable
            return BatchCommand.ShowVariable(trimmed.ToUpperInvariant());
        }

        // SET NAME=VALUE
        string name = trimmed[..equalsIndex].Trim().ToUpperInvariant();
        string value = trimmed[(equalsIndex + 1)..];

        if (string.IsNullOrEmpty(name)) {
            return BatchCommand.Empty();
        }

        return BatchCommand.SetVariable(name, value);
    }

    /// <summary>
    /// Handles the IF command.
    /// </summary>
    /// <remarks>
    /// Supports:
    /// - IF [NOT] EXIST filename command
    /// - IF [NOT] ERRORLEVEL number command
    /// - IF [NOT] string1==string2 command
    /// </remarks>
    private BatchCommand HandleIfCommand(string arguments) {
        string trimmed = arguments.Trim();
        bool negate = false;

        // Check for NOT
        if (trimmed.StartsWith("NOT ", StringComparison.OrdinalIgnoreCase)) {
            negate = true;
            trimmed = trimmed[4..].TrimStart();
        }

        // Check for EXIST
        if (trimmed.StartsWith("EXIST ", StringComparison.OrdinalIgnoreCase)) {
            string rest = trimmed[6..].TrimStart();
            return BatchCommand.If("EXIST", rest, negate);
        }

        // Check for ERRORLEVEL
        if (trimmed.StartsWith("ERRORLEVEL ", StringComparison.OrdinalIgnoreCase)) {
            string rest = trimmed[11..].TrimStart();
            return BatchCommand.If("ERRORLEVEL", rest, negate);
        }

        // String comparison (string1==string2)
        int doubleEquals = trimmed.IndexOf("==", StringComparison.Ordinal);
        if (doubleEquals > 0) {
            return BatchCommand.If("COMPARE", trimmed, negate);
        }

        if (_loggerService.IsEnabled(LogEventLevel.Warning)) {
            _loggerService.Warning("BatchProcessor: Invalid IF syntax: {Arguments}", arguments);
        }
        return BatchCommand.Empty();
    }

    /// <summary>
    /// Handles the SHIFT command.
    /// </summary>
    private BatchCommand HandleShiftCommand() {
        if (_currentContext is not null) {
            _currentContext.Shift();
        }
        return BatchCommand.Shift();
    }
}

/// <summary>
/// Represents the context of a batch file being processed.
/// </summary>
internal sealed class BatchContext : IDisposable {
    private readonly StreamReader _reader;
    private string[] _parameters;
    private int _shiftOffset = 0;

    /// <summary>
    /// Gets the full path to the batch file.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Gets or sets the parent batch context (for CALL).
    /// </summary>
    public BatchContext? Parent { get; set; }

    /// <summary>
    /// Gets the saved echo state from when this batch started.
    /// </summary>
    public bool SavedEchoState { get; }

    /// <summary>
    /// Initializes a new batch context.
    /// </summary>
    /// <param name="filePath">Path to the batch file.</param>
    /// <param name="arguments">Command line arguments.</param>
    /// <param name="currentEchoState">Current echo state to save.</param>
    public BatchContext(string filePath, string[] arguments, bool currentEchoState) {
        FilePath = filePath;
        SavedEchoState = currentEchoState;

        // Build parameters array: %0 is the batch file name, %1-%9 are arguments
        _parameters = new string[10];
        _parameters[0] = filePath;
        for (int i = 0; i < Math.Min(9, arguments.Length); i++) {
            _parameters[i + 1] = arguments[i];
        }
        // Fill remaining with empty strings
        for (int i = arguments.Length + 1; i < 10; i++) {
            _parameters[i] = string.Empty;
        }

        // Open the file for reading using ASCII encoding
        // Note: For full DOS compatibility, CP437 would be preferred but requires
        // registering System.Text.Encoding.CodePages provider at startup
        _reader = new StreamReader(filePath, Encoding.ASCII);
    }

    /// <summary>
    /// Gets a parameter by index (0-9).
    /// </summary>
    /// <param name="index">Parameter index (0 = batch file name, 1-9 = arguments).</param>
    /// <returns>The parameter value, or empty string if not defined.</returns>
    public string GetParameter(int index) {
        // Apply shift offset for parameters 1-9 (not %0 which is always the batch file)
        int actualIndex = index == 0 ? 0 : index + _shiftOffset;
        if (actualIndex < 0 || actualIndex >= _parameters.Length) {
            return string.Empty;
        }
        return _parameters[actualIndex];
    }

    /// <summary>
    /// Shifts the parameters by one position.
    /// After SHIFT, %1 becomes what was %2, %2 becomes what was %3, etc.
    /// </summary>
    public void Shift() {
        _shiftOffset++;
    }

    /// <summary>
    /// Reads the next line from the batch file.
    /// </summary>
    /// <returns>The next line, or null if end of file.</returns>
    public string? ReadLine() {
        return _reader.ReadLine();
    }

    /// <summary>
    /// Seeks to a label in the batch file.
    /// </summary>
    /// <param name="label">The label to find (without leading colon).</param>
    /// <returns>True if found, false otherwise.</returns>
    public bool SeekToLabel(string label) {
        // Reset to beginning of file
        _reader.BaseStream.Seek(0, SeekOrigin.Begin);
        _reader.DiscardBufferedData();

        string labelToFind = label.ToUpperInvariant();

        while (true) {
            string? line = _reader.ReadLine();
            if (line is null) {
                // End of file - label not found
                return false;
            }

            line = line.Trim();
            if (line.Length == 0 || line[0] != ':') {
                continue;
            }

            // Extract the label from the line
            string lineLabel = line[1..].Trim();
            
            // Label ends at first whitespace (find the first whitespace character)
            int spaceIndex = -1;
            for (int i = 0; i < lineLabel.Length; i++) {
                if (char.IsWhiteSpace(lineLabel[i])) {
                    spaceIndex = i;
                    break;
                }
            }
            if (spaceIndex >= 0) {
                lineLabel = lineLabel[..spaceIndex];
            }

            if (lineLabel.ToUpperInvariant() == labelToFind) {
                return true;
            }
        }
    }

    /// <summary>
    /// Disposes the batch context.
    /// </summary>
    public void Dispose() {
        _reader.Dispose();
    }
}

/// <summary>
/// Represents a batch command to be executed.
/// </summary>
public readonly struct BatchCommand {
    /// <summary>
    /// The type of command.
    /// </summary>
    public BatchCommandType Type { get; }

    /// <summary>
    /// The primary value (program name, label, or message depending on type).
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Secondary value (arguments for programs).
    /// </summary>
    public string Arguments { get; }

    /// <summary>
    /// For IF commands, indicates whether the condition should be negated (IF NOT).
    /// </summary>
    public bool Negate { get; }

    private BatchCommand(BatchCommandType type, string value = "", string arguments = "", bool negate = false) {
        Type = type;
        Value = value;
        Arguments = arguments;
        Negate = negate;
    }

    /// <summary>
    /// Creates an empty (no-op) command.
    /// </summary>
    public static BatchCommand Empty() => new(BatchCommandType.Empty);

    /// <summary>
    /// Creates a command to print a message.
    /// </summary>
    public static BatchCommand PrintMessage(string message) =>
        new(BatchCommandType.PrintMessage, message);

    /// <summary>
    /// Creates a command to show the current echo state.
    /// </summary>
    public static BatchCommand ShowEchoState(bool isOn) =>
        new(BatchCommandType.ShowEchoState, isOn ? "ON" : "OFF");

    /// <summary>
    /// Creates a command to execute an external program.
    /// </summary>
    public static BatchCommand ExecuteProgram(string program, string arguments) =>
        new(BatchCommandType.ExecuteProgram, program, arguments);

    /// <summary>
    /// Creates a GOTO command.
    /// </summary>
    public static BatchCommand Goto(string label) =>
        new(BatchCommandType.Goto, label);

    /// <summary>
    /// Creates a CALL command to invoke another batch file.
    /// </summary>
    public static BatchCommand CallBatch(string batchFile, string arguments) =>
        new(BatchCommandType.CallBatch, batchFile, arguments);

    /// <summary>
    /// Creates a SET command to set an environment variable.
    /// </summary>
    public static BatchCommand SetVariable(string name, string value) =>
        new(BatchCommandType.SetVariable, name, value);

    /// <summary>
    /// Creates a command to show all environment variables.
    /// </summary>
    public static BatchCommand ShowVariables() =>
        new(BatchCommandType.ShowVariables);

    /// <summary>
    /// Creates a command to show a specific environment variable.
    /// </summary>
    public static BatchCommand ShowVariable(string name) =>
        new(BatchCommandType.ShowVariable, name);

    /// <summary>
    /// Creates an IF conditional command.
    /// </summary>
    /// <param name="condition">The condition type (EXIST, ERRORLEVEL, or string comparison).</param>
    /// <param name="arguments">The condition arguments and command to execute.</param>
    /// <param name="negate">True if the condition should be negated (IF NOT).</param>
    public static BatchCommand If(string condition, string arguments, bool negate = false) =>
        new(BatchCommandType.If, condition, arguments, negate);

    /// <summary>
    /// Creates a SHIFT command.
    /// </summary>
    public static BatchCommand Shift() =>
        new(BatchCommandType.Shift);

    /// <summary>
    /// Creates a PAUSE command.
    /// </summary>
    public static BatchCommand Pause() =>
        new(BatchCommandType.Pause);

    /// <summary>
    /// Creates an EXIT command.
    /// </summary>
    public static BatchCommand Exit() =>
        new(BatchCommandType.Exit);
}

/// <summary>
/// Types of batch commands.
/// </summary>
public enum BatchCommandType {
    /// <summary>Empty command (no operation).</summary>
    Empty,

    /// <summary>Print a message to stdout.</summary>
    PrintMessage,

    /// <summary>Show the current ECHO state.</summary>
    ShowEchoState,

    /// <summary>Execute an external program.</summary>
    ExecuteProgram,

    /// <summary>GOTO a label.</summary>
    Goto,

    /// <summary>CALL another batch file.</summary>
    CallBatch,

    /// <summary>SET environment variable.</summary>
    SetVariable,

    /// <summary>Show all environment variables.</summary>
    ShowVariables,

    /// <summary>Show a specific environment variable.</summary>
    ShowVariable,

    /// <summary>IF conditional command.</summary>
    If,

    /// <summary>SHIFT parameters.</summary>
    Shift,

    /// <summary>PAUSE execution.</summary>
    Pause,

    /// <summary>EXIT batch file.</summary>
    Exit
}
