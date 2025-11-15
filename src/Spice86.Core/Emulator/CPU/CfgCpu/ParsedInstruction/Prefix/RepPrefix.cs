namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;

/// <summary>
/// Represents rep prefix.
/// </summary>
public class RepPrefix : InstructionPrefix {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="prefixField">The prefix field.</param>
    /// <param name="continueZeroFlagValue">The continue zero flag value.</param>
    public RepPrefix(InstructionField<byte> prefixField, bool continueZeroFlagValue) : base(prefixField) {
        ContinueZeroFlagValue = continueZeroFlagValue;
    }

    /// <summary>
    /// Gets continue zero flag value.
    /// </summary>
    public bool ContinueZeroFlagValue { get; }
}