namespace Spice86.MCP.Tools;
using System.ComponentModel;

using ModelContextProtocol;
using ModelContextProtocol.Server;
using Spice86.Core.Emulator.Function;
using Spice86.Shared.Emulator.Memory;
using System.Text;

/// <summary>
/// MCP tools for querying and exploring function information from reverse engineering.
/// Provides access to the FunctionInformations dictionary populated during reverse engineering.
/// </summary>
[McpServerToolType]
public sealed class FunctionInformationOperations {
    private readonly IDictionary<SegmentedAddress, FunctionInformation>? _functionInformations;

    /// <summary>
    /// Initializes a new instance of the <see cref="FunctionInformationOperations"/> class.
    /// </summary>
    /// <param name="functionInformations">Dictionary of function information from reverse engineering.</param>
    public FunctionInformationOperations(IDictionary<SegmentedAddress, FunctionInformation>? functionInformations = null) {
        _functionInformations = functionInformations;
    }

    /// <summary>
    /// Lists all known functions from reverse engineering.
    /// </summary>
    /// <returns>A list of all functions with their addresses and names.</returns>
    [McpServerTool]
    [Description("Lists all reverse-engineered functions with their addresses and names.")]
    public Task<string> ListFunctions() {
        if (_functionInformations == null || _functionInformations.Count == 0) {
            return Task.FromResult("No function information available. Functions are populated during reverse engineering.");
        }

        StringBuilder result = new StringBuilder();
        result.AppendLine($"Reverse-Engineered Functions ({_functionInformations.Count} total):");
        result.AppendLine();

        foreach (KeyValuePair<SegmentedAddress, FunctionInformation> kvp in _functionInformations.OrderBy(x => x.Key.Linear)) {
            SegmentedAddress addr = kvp.Key;
            FunctionInformation info = kvp.Value;
            string overrideStatus = info.FunctionOverride != null ? " [HAS C# OVERRIDE]" : "";
            result.AppendLine($"{addr} - {info.Name}{overrideStatus}");
        }

        return Task.FromResult(result.ToString());
    }

    /// <summary>
    /// Gets detailed information about a specific function.
    /// </summary>
    /// <param name="address">The segmented address of the function (e.g., "0xF000:0x1000").</param>
    /// <returns>Detailed information about the function including callers and returns.</returns>
    [McpServerTool]
    [Description("Gets detailed information about a specific function including callers, returns, and override status.")]
    public Task<string> GetFunctionDetails(string address) {
        if (_functionInformations == null) {
            return Task.FromResult("No function information available.");
        }

        if (!TryParseSegmentedAddress(address, out SegmentedAddress segAddr)) {
            return Task.FromResult($"Error: Invalid address format '{address}'. Use format like 0xF000:0x1000");
        }

        if (!_functionInformations.TryGetValue(segAddr, out FunctionInformation? info)) {
            return Task.FromResult($"No function found at address {segAddr}");
        }

        StringBuilder result = new StringBuilder();
        result.AppendLine($"Function Details for {segAddr}:");
        result.AppendLine();
        result.AppendLine($"Name: {info.Name}");
        result.AppendLine($"Address: {segAddr}");
        result.AppendLine($"Has C# Override: {(info.FunctionOverride != null ? "Yes" : "No")}");
        result.AppendLine();

        // Callers
        if (info.Callers.Count > 0) {
            result.AppendLine($"Called by ({info.Callers.Count} functions):");
            foreach (FunctionInformation caller in info.Callers.OrderBy(c => c.Address.Linear)) {
                result.AppendLine($"  - {caller.Address}: {caller.Name}");
            }
        } else {
            result.AppendLine("No known callers");
        }
        result.AppendLine();

        // Returns
        if (info.Returns != null && info.Returns.Count > 0) {
            result.AppendLine("Return points:");
            foreach (KeyValuePair<FunctionReturn, HashSet<SegmentedAddress>> ret in info.Returns) {
                result.AppendLine($"  {ret.Key}: {ret.Value.Count} location(s)");
            }
        } else {
            result.AppendLine("No return information available");
        }

        return Task.FromResult(result.ToString());
    }

    /// <summary>
    /// Searches for functions by name pattern.
    /// </summary>
    /// <param name="namePattern">The pattern to search for in function names (case-insensitive).</param>
    /// <returns>A list of matching functions.</returns>
    [McpServerTool]
    [Description("Searches for functions whose names contain the specified pattern (case-insensitive).")]
    public Task<string> SearchFunctions(string namePattern) {
        if (_functionInformations == null || _functionInformations.Count == 0) {
            return Task.FromResult("No function information available.");
        }

        if (string.IsNullOrWhiteSpace(namePattern)) {
            return Task.FromResult("Error: Search pattern cannot be empty");
        }

        IEnumerable<KeyValuePair<SegmentedAddress, FunctionInformation>> matches = _functionInformations
            .Where(kvp => kvp.Value.Name.Contains(namePattern, StringComparison.OrdinalIgnoreCase))
            .OrderBy(kvp => kvp.Key.Linear);

        if (!matches.Any()) {
            return Task.FromResult($"No functions found matching pattern: {namePattern}");
        }

        StringBuilder result = new StringBuilder();
        result.AppendLine($"Functions matching '{namePattern}' ({matches.Count()} found):");
        result.AppendLine();

        foreach (KeyValuePair<SegmentedAddress, FunctionInformation> match in matches) {
            string overrideStatus = match.Value.FunctionOverride != null ? " [C# OVERRIDE]" : "";
            result.AppendLine($"{match.Key} - {match.Value.Name}{overrideStatus}");
        }

        return Task.FromResult(result.ToString());
    }

    /// <summary>
    /// Gets statistics about the function information database.
    /// </summary>
    /// <returns>Statistics about reverse-engineered functions.</returns>
    [McpServerTool]
    [Description("Gets statistics about reverse-engineered functions (total count, overrides, etc.).")]
    public Task<string> GetFunctionStatistics() {
        if (_functionInformations == null || _functionInformations.Count == 0) {
            return Task.FromResult("No function information available.");
        }

        int totalFunctions = _functionInformations.Count;
        int functionsWithOverrides = _functionInformations.Count(kvp => kvp.Value.FunctionOverride != null);
        int functionsWithCallers = _functionInformations.Count(kvp => kvp.Value.Callers.Count > 0);
        int functionsWithReturns = _functionInformations.Count(kvp => kvp.Value.Returns != null && kvp.Value.Returns.Count > 0);

        StringBuilder result = new StringBuilder();
        result.AppendLine("Function Information Statistics:");
        result.AppendLine();
        result.AppendLine($"Total Functions: {totalFunctions}");
        result.AppendLine($"Functions with C# Overrides: {functionsWithOverrides} ({(double)functionsWithOverrides / totalFunctions * 100:F1}%)");
        result.AppendLine($"Functions with Known Callers: {functionsWithCallers} ({(double)functionsWithCallers / totalFunctions * 100:F1}%)");
        result.AppendLine($"Functions with Return Info: {functionsWithReturns} ({(double)functionsWithReturns / totalFunctions * 100:F1}%)");

        return Task.FromResult(result.ToString());
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

        return false;
    }

    private bool TryParseHex(string value, out ushort result) {
        result = 0;
        string trimmed = value.Trim().Replace("0x", "").Replace("0X", "");
        return ushort.TryParse(trimmed, System.Globalization.NumberStyles.HexNumber, null, out result);
    }
}
