namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Builder;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Instruction;
using Spice86.Core.Emulator.CPU.CfgCpu.InstructionExecutor;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents cwd 32.
/// </summary>
public class Cwd32 : CfgInstruction {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="opcodeField">The opcode field.</param>
    /// <param name="prefixes">The prefixes.</param>
    public Cwd32(SegmentedAddress address, InstructionField<ushort> opcodeField, List<InstructionPrefix> prefixes) :
        base(address, opcodeField, prefixes, 1) {
    }

    /// <summary>
    /// Executes .
    /// </summary>
    /// <param name="helper">The helper.</param>
    public override void Execute(InstructionExecutionHelper helper) {
        // CWD, Sign extend EAX into EDX (dword to qword)
        if (helper.State.EAX >= 0x80000000) {
            helper.State.EDX = 0xFFFFFFFF;
        } else {
            helper.State.EDX = 0;
        }
        helper.MoveIpAndSetNextNode(this);
    }

    /// <summary>
    /// Converts to instruction ast.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The result of the operation.</returns>
    public override InstructionNode ToInstructionAst(AstBuilder builder) {
        return new InstructionNode(InstructionOperation.CDQ);
    }
}