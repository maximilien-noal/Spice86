namespace Spice86.Core.Emulator.OperatingSystem.Command.BatchProcessing;

using System;

/// <summary>
/// Provides an interface for reading lines from a batch file.
/// </summary>
/// <remarks>
/// This abstraction allows batch file reading through different sources:
/// - Host file system (for testing and standalone usage)
/// - DOS file system (for full emulation integration)
/// - In-memory content (for auto-generated batch files)
/// Based on DOSBox staging's LineReader pattern.
/// </remarks>
public interface IBatchLineReader : IDisposable {
    /// <summary>
    /// Reads the next line from the batch file.
    /// </summary>
    /// <returns>The next line, or null if end of file or error.</returns>
    string? ReadLine();

    /// <summary>
    /// Resets the reader to the beginning of the file.
    /// </summary>
    /// <returns>True if reset was successful, false otherwise.</returns>
    bool Reset();
}
