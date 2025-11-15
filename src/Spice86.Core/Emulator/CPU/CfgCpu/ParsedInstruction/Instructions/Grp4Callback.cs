namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Builder;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Instruction;
using Spice86.Core.Emulator.CPU.CfgCpu.InstructionExecutor;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.ModRm;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents grp 4 callback.
/// </summary>
public class Grp4Callback : InstructionWithModRm {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="opcodeField">The opcode field.</param>
    /// <param name="prefixes">The prefixes.</param>
    /// <param name="modRmContext">The mod rm context.</param>
    /// <param name="callbackNumber">The callback number.</param>
    public Grp4Callback(SegmentedAddress address, InstructionField<ushort> opcodeField, List<InstructionPrefix> prefixes,
        ModRmContext modRmContext, InstructionField<ushort> callbackNumber) : base(address, opcodeField, prefixes,
        modRmContext, null) {
        CallbackNumber = callbackNumber;
        AddField(callbackNumber);
    }

    /// <summary>
    /// Gets callback number.
    /// </summary>
    public InstructionField<ushort> CallbackNumber { get; }

    /// <summary>
    /// Executes .
    /// </summary>
    /// <param name="helper">The helper.</param>
    public override void Execute(InstructionExecutionHelper helper) {
        helper.ModRm.RefreshWithNewModRmContext(ModRmContext);
        helper.CallbackHandler.Run(helper.InstructionFieldValueRetriever.GetFieldValue(CallbackNumber));
        helper.MoveIpAndSetNextNode(this);
    }

    /// <summary>
    /// Converts to instruction ast.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The result of the operation.</returns>
    public override InstructionNode ToInstructionAst(AstBuilder builder) {
        return new InstructionNode(InstructionOperation.CALLBACK);
    }
}