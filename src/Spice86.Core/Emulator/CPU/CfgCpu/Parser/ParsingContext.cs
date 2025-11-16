namespace Spice86.Core.Emulator.CPU.CfgCpu.Parser;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.ModRm;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Shared.Emulator.Memory;

using System.Linq;
using System.Numerics;

/// <summary>
/// Represents the ParsingContext class.
/// </summary>
public class ParsingContext : ModRmParsingContext {
    /// <summary>
    /// Gets or sets the Address.
    /// </summary>
    public SegmentedAddress Address { get; }
    /// <summary>
    /// Gets or sets the OpcodeField.
    /// </summary>
    public InstructionField<ushort> OpcodeField { get; }
    /// <summary>
    /// Gets or sets the Prefixes.
    /// </summary>
    public List<InstructionPrefix> Prefixes { get; }
    /// <summary>
    /// Gets or sets the AddressWidthFromPrefixes.
    /// </summary>
    public BitWidth AddressWidthFromPrefixes { get; }
    public int? SegmentOverrideFromPrefixes { get; }
    /// <summary>
    /// Gets or sets the HasOperandSize32.
    /// </summary>
    public bool HasOperandSize32 { get; }

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