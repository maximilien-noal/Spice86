namespace Spice86.MCP.Extensibility;

using Spice86.Core.Emulator.ReverseEngineer.DataStructure;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Interface for providing custom game-specific memory data structures to the MCP server.
/// Implement this interface to expose your reverse-engineered game structures without
/// needing to understand MCP or AI internals.
/// </summary>
public interface ICustomStructureProvider {
    /// <summary>
    /// Gets the name of this structure provider (e.g., "Dune", "MyGame").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets the collection of custom structure definitions.
    /// </summary>
    /// <returns>A dictionary mapping structure names to their factory functions.</returns>
    IReadOnlyDictionary<string, Func<SegmentedAddress, MemoryBasedDataStructure>> GetStructureFactories();

    /// <summary>
    /// Gets a description of a structure type for documentation purposes.
    /// </summary>
    /// <param name="structureName">The name of the structure.</param>
    /// <returns>A human-readable description of the structure and its fields.</returns>
    string GetStructureDescription(string structureName);
}

/// <summary>
/// Registry for custom structure providers. Users can register their game-specific
/// structures here to make them available through the MCP server.
/// </summary>
public sealed class CustomStructureRegistry {
    private readonly List<ICustomStructureProvider> _providers = new();

    /// <summary>
    /// Registers a custom structure provider.
    /// </summary>
    /// <param name="provider">The structure provider to register.</param>
    public void RegisterProvider(ICustomStructureProvider provider) {
        if (provider == null) {
            throw new ArgumentNullException(nameof(provider));
        }

        if (_providers.Any(p => p.ProviderName == provider.ProviderName)) {
            throw new InvalidOperationException($"A provider with name '{provider.ProviderName}' is already registered");
        }

        _providers.Add(provider);
    }

    /// <summary>
    /// Gets all registered providers.
    /// </summary>
    public IReadOnlyList<ICustomStructureProvider> Providers => _providers.AsReadOnly();

    /// <summary>
    /// Tries to create a structure instance from any registered provider.
    /// </summary>
    /// <param name="structureName">The name of the structure to create.</param>
    /// <param name="address">The address where the structure is located.</param>
    /// <param name="structure">The created structure, or null if not found.</param>
    /// <returns>True if the structure was found and created, false otherwise.</returns>
    public bool TryCreateStructure(string structureName, SegmentedAddress address, out MemoryBasedDataStructure? structure) {
        structure = null;

        foreach (ICustomStructureProvider provider in _providers) {
            IReadOnlyDictionary<string, Func<SegmentedAddress, MemoryBasedDataStructure>> factories = provider.GetStructureFactories();
            if (factories.TryGetValue(structureName, out Func<SegmentedAddress, MemoryBasedDataStructure>? factory)) {
                structure = factory(address);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets a list of all available structure names from all providers.
    /// </summary>
    public IEnumerable<(string ProviderName, string StructureName)> GetAllStructureNames() {
        foreach (ICustomStructureProvider provider in _providers) {
            foreach (string structureName in provider.GetStructureFactories().Keys) {
                yield return (provider.ProviderName, structureName);
            }
        }
    }
}
