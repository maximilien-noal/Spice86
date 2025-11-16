namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;

/// <summary>
/// Represents the LockPrefix class.
/// </summary>
public class LockPrefix : InstructionPrefix {
    public LockPrefix(InstructionField<byte> prefixField) : base(prefixField) {
    }
}