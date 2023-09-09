namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;

using Spice86.Core.Emulator.CPU.CfgCpu.ControlFlowGraph;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;

using System.Linq;

/// <summary>
/// Base of all the instructions: Prefixes (optional) and an opcode that can be either one or 2 bytes.
/// </summary>
public abstract class CfgInstruction : CfgNode {
    protected CfgInstruction(uint physicalAddress, InstructionField<byte> opcodeField) : this(physicalAddress,
        opcodeField, new List<InstructionPrefix>()) {
    }

    protected CfgInstruction(uint physicalAddress, InstructionField<byte> opcodeField,
        IList<InstructionPrefix> prefixes)
        : base(physicalAddress) {
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
        FieldsInOrder.AddRange(PrefixFields);
        FieldsInOrder.Add(OpcodeField);
    }

    public void PostInit() {
        Length = (byte)FieldsInOrder.Sum(field => field.Length);
    }

    /// <summary>
    /// Cache of Successors property per address. Maintenance is complex with self modifying code and is done by the InstructionLinker
    /// </summary>
    public IDictionary<uint, ICfgNode> SuccessorsPerAddress { get; private set; } = new Dictionary<uint, ICfgNode>();

    public override void UpdateSuccessorCache() {
        SuccessorsPerAddress = Successors.ToDictionary(node => node.PhysicalAddress);
    }

    public override bool IsAssembly { get => true; }

    public List<IFieldWithValue> FieldsInOrder { get; } = new();

    public SegmentOverrideInstructionPrefix? SegmentOverrideInstructionPrefix { get; }
    public OperandSize32Prefix? OperandSize32Prefix { get; }
    public AddressSize32Prefix? AddressSize32Prefix { get; }
    public RepPrefix? RepPrefix { get; }

    public byte Length { get; private set; }

    public IList<InstructionPrefix> InstructionPrefixes { get; }

    /// <summary>
    /// List of prefixes for this instruction
    /// </summary>
    public IList<InstructionField<byte>> PrefixFields { get; }

    /// <summary>
    /// Opcode
    /// </summary>
    public InstructionField<byte> OpcodeField { get; }

    /// <summary>
    /// What allows to uniquely identify the instruction among other at the same address.
    /// Usually all the fields except in some cases when they are modified (example imm value or disp), in this case instead of bytes there will be nulls
    /// </summary>
    public InstructionDiscriminator Discriminator {
        get {
            IList<byte?> discriminatorBytes =
                FieldsInOrder.Select(field => field.DiscriminatorValue).SelectMany(i => i).ToList();
            return new InstructionDiscriminator(discriminatorBytes);
        }
    }
    // Equals and HashCode to use the discriminator and super methods
}