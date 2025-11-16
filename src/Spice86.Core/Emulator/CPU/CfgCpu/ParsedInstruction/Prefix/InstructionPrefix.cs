namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;

/// <summary>
/// Represents the InstructionPrefix class.
/// </summary>
public class InstructionPrefix {
    public InstructionPrefix(InstructionField<byte> prefixField) {
        PrefixField = prefixField;
    }

    /// <summary>
    /// Gets or sets the PrefixField.
    /// </summary>
    public InstructionField<byte> PrefixField { get; }
}