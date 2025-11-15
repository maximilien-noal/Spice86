namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;

/// <summary>
/// Represents operand size 32 prefix.
/// </summary>
public class OperandSize32Prefix : InstructionPrefix {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="prefixField">The prefix field.</param>
    public OperandSize32Prefix(InstructionField<byte> prefixField) : base(prefixField) {
    }
}