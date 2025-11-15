namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;

using Spice86.Core.Emulator.CPU.CfgCpu.ControlFlowGraph;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.Interfaces;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Shared.Emulator.Memory;

using System.Collections.Immutable;
using System.Linq;

/// <summary>
/// Base of all the instructions: Prefixes (optional) and an opcode that can be either one or 2 bytes.
/// </summary>
public abstract class CfgInstruction : CfgNode, ICfgInstruction {
    /// <summary>
    /// Instructions are born live.
    /// </summary>
    private bool _isLive = true;

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="opcodeField">The opcode field.</param>
    /// <param name="maxSuccessorsCount">The max successors count.</param>
    protected CfgInstruction(SegmentedAddress address, InstructionField<ushort> opcodeField, int? maxSuccessorsCount) : this(address,
        opcodeField, new List<InstructionPrefix>(), maxSuccessorsCount) {
    }

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="opcodeField">The opcode field.</param>
    /// <param name="prefixes">The prefixes.</param>
    /// <param name="maxSuccessorsCount">The max successors count.</param>
    protected CfgInstruction(SegmentedAddress address, InstructionField<ushort> opcodeField,
        List<InstructionPrefix> prefixes, int? maxSuccessorsCount)
        : base(address, maxSuccessorsCount) {
        InstructionPrefixes = prefixes;
        PrefixFields = prefixes.Select(prefix => prefix.PrefixField).ToList();
        foreach (InstructionPrefix prefix in prefixes) {
            if (prefix is SegmentOverrideInstructionPrefix instructionPrefix) {
                SegmentOverrideInstructionPrefix = instructionPrefix;
            } else if (prefix is OperandSize32Prefix size32Prefix) {
                OperandSize32Prefix = size32Prefix;
            } else if (prefix is AddressSize32Prefix addressSize32Prefix) {
                AddressSize32Prefix = addressSize32Prefix;
            } else if (prefix is RepPrefix repPrefix) {
                RepPrefix = repPrefix;
            }
        }

        OpcodeField = opcodeField;
        AddFields(PrefixFields);
        AddField(OpcodeField);
    }

    /// <summary>
    /// To call after constructor to calculate instruction length
    /// </summary>
    private void UpdateLength() {
        Length = (byte)FieldsInOrder.Sum(field => field.Length);
    }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    public Dictionary<SegmentedAddress, ICfgNode> SuccessorsPerAddress { get; private set; } = new();

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    public Dictionary<InstructionSuccessorType, ISet<ICfgNode>> SuccessorsPerType { get; } = new();

    /// <summary>
    /// Updates successor cache.
    /// </summary>
    public override void UpdateSuccessorCache() {
        SuccessorsPerAddress = Successors.ToDictionary(node => node.Address);
    }

    /// <summary>
    /// The is live.
    /// </summary>
    public override bool IsLive => _isLive;

    /// <summary>
    /// Gets fields in order.
    /// </summary>
    public List<FieldWithValue> FieldsInOrder { get; } = new();

    /// <summary>
    /// Adds field.
    /// </summary>
    /// <param name="fieldWithValue">The field with value.</param>
    protected void AddField(FieldWithValue fieldWithValue) {
        FieldsInOrder.Add(fieldWithValue);
        UpdateLength();
    }

    /// <summary>
    /// Adds fields.
    /// </summary>
    /// <param name="fieldWithValues">The field with values.</param>
    protected void AddFields(IEnumerable<FieldWithValue> fieldWithValues) {
        fieldWithValues.ToList().ForEach(AddField);
    }

    /// <summary>
    /// Gets segment override instruction prefix.
    /// </summary>
    public SegmentOverrideInstructionPrefix? SegmentOverrideInstructionPrefix { get; }
    /// <summary>
    /// Gets operand size 32 prefix.
    /// </summary>
    public OperandSize32Prefix? OperandSize32Prefix { get; }
    /// <summary>
    /// Gets address size 32 prefix.
    /// </summary>
    public AddressSize32Prefix? AddressSize32Prefix { get; }
    /// <summary>
    /// Gets rep prefix.
    /// </summary>
    public RepPrefix? RepPrefix { get; }

    public byte Length { get; private set; }

    /// <summary>
    /// The next in memory address.
    /// </summary>
    public SegmentedAddress NextInMemoryAddress => new(Address.Segment, (ushort)(Address.Offset + Length));

    /// <summary>
    /// Gets instruction prefixes.
    /// </summary>
    public List<InstructionPrefix> InstructionPrefixes { get; }

    /// <summary>
    /// List of prefixes for this instruction
    /// </summary>
    public List<InstructionField<byte>> PrefixFields { get; }

    /// <summary>
    /// Opcode
    /// </summary>
    public InstructionField<ushort> OpcodeField { get; }

    /// <summary>
    /// What allows to uniquely identify the instruction among other at the same address.
    /// Usually all the fields except in some cases when they are modified (example imm value or disp), in this case instead of bytes there will be nulls
    /// </summary>
    public Signature Signature {
        get {
            ImmutableList<byte?> signatureBytes = ComputeSignatureBytes(FieldsInOrder);
            return new Signature(signatureBytes);
        }
    }

    /// <summary>
    /// Same as Signature but only aggregates final fields, ignoring those that can change.
    /// </summary>
    public Signature SignatureFinal {
        get {
            ImmutableList<byte?> signatureBytes = ComputeSignatureBytes(FieldsInOrder
                .Where(f => f.Final));
            return new Signature(signatureBytes);
        }
    }

    private ImmutableList<byte?> ComputeSignatureBytes(IEnumerable<FieldWithValue> bytes) {
        return bytes
            .Select(f => f.SignatureValue)
            .SelectMany(i => i)
            .ToImmutableList();
    }

    /// <summary>
    /// Sets live.
    /// </summary>
    /// <param name="isLive">The is live.</param>
    public void SetLive(bool isLive) {
        _isLive = isLive;
    }

    public void IncreaseMaxSuccessorsCount(SegmentedAddress target) {
        if (MaxSuccessorsCount is not null && !SuccessorsPerAddress.ContainsKey(target)) {
            // Ensure the subsequent link attempt will be done
            CanHaveMoreSuccessors = true;
            MaxSuccessorsCount++;
            // Reset it. Will not be used anymore if MaxSuccessorsCount is now more than 1 or null.
            UniqueSuccessor = null;
        }
    }
}