namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;

/// <summary>
/// Represents instruction prefix.
/// </summary>
public class InstructionPrefix {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="prefixField">The prefix field.</param>
    public InstructionPrefix(InstructionField<byte> prefixField) {
        PrefixField = prefixField;
    }

    /// <summary>
    /// Gets prefix field.
    /// </summary>
    public InstructionField<byte> PrefixField { get; }
}