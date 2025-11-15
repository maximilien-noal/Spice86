namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;

/// <summary>
/// Represents lock prefix.
/// </summary>
public class LockPrefix : InstructionPrefix {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="prefixField">The prefix field.</param>
    public LockPrefix(InstructionField<byte> prefixField) : base(prefixField) {
    }
}