namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.ModRm;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Shared.Emulator.Memory;

using System.Linq;
using System.Numerics;

/// <summary>
/// Represents parsing context.
/// </summary>
public class ParsingContext : ModRmParsingContext {
    /// <summary>
    /// Gets address.
    /// </summary>
    public SegmentedAddress Address { get; }
    /// <summary>
    /// Gets opcode field.
    /// </summary>
    public InstructionField<ushort> OpcodeField { get; }
    /// <summary>
    /// Gets prefixes.
    /// </summary>
    public List<InstructionPrefix> Prefixes { get; }
    /// <summary>
    /// Gets address width from prefixes.
    /// </summary>
    public BitWidth AddressWidthFromPrefixes { get; }
    /// <summary>
    /// Gets segment override from prefixes.
    /// </summary>
    public int? SegmentOverrideFromPrefixes { get; }
    /// <summary>
    /// Gets has operand size 32.
    /// </summary>
    public bool HasOperandSize32 { get; }

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="opcodeField">The opcode field.</param>
    /// <param name="prefixes">The prefixes.</param>
    public ParsingContext(SegmentedAddress address, InstructionField<ushort> opcodeField,
        List<InstructionPrefix> prefixes) {
        Address = address;
        OpcodeField = opcodeField;
        Prefixes = prefixes;
        AddressWidthFromPrefixes = ComputeAddressSize(prefixes);
        SegmentOverrideFromPrefixes = ComputeSegmentOverrideIndex(prefixes);
        HasOperandSize32 = ComputeHasOperandSize32(prefixes);
    }

    private static int? ComputeSegmentOverrideIndex(List<InstructionPrefix> prefixes) {
        SegmentOverrideInstructionPrefix? overridePrefix =
            prefixes.OfType<SegmentOverrideInstructionPrefix>().FirstOrDefault();
        return overridePrefix?.SegmentRegisterIndexValue;
    }

    private static BitWidth ComputeAddressSize(List<InstructionPrefix> prefixes) {
        AddressSize32Prefix? addressSize32Prefix = prefixes.OfType<AddressSize32Prefix>().FirstOrDefault();
        return addressSize32Prefix == null ? BitWidth.WORD_16 : BitWidth.DWORD_32;
    }

    private static bool ComputeHasOperandSize32(IList<InstructionPrefix> prefixes) {
        return prefixes.Any(p => p is OperandSize32Prefix);
    }
}