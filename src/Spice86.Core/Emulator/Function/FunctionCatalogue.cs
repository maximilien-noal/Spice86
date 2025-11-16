namespace Spice86.Core.Emulator.Function;

using Spice86.Shared.Emulator.Memory;

using System.Linq;

/// <summary>
/// Represents the FunctionCatalogue class.
/// </summary>
public class FunctionCatalogue {
    public FunctionCatalogue() : this(new List<FunctionInformation>()) {
    }

    public FunctionCatalogue(IEnumerable<FunctionInformation> functionInformations) {
        FunctionInformations = functionInformations.ToDictionary(f => f.Address, f => f);
    }

    /// <summary>
    /// Gets or sets the FunctionInformations.
    /// </summary>
    public IDictionary<SegmentedAddress, FunctionInformation> FunctionInformations { get; }

    /// <summary>
    /// GetOrCreateFunctionInformation method.
    /// </summary>
    public FunctionInformation GetOrCreateFunctionInformation(SegmentedAddress entryAddress, string? name) {
        if (!FunctionInformations.TryGetValue(entryAddress, out FunctionInformation? res)) {
            res = new FunctionInformation(entryAddress, string.IsNullOrWhiteSpace(name) ? "unknown" : name);
            FunctionInformations.Add(entryAddress, res);
        }
        return res;
    }

    public FunctionInformation? GetFunctionInformation(FunctionCall? functionCall) {
        if (functionCall == null) {
            return null;
        }
        return FunctionInformations.TryGetValue(functionCall.Value.EntryPointAddress, out FunctionInformation? value) ? value : null;
    }

}