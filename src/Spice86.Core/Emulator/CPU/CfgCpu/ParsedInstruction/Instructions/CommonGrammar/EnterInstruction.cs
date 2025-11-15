namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.CommonGrammar;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents enter instruction.
/// </summary>
public abstract class EnterInstruction : CfgInstruction {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="opcodeField">The opcode field.</param>
    /// <param name="prefixes">The prefixes.</param>
    /// <param name="storageField">The storage field.</param>
    /// <param name="levelField">The level field.</param>
    /// <param name="maxSuccessorsCount">The max successors count.</param>
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
    /// Gets storage field.
    /// </summary>
    public InstructionField<ushort> StorageField { get; }
    /// <summary>
    /// Gets level field.
    /// </summary>
    public InstructionField<byte> LevelField { get; }
}