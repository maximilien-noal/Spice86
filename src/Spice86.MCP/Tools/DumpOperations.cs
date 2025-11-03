namespace Spice86.MCP.Tools;
using System.ComponentModel;

using ModelContextProtocol;
using ModelContextProtocol.Server;
using Spice86.Core.Emulator.Function.Dump;
using Spice86.Core.Emulator.Memory;
using Spice86.Shared.Emulator.Memory;
using System.Text;

/// <summary>
/// MCP tools for dumping memory and execution flow data.
/// Provides access to memory dumps, execution flow recording, and analysis data export.
/// </summary>
[McpServerToolType]
public sealed class DumpOperations {
    private readonly IMemory _memory;
    private readonly ExecutionFlowDumper? _executionFlowDumper;

    /// <summary>
    /// Initializes a new instance of the <see cref="DumpOperations"/> class.
    /// </summary>
    /// <param name="memory">The memory interface.</param>
    /// <param name="executionFlowDumper">Optional execution flow dumper.</param>
    public DumpOperations(IMemory memory, ExecutionFlowDumper? executionFlowDumper = null) {
        _memory = memory;
        _executionFlowDumper = executionFlowDumper;
    }

    /// <summary>
    /// Dumps a region of memory to a hexadecimal string representation.
    /// </summary>
    /// <param name="address">The segmented address to start dumping from (e.g., "0xF000:0x0000").</param>
    /// <param name="length">The number of bytes to dump.</param>
    /// <returns>A hexadecimal representation of the memory region.</returns>
    [McpServerTool]
    [Description("Dumps a region of memory as hexadecimal. Returns the memory contents for analysis or saving.")]
    public Task<string> DumpMemoryRegion(string address, int length) {
        if (!TryParseSegmentedAddress(address, out SegmentedAddress segAddr)) {
            return Task.FromResult($"Error: Invalid address format '{address}'. Use format like 0xF000:0x0000");
        }

        if (length <= 0 || length > 65536) {
            return Task.FromResult($"Error: Length must be between 1 and 65536 bytes. Got: {length}");
        }

        StringBuilder result = new StringBuilder();
        result.AppendLine($"Memory dump from {segAddr} ({length} bytes):");
        result.AppendLine();

        uint linearAddr = segAddr.Linear;
        for (int offset = 0; offset < length; offset += 16) {
            uint currentAddr = linearAddr + (uint)offset;
            int remainingBytes = Math.Min(16, length - offset);

            // Address
            result.Append($"{currentAddr:X8}  ");

            // Hex bytes
            for (int i = 0; i < remainingBytes; i++) {
                byte value = _memory.UInt8[currentAddr + (uint)i];
                result.Append($"{value:X2} ");
            }

            // Padding for incomplete rows
            for (int i = remainingBytes; i < 16; i++) {
                result.Append("   ");
            }

            result.Append(" ");

            // ASCII representation
            for (int i = 0; i < remainingBytes; i++) {
                byte value = _memory.UInt8[currentAddr + (uint)i];
                char c = (value >= 32 && value < 127) ? (char)value : '.';
                result.Append(c);
            }

            result.AppendLine();
        }

        return Task.FromResult(result.ToString());
    }

    /// <summary>
    /// Dumps the entire memory to a binary file.
    /// </summary>
    /// <param name="filePath">The file path where the memory dump should be saved.</param>
    /// <returns>A status message indicating success or failure.</returns>
    [McpServerTool]
    [Description("Dumps the entire memory to a binary file for analysis. Useful for creating memory snapshots.")]
    public Task<string> DumpMemoryToFile(string filePath) {
        try {
            if (string.IsNullOrWhiteSpace(filePath)) {
                return Task.FromResult("Error: File path cannot be empty");
            }

            // Ensure directory exists
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }

            // Get the RAM and dump it
            if (_memory is Memory memory) {
                byte[] ramData = memory.ReadRam();
                File.WriteAllBytes(filePath, ramData);
                return Task.FromResult($"Memory dumped successfully to: {filePath} ({ramData.Length} bytes)");
            }

            return Task.FromResult("Error: Unable to access memory data for dumping");
        } catch (Exception ex) {
            return Task.FromResult($"Error dumping memory: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets information about the execution flow dumper status.
    /// </summary>
    /// <returns>Information about whether execution flow recording is available.</returns>
    [McpServerTool]
    [Description("Checks if execution flow dumping is available and provides status information.")]
    public Task<string> GetExecutionFlowDumpStatus() {
        if (_executionFlowDumper == null) {
            return Task.FromResult("Execution flow dumping is not available in this configuration.");
        }

        return Task.FromResult("Execution flow dumper is available and ready to record execution data.");
    }

    private bool TryParseSegmentedAddress(string addressStr, out SegmentedAddress address) {
        address = default;

        if (string.IsNullOrWhiteSpace(addressStr)) {
            return false;
        }

        // Try to parse as segmented address (e.g., 0xF000:0x1000 or F000:1000)
        if (addressStr.Contains(':')) {
            string[] parts = addressStr.Split(':');
            if (parts.Length != 2) {
                return false;
            }

            if (!TryParseHex(parts[0], out ushort segment) || !TryParseHex(parts[1], out ushort offset)) {
                return false;
            }

            address = new SegmentedAddress(segment, offset);
            return true;
        }

        // Try to parse as linear address
        if (TryParseHex(addressStr, out uint linear)) {
            // Convert to segmented (using segment = addr >> 4, offset = 0)
            address = new SegmentedAddress((ushort)(linear >> 4), 0);
            return true;
        }

        return false;
    }

    private bool TryParseHex(string value, out ushort result) {
        result = 0;
        string trimmed = value.Trim().Replace("0x", "").Replace("0X", "");
        return ushort.TryParse(trimmed, System.Globalization.NumberStyles.HexNumber, null, out result);
    }

    private bool TryParseHex(string value, out uint result) {
        result = 0;
        string trimmed = value.Trim().Replace("0x", "").Replace("0X", "");
        return uint.TryParse(trimmed, System.Globalization.NumberStyles.HexNumber, null, out result);
    }
}
