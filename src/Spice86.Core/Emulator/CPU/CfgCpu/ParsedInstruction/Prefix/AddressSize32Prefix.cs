namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;

/// <summary>
/// Represents the AddressSize32Prefix class.
/// </summary>
public class AddressSize32Prefix : InstructionPrefix {
    public AddressSize32Prefix(InstructionField<byte> prefixField) : base(prefixField) {
    }
}