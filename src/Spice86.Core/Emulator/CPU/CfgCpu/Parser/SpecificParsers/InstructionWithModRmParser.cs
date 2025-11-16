namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.SpecificParsers;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.ModRm;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Defines the contract for IInstructionWithModRmFactory.
/// </summary>
public interface IInstructionWithModRmFactory {
    /// <summary>
    /// Parse method.
    /// </summary>
    public CfgInstruction Parse(ParsingContext context, ModRmContext modRmContext, BitWidth bitWidth);
}