namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.CommonGrammar;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// The class.
/// </summary>
public abstract class EnterInstruction : CfgInstruction {
    public EnterInstruction(SegmentedAddress address,
        InstructionField<ushort> opcodeField, List<InstructionPrefix> prefixes,
        InstructionField<ushort> storageField,
        InstructionField<byte> levelField,
        int? maxSuccessorsCount) : base(address, opcodeField, prefixes, maxSuccessorsCount) {
        StorageField = storageField;
        LevelField = levelField;
        AddField(StorageField);
        AddField(LevelField);
    }
    /// <summary>
    /// Gets or sets the StorageField.
    /// </summary>
    public InstructionField<ushort> StorageField { get; }
    /// <summary>
    /// Gets or sets the LevelField.
    /// </summary>
    public InstructionField<byte> LevelField { get; }
}