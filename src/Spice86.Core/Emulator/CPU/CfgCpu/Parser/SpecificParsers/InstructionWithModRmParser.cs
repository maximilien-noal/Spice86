namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser.SpecificParsers;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.ModRm;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Defines the contract for i instruction with mod rm factory.
/// </summary>
public interface IInstructionWithModRmFactory {
    public CfgInstruction Parse(ParsingContext context, ModRmContext modRmContext, BitWidth bitWidth);
}