namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;

/// <summary>
/// Represents the RepPrefix class.
/// </summary>
public class RepPrefix : InstructionPrefix {
    public RepPrefix(InstructionField<byte> prefixField, bool continueZeroFlagValue) : base(prefixField) {
        ContinueZeroFlagValue = continueZeroFlagValue;
    }

    /// <summary>
    /// Gets or sets the ContinueZeroFlagValue.
    /// </summary>
    public bool ContinueZeroFlagValue { get; }
}