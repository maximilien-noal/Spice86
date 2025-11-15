namespace Spice86.Core.Emulator.Function;

using Spice86.Shared.Emulator.Memory;

using System.Linq;

/// <summary>
/// Represents function catalogue.
/// </summary>
public class FunctionCatalogue {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public FunctionCatalogue() : this(new List<FunctionInformation>()) {
    }

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="functionInformations">The function informations.</param>
    public FunctionCatalogue(IEnumerable<FunctionInformation> functionInformations) {
        FunctionInformations = functionInformations.ToDictionary(f => f.Address, f => f);
    }

    /// <summary>
    /// Gets function informations.
    /// </summary>
    public IDictionary<SegmentedAddress, FunctionInformation> FunctionInformations { get; }

    /// <summary>
    /// Gets or create function information.
    /// </summary>
    /// <param name="entryAddress">The entry address.</param>
    /// <param name="name">The name.</param>
    /// <returns>The result of the operation.</returns>
    public FunctionInformation GetOrCreateFunctionInformation(SegmentedAddress entryAddress, string? name) {
        if (!FunctionInformations.TryGetValue(entryAddress, out FunctionInformation? res)) {
            res = new FunctionInformation(entryAddress, string.IsNullOrWhiteSpace(name) ? "unknown" : name);
            FunctionInformations.Add(entryAddress, res);
        }
        return res;
    }

    /// <summary>
    /// Gets function information.
    /// </summary>
    /// <param name="functionCall">The function call.</param>
    public FunctionInformation? GetFunctionInformation(FunctionCall? functionCall) {
        if (functionCall == null) {
            return null;
        }
        return FunctionInformations.TryGetValue(functionCall.Value.EntryPointAddress, out FunctionInformation? value) ? value : null;
    }

}