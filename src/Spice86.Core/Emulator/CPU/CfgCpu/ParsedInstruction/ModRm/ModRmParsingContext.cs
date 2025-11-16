namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.ModRm;

using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Defines the contract for ModRmParsingContext.
/// </summary>
public interface ModRmParsingContext {
    /// <summary>
    /// Gets or sets the AddressWidthFromPrefixes.
    /// </summary>
    public BitWidth AddressWidthFromPrefixes { get; }
    public int? SegmentOverrideFromPrefixes { get; }
}