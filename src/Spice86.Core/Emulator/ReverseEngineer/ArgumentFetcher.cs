namespace Spice86.Core.Emulator.ReverseEngineer;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Shared.Utils;

/// <summary>
/// <para>Helper to get function arguments from the stack.</para>
/// <para>Argument naming is based on cdecl calling convention.</para>
/// </summary>
public class ArgumentFetcher {
    private readonly Stack _stack;
    private readonly IMemory _memory;
    private readonly State _state;

    /// <summary>
    /// Instantiates a new instance.
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="memory"></param>
    public ArgumentFetcher(Cpu cpu, IMemory memory) {
        _stack = cpu.Stack;
        _memory = memory;
        _state = cpu.State;
    }

    /// <summary>
    /// Gets .
    /// </summary>
    /// <param name="arg1">The arg 1.</param>
    /// <param name="arg2">The arg 2.</param>
    /// <param name="arg3">The arg 3.</param>
    public void Get(out ushort arg1, out uint arg2, out ushort arg3) {
        arg1 = _stack.Peek16(4);
        arg2 = _stack.Peek32(6);
        arg3 = _stack.Peek16(10);
    }

    /// <summary>
    /// Gets .
    /// </summary>
    /// <param name="arg1">The arg 1.</param>
    /// <param name="arg2">The arg 2.</param>
    /// <param name="arg3">The arg 3.</param>
    public void Get(out string arg1, out short arg2, out ushort arg3) {
        ushort stringPointerOffset = _stack.Peek16(4);
        arg2 = (short)_stack.Peek16(6);
        arg3 = _stack.Peek16(8);
        arg1 = GetStringFromDsPointer(stringPointerOffset);
    }

    /// <summary>
    /// Gets .
    /// </summary>
    /// <param name="arg1">The arg 1.</param>
    /// <param name="arg2">The arg 2.</param>
    /// <param name="arg3">The arg 3.</param>
    public void Get(out string arg1, out ushort arg2, out short arg3) {
        ushort stringPointerOffset = _stack.Peek16(4);
        arg2 = _stack.Peek16(6);
        arg3 = (short)_stack.Peek16(8);
        arg1 = GetStringFromDsPointer(stringPointerOffset);
    }

    /// <summary>
    /// Gets .
    /// </summary>
    /// <param name="arg1">The arg 1.</param>
    /// <param name="arg2">The arg 2.</param>
    /// <param name="arg3">The arg 3.</param>
    public void Get(out ushort arg1, out int arg2, out ushort arg3) {
        arg1 = _stack.Peek16(4);
        arg2 = (int)_stack.Peek32(6);
        arg3 = _stack.Peek16(10);
    }

    /// <summary>
    /// Gets .
    /// </summary>
    /// <param name="arg1">The arg 1.</param>
    /// <param name="arg2">The arg 2.</param>
    public void Get(out ushort arg1, out ushort arg2) {
        arg1 = _stack.Peek16(4);
        arg2 = _stack.Peek16(6);
    }

    /// <summary>
    /// Gets .
    /// </summary>
    /// <param name="arg1">The arg 1.</param>
    public void Get(out ushort arg1) {
        arg1 = _stack.Peek16(4);
    }

    /// <summary>
    /// Gets .
    /// </summary>
    /// <param name="arg1">The arg 1.</param>
    public void Get(out string arg1) {
        ushort arg1PointerOffset = _stack.Peek16(4);
        arg1 = GetStringFromDsPointer(arg1PointerOffset);
    }

    /// <summary>
    /// Gets .
    /// </summary>
    /// <param name="arg1">The arg 1.</param>
    /// <param name="arg2">The arg 2.</param>
    public void Get(out string arg1, out string arg2) {
        ushort arg1PointerOffset = _stack.Peek16(4);
        ushort arg2PointerOffset = _stack.Peek16(6);
        arg1 = GetStringFromDsPointer(arg1PointerOffset);
        arg2 = GetStringFromDsPointer(arg2PointerOffset);
    }

    /// <summary>
    /// Gets .
    /// </summary>
    /// <param name="arg1">The arg 1.</param>
    /// <param name="arg2">The arg 2.</param>
    /// <param name="arg3">The arg 3.</param>
    /// <param name="arg4">The arg 4.</param>
    public void Get(out ushort arg1, out ushort arg2, out ushort arg3, out ushort arg4) {
        arg1 = _stack.Peek16(4);
        arg2 = _stack.Peek16(6);
        arg3 = _stack.Peek16(8);
        arg4 = _stack.Peek16(10);
    }

    /// <summary>
    /// Gets .
    /// </summary>
    /// <param name="arg1">The arg 1.</param>
    /// <param name="arg2">The arg 2.</param>
    public void Get(out ushort arg1, out string arg2) {
        arg1 = _stack.Peek16(4);
        ushort arg2PointerOffset = _stack.Peek16(6);
        arg2 = GetStringFromDsPointer(arg2PointerOffset);
    }

    /// <summary>
    /// Gets .
    /// </summary>
    /// <param name="arg1">The arg 1.</param>
    /// <param name="arg2">The arg 2.</param>
    /// <param name="arg3">The arg 3.</param>
    public void Get(out ushort arg1, out ushort arg2, out ushort arg3) {
        arg1 = _stack.Peek16(4);
        arg2 = _stack.Peek16(6);
        arg3 = _stack.Peek16(8);
    }

    /// <summary>
    /// Gets .
    /// </summary>
    /// <param name="arg1">The arg 1.</param>
    /// <param name="arg2">The arg 2.</param>
    /// <param name="arg3">The arg 3.</param>
    public void Get(out uint arg1, out uint arg2, out ushort arg3) {
        arg1 = _stack.Peek32(4);
        arg2 = _stack.Peek32(8);
        arg3 = _stack.Peek16(12);
    }

    /// <summary>
    /// Gets .
    /// </summary>
    /// <param name="arg1">The arg 1.</param>
    /// <param name="arg2">The arg 2.</param>
    public void Get(out uint arg1, out uint arg2) {
        arg1 = _stack.Peek32(4);
        arg2 = _stack.Peek32(8);
    }

    private string GetStringFromDsPointer(ushort offset) {
        uint address = MemoryUtils.ToPhysicalAddress(_state.DS, offset);
        return _memory.GetZeroTerminatedString(address, int.MaxValue);
    }
}