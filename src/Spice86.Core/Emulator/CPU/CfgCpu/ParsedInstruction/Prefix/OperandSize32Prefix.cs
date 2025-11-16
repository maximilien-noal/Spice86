namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;

/// <summary>
/// Represents the OperandSize32Prefix class.
/// </summary>
public class OperandSize32Prefix : InstructionPrefix {
    public OperandSize32Prefix(InstructionField<byte> prefixField) : base(prefixField) {
    }
}