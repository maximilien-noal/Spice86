namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.ModRm;

/// <summary>
/// Represents sib context.
/// </summary>
public class SibContext {
    /// <summary>
    /// Gets sib.
    /// </summary>
    public InstructionField<byte> Sib { get; }
    /// <summary>
    /// Gets scale.
    /// </summary>
    public byte Scale { get; }
    /// <summary>
    /// Gets index register.
    /// </summary>
    public int IndexRegister { get; }
    /// <summary>
    /// Gets base register.
    /// </summary>
    public int BaseRegister { get; }
    /// <summary>
    /// Gets sib base.
    /// </summary>
    public SibBase SibBase { get; }
    /// <summary>
    /// Gets base field.
    /// </summary>
    public InstructionField<uint>? BaseField { get; }
    /// <summary>
    /// Gets sib index.
    /// </summary>
    public SibIndex SibIndex { get; }

    /// <summary>
    /// Gets fields in order.
    /// </summary>
    public List<FieldWithValue> FieldsInOrder { get; } = new();

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="sib">The sib.</param>
    /// <param name="scale">The scale.</param>
    /// <param name="indexRegister">The index register.</param>
    /// <param name="baseRegister">The base register.</param>
    /// <param name="sibBase">The sib base.</param>
    /// <param name="baseField">The base field.</param>
    /// <param name="sibIndex">The sib index.</param>
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