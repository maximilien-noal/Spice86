namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.ModRm;

using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Defines the contract for mod rm parsing context.
/// </summary>
public interface ModRmParsingContext {
    /// <summary>
    /// Gets address width from prefixes.
    /// </summary>
    public BitWidth AddressWidthFromPrefixes { get; }
    /// <summary>
    /// Gets segment override from prefixes.
    /// </summary>
    public int? SegmentOverrideFromPrefixes { get; }
}