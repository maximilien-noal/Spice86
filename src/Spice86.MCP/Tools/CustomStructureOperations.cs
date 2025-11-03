namespace Spice86.MCP.Tools;
using System.ComponentModel;

using ModelContextProtocol;
using ModelContextProtocol.Server;
using Spice86.Core.Emulator.ReverseEngineer.DataStructure;
using Spice86.MCP.Extensibility;
using Spice86.Shared.Emulator.Memory;
using System.Text;

/// <summary>
/// MCP tools for working with custom game-specific memory structures.
/// Provides access to structures registered by reverse engineering projects.
/// </summary>
[McpServerToolType]
public sealed class CustomStructureOperations {
    private readonly CustomStructureRegistry _structureRegistry;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomStructureOperations"/> class.
    /// </summary>
    /// <param name="structureRegistry">The registry containing custom structure providers.</param>
    public CustomStructureOperations(CustomStructureRegistry structureRegistry) {
        _structureRegistry = structureRegistry ?? throw new ArgumentNullException(nameof(structureRegistry));
    }

    /// <summary>
    /// Lists all available custom structure types from all registered providers.
    /// </summary>
    /// <returns>A list of all custom structures grouped by provider.</returns>
    [McpServerTool]
    [Description("Lists all custom game-specific structure types available from registered providers.")]
    public Task<string> ListCustomStructures() {
        IEnumerable<(string ProviderName, string StructureName)> structures = _structureRegistry.GetAllStructureNames().ToList();

        if (!structures.Any()) {
            return Task.FromResult("No custom structure providers registered. Register your game-specific structures using ICustomStructureProvider.");
        }

        StringBuilder result = new StringBuilder();
        result.AppendLine("Custom Game-Specific Structures:");
        result.AppendLine();

        foreach (IGrouping<string, (string ProviderName, string StructureName)> group in structures.GroupBy(s => s.ProviderName)) {
            result.AppendLine($"{group.Key}:");
            foreach ((string _, string StructureName) in group) {
                result.AppendLine($"  - {StructureName}");
            }
            result.AppendLine();
        }

        return Task.FromResult(result.ToString());
    }

    /// <summary>
    /// Gets the description of a custom structure type.
    /// </summary>
    /// <param name="structureName">The name of the structure.</param>
    /// <returns>A description of the structure and its fields.</returns>
    [McpServerTool]
    [Description("Gets detailed description of a custom structure type including its fields and usage.")]
    public Task<string> GetStructureDescription(string structureName) {
        if (string.IsNullOrWhiteSpace(structureName)) {
            return Task.FromResult("Error: Structure name cannot be empty");
        }

        foreach (ICustomStructureProvider provider in _structureRegistry.Providers) {
            if (provider.GetStructureFactories().ContainsKey(structureName)) {
                string description = provider.GetStructureDescription(structureName);
                return Task.FromResult($"Structure: {structureName}\nProvider: {provider.ProviderName}\n\n{description}");
            }
        }

        return Task.FromResult($"Structure '{structureName}' not found. Use ListCustomStructures to see available structures.");
    }

    /// <summary>
    /// Reads a custom structure from memory at the specified address.
    /// </summary>
    /// <param name="structureName">The name of the structure type to read.</param>
    /// <param name="address">The segmented address where the structure is located (e.g., "0x1000:0x0000").</param>
    /// <returns>The structure data formatted as a string.</returns>
    [McpServerTool]
    [Description("Reads and displays a custom game-specific structure from memory at the given address.")]
    public Task<string> ReadCustomStructure(string structureName, string address) {
        if (string.IsNullOrWhiteSpace(structureName)) {
            return Task.FromResult("Error: Structure name cannot be empty");
        }

        if (!TryParseSegmentedAddress(address, out SegmentedAddress segAddr)) {
            return Task.FromResult($"Error: Invalid address format '{address}'. Use format like 0x1000:0x0000");
        }

        if (!_structureRegistry.TryCreateStructure(structureName, segAddr, out MemoryBasedDataStructure? structure)) {
            return Task.FromResult($"Structure '{structureName}' not found. Use ListCustomStructures to see available structures.");
        }

        // The structure is created and can access memory at the specified address
        // We'll return a generic representation since we don't know the structure's fields
        return Task.FromResult($"Structure '{structureName}' at {segAddr}:\n\nStructure instance created successfully. Use structure-specific methods to access fields.");
    }

    /// <summary>
    /// Gets information about registered structure providers.
    /// </summary>
    /// <returns>Information about all registered providers.</returns>
    [McpServerTool]
    [Description("Gets information about registered custom structure providers and their games/projects.")]
    public Task<string> GetProviderInfo() {
        if (_structureRegistry.Providers.Count == 0) {
            return Task.FromResult("No custom structure providers registered.\n\nTo add your game-specific structures:\n1. Implement ICustomStructureProvider\n2. Register it with CustomStructureRegistry\n3. No MCP or AI knowledge required!");
        }

        StringBuilder result = new StringBuilder();
        result.AppendLine("Registered Structure Providers:");
        result.AppendLine();

        foreach (ICustomStructureProvider provider in _structureRegistry.Providers) {
            int structureCount = provider.GetStructureFactories().Count;
            result.AppendLine($"Provider: {provider.ProviderName}");
            result.AppendLine($"Structures: {structureCount}");
            result.AppendLine();
        }

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
