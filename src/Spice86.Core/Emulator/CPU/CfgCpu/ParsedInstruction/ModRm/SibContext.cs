namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.ModRm;

/// <summary>
/// Represents the SibContext class.
/// </summary>
public class SibContext {
    /// <summary>
    /// Gets or sets the Sib.
    /// </summary>
    public InstructionField<byte> Sib { get; }
    /// <summary>
    /// Gets or sets the Scale.
    /// </summary>
    public byte Scale { get; }
    /// <summary>
    /// Gets or sets the IndexRegister.
    /// </summary>
    public int IndexRegister { get; }
    /// <summary>
    /// Gets or sets the BaseRegister.
    /// </summary>
    public int BaseRegister { get; }
    /// <summary>
    /// Gets or sets the SibBase.
    /// </summary>
    public SibBase SibBase { get; }
    public InstructionField<uint>? BaseField { get; }
    /// <summary>
    /// Gets or sets the SibIndex.
    /// </summary>
    public SibIndex SibIndex { get; }

    /// <summary>
    /// FieldsInOrder method.
    /// </summary>
    public List<FieldWithValue> FieldsInOrder { get; } = new();

    public SibContext(
        InstructionField<byte> sib,
        byte scale,
        int indexRegister,
        int baseRegister,
        SibBase sibBase,
        InstructionField<uint>? baseField,
        SibIndex sibIndex) {
        Sib = sib;
        Scale = scale;
        IndexRegister = indexRegister;
        BaseRegister = baseRegister;
        SibBase = sibBase;
        BaseField = baseField;
        SibIndex = sibIndex;
        FieldsInOrder.Add(Sib);
        if (BaseField != null) {
            FieldsInOrder.Add(BaseField);
        }
    }
}