namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions;

using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Builder;
using Spice86.Core.Emulator.CPU.CfgCpu.Ast.Instruction;
using Spice86.Core.Emulator.CPU.CfgCpu.InstructionExecutor;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents the Cwd16 class.
/// </summary>
public class Cwd16 : CfgInstruction {
    public Cwd16(SegmentedAddress address, InstructionField<ushort> opcodeField, List<InstructionPrefix> prefixes) :
        base(address, opcodeField, prefixes, 1) {
    }

    /// <summary>
    /// void method.
    /// </summary>
    public override void Execute(InstructionExecutionHelper helper) {
        // CWD, Sign extend AX into DX (word to dword)
        if (helper.State.AX >= 0x8000) {
            helper.State.DX = 0xFFFF;
        } else {
            helper.State.DX = 0;
        }
        helper.MoveIpAndSetNextNode(this);
    }

    /// <summary>
    /// InstructionNode method.
    /// </summary>
    public override InstructionNode ToInstructionAst(AstBuilder builder) {
        return new InstructionNode(InstructionOperation.CWD);
    }
}