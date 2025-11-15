namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.ModRm;

using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents mod rm context.
/// </summary>
public class ModRmContext {

    /// <summary>
    /// Gets mod rm field.
    /// </summary>
    public InstructionField<byte> ModRmField { get; }
    /// <summary>
    /// Gets mode.
    /// </summary>
    public uint Mode { get; }
    /// <summary>
    /// Gets register index.
    /// </summary>
    public int RegisterIndex { get; }
    /// <summary>
    /// Gets register memory index.
    /// </summary>
    public int RegisterMemoryIndex { get; }
    /// <summary>
    /// The address size.
    /// </summary>
    public BitWidth AddressSize;
    /// <summary>
    /// Gets memory offset type.
    /// </summary>
    public MemoryOffsetType MemoryOffsetType { get; }
    /// <summary>
    /// Gets memory address type.
    /// </summary>
    public MemoryAddressType MemoryAddressType { get; }
    /// <summary>
    /// Gets sib context.
    /// </summary>
    public SibContext? SibContext { get; }
    /// <summary>
    /// Gets displacement type.
    /// </summary>
    public DisplacementType? DisplacementType { get; }
    /// <summary>
    /// Gets displacement field.
    /// </summary>
    public FieldWithValue? DisplacementField { get; }
    /// <summary>
    /// Gets mod rm offset type.
    /// </summary>
    public ModRmOffsetType? ModRmOffsetType { get; }
    /// <summary>
    /// Gets mod rm offset field.
    /// </summary>
    public InstructionField<ushort>? ModRmOffsetField { get; }
    /// <summary>
    /// Gets segment index.
    /// </summary>
    public int? SegmentIndex { get; }

    /// <summary>
    /// Gets fields in order.
    /// </summary>
    public List<FieldWithValue> FieldsInOrder { get; } = new();

    public ModRmContext(
        InstructionField<byte> modRmField,
        uint mode,
        int registerIndex,
        int registerMemoryIndex,
        BitWidth addressSize,
        MemoryOffsetType memoryOffsetType,
        MemoryAddressType memoryAddressType,
        SibContext? sibContext,
        DisplacementType? displacementType,
        FieldWithValue? displacementField,
        ModRmOffsetType? modRmOffsetType,
        InstructionField<ushort>? modRmOffsetField,
        int? segmentIndex
    ) {
        ModRmField = modRmField;
        Mode = mode;
        RegisterIndex = registerIndex;
        RegisterMemoryIndex = registerMemoryIndex;
        AddressSize = addressSize;
        MemoryOffsetType = memoryOffsetType;
        MemoryAddressType = memoryAddressType;
        SibContext = sibContext;
        DisplacementType = displacementType;
        DisplacementField = displacementField;
        ModRmOffsetType = modRmOffsetType;
        ModRmOffsetField = modRmOffsetField;
        SegmentIndex = segmentIndex;

        // Order of the bytes in modrm context:
        // First ModRM byte
        FieldsInOrder.Add(ModRmField);
        // Then SIB byte and its friends
        if (SibContext != null) {
            FieldsInOrder.AddRange(SibContext.FieldsInOrder);
        }
        // Then displacement
        if (DisplacementField != null) {
            FieldsInOrder.Add(DisplacementField);
        }
        // Then offset
        if (ModRmOffsetField != null) {
            FieldsInOrder.Add(ModRmOffsetField);
        }
    }

}