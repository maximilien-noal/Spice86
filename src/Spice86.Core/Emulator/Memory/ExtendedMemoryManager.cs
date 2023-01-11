﻿namespace Spice86.Core.Emulator.Memory;

using Spice86.Core;
using Spice86.Core.Emulator.Devices;
using Spice86.Core.Emulator.InterruptHandlers;
using Spice86.Core.Emulator.VM;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Provides DOS applications with XMS memory.
/// </summary>
public class ExtendedMemoryManager : InterruptHandler, IDeviceCallbackProvider {
    private SegmentedAddress callbackAddress;
    private int a20EnableCount;
    private readonly LinkedList<XmsBlock> xms = new();
    private readonly SortedList<int, int> handles = new();

    public ExtendedMemoryManager(Machine machine) : base(machine) {
        //TODO: Initialize this in Machine on startup, along with other
        // IDeviceCallbackProvider devices.
        callbackAddress = new(0, 0);
        _machine = machine;
        this.InitializeMemoryMap();
    }

    /// <summary>
    /// Specifies the starting physical address of XMS.
    /// </summary>
    public const uint XmsBaseAddress = Memory.ConvMemorySize + 65536 + 0x4000 + 1024 * 1024;
    /// <summary>
    /// Total number of handles available at once.
    /// </summary>
    private const int MaxHandles = 128;

    /// <summary>
    /// Gets the largest free block of memory in bytes.
    /// </summary>
    public uint LargestFreeBlock => this.GetFreeBlocks().FirstOrDefault().Length;
    /// <summary>
    /// Gets the total amount of free memory in bytes.
    /// </summary>
    public long TotalFreeMemory => this.GetFreeBlocks().Sum(b => (long)b.Length);
    /// <summary>
    /// Gets the total amount of extended memory.
    /// </summary>
    public int ExtendedMemorySize => this._machine.Memory.MemorySize - (int)XmsBaseAddress;

    IEnumerable<int> InputPorts => new int[] { 0x92 };
    IEnumerable<int> OutputPorts => new int[] { 0x92 };
    public override byte Index => 0x43;
    public bool IsHookable => true;
    public SegmentedAddress CallbackAddress {
        set => this.callbackAddress = value;
    }

    public override void Run() {
        switch (_machine.Cpu.State.AL) {
            case XmsHandlerFunctions.InstallationCheck:
                _machine.Cpu.State.AL = 0x80;
                break;

            case XmsHandlerFunctions.GetCallbackAddress:
                _machine.Cpu.State.BX = (ushort)this.callbackAddress.Offset;
                _machine.Cpu.State.ES = this.callbackAddress.Segment;
                break;

            default:
                throw new NotImplementedException($"XMS interrupt handler function {_machine.Cpu.State.AL:X2}h not implemented.");
        }
    }
    public void InvokeCallback() {
        switch (_machine.Cpu.State.AH) {
            case XmsFunctions.GetVersionNumber:
                _machine.Cpu.State.AX = 0x0300; // Return version 3.00
                _machine.Cpu.State.BX = 0; // Internal version
                _machine.Cpu.State.DX = 1; // HMA exists
                break;

            case XmsFunctions.RequestHighMemoryArea:
                _machine.Cpu.State.AX = 0; // Didn't work
                _machine.Cpu.State.BL = 0x91; // HMA already in use
                break;

            case XmsFunctions.ReleaseHighMemoryArea:
                _machine.Cpu.State.AX = 0; // Didn't work
                _machine.Cpu.State.BL = 0x93; // HMA not allocated
                break;

            case XmsFunctions.GlobalEnableA20:
                _machine.Memory.EnableA20 = true;
                _machine.Cpu.State.AX = 1; // Success
                break;

            case XmsFunctions.GlobalDisableA20:
                _machine.Memory.EnableA20 = false;
                _machine.Cpu.State.AX = 1; // Success
                break;

            case XmsFunctions.LocalEnableA20:
                EnableLocalA20();
                _machine.Cpu.State.AX = 1; // Success
                break;

            case XmsFunctions.LocalDisableA20:
                DisableLocalA20();
                _machine.Cpu.State.AX = 1; // Success
                break;

            case XmsFunctions.QueryA20:
                _machine.Cpu.State.AX = (ushort)((this.a20EnableCount > 0) ? (short)1 : (short)0);
                break;

            case XmsFunctions.QueryFreeExtendedMemory:
                if (this.LargestFreeBlock <= ushort.MaxValue * 1024u) {
                    _machine.Cpu.State.AX = (ushort)(this.LargestFreeBlock / 1024u);
                } else {
                    _machine.Cpu.State.AX = unchecked((ushort)ushort.MaxValue);
                }

                if (this.TotalFreeMemory <= ushort.MaxValue * 1024u) {
                    _machine.Cpu.State.DX = (ushort)(this.TotalFreeMemory / 1024);
                } else {
                    _machine.Cpu.State.DX = unchecked((ushort)ushort.MaxValue);
                }

                if (_machine.Cpu.State.AX == 0 && _machine.Cpu.State.DX == 0) {
                    _machine.Cpu.State.BL = 0xA0;
                }

                break;

            case XmsFunctions.AllocateExtendedMemoryBlock:
                AllocateBlock((ushort)_machine.Cpu.State.DX);
                break;

            case XmsFunctions.FreeExtendedMemoryBlock:
                FreeBlock();
                break;

            case XmsFunctions.LockExtendedMemoryBlock:
                LockBlock();
                break;

            case XmsFunctions.UnlockExtendedMemoryBlock:
                UnlockBlock();
                break;

            case XmsFunctions.GetHandleInformation:
                GetHandleInformation();
                break;

            case XmsFunctions.MoveExtendedMemoryBlock:
                MoveMemoryBlock();
                break;

            case XmsFunctions.RequestUpperMemoryBlock:
                _machine.Cpu.State.BL = 0xB1; // No UMB's available.
                _machine.Cpu.State.AX = 0; // Didn't work.
                break;

            case XmsFunctions.QueryAnyFreeExtendedMemory:
                QueryAnyFreeExtendedMemory();
                break;

            case XmsFunctions.AllocateAnyExtendedMemory:
                AllocateBlock((uint)_machine.Cpu.State.EDX);
                break;

            default:
                throw new NotImplementedException($"XMS function {_machine.Cpu.State.AH:X2}h not implemented.");
        }
    }
    byte ReadByte(int port) => this._machine.Memory.EnableA20 ? (byte)0x02 : (byte)0x00;
    ushort ReadWord(int port) => throw new NotSupportedException();
    void WriteByte(int port, byte value) => this._machine.Memory.EnableA20 = (value & 0x02) != 0;
    void WriteWord(int port, ushort value) => throw new NotSupportedException();

    /// <summary>
    /// Attempts to allocate a block of extended memory.
    /// </summary>
    /// <param name="length">Number of bytes to allocate.</param>
    /// <param name="handle">If successful, contains the allocation handle.</param>
    /// <returns>Zero on success. Nonzero indicates error code.</returns>
    public byte TryAllocate(uint length, out short handle) {
        handle = (short)this.GetNextHandle();
        if (handle == 0) {
            return 0xA1; // All handles are used.
        }

        // Round up to next kbyte if necessary.
        if ((length % 1024) != 0) {
            length = (length & 0xFFFFFC00u) + 1024u;
        } else {
            length &= 0xFFFFFC00u;
        }

        // Zero-length allocations are allowed.
        if (length == 0) {
            this.handles.Add(handle, 0);
            return 0;
        }

        XmsBlock? smallestFreeBlock = this.GetFreeBlocks()
            .Where(b => b.Length >= length)
            .Select(static b => new XmsBlock?(b))
            .FirstOrDefault();

        if (smallestFreeBlock == null) {
            return 0xA0; // Not enough free memory.
        }

        LinkedListNode<XmsBlock>? freeNode = this.xms.Find(smallestFreeBlock.Value);
        if (freeNode is not null) {
            XmsBlock[] newNodes = freeNode.Value.Allocate(handle, length);
            this.xms.Replace((XmsBlock)smallestFreeBlock, newNodes);
        }

        this.handles.Add(handle, 0);
        return 0;
    }
    /// <summary>
    /// Returns the block with the specified handle if found; otherwise returns null.
    /// </summary>
    /// <param name="handle">Handle of block to search for.</param>
    /// <param name="block">On success, contains information about the block.</param>
    /// <returns>True if handle was found; otherwise false.</returns>
    public bool TryGetBlock(int handle, out XmsBlock block) {
        foreach (XmsBlock b in this.xms) {
            if (b.IsUsed && b.Handle == handle) {
                block = b;
                return true;
            }
        }

        block = default;
        return false;
    }

    /// <summary>
    /// Increments the A20 enable count.
    /// </summary>
    private void EnableLocalA20() {
        if (this.a20EnableCount == 0) {
            this._machine.Memory.EnableA20 = true;
        }

        this.a20EnableCount++;
    }
    /// <summary>
    /// Decrements the A20 enable count.
    /// </summary>
    private void DisableLocalA20() {
        if (this.a20EnableCount == 1) {
            this._machine.Memory.EnableA20 = false;
        }

        if (this.a20EnableCount > 0) {
            this.a20EnableCount--;
        }
    }
    /// <summary>
    /// Initializes the internal memory map.
    /// </summary>
    /// <exception cref="InvalidOperationException">If xms is already initialized</exception>
    private void InitializeMemoryMap() {
        if (this.xms.Count != 0) {
            throw new InvalidOperationException();
        }

        uint memoryAvailable = (uint)_machine.Memory.MemorySize - XmsBaseAddress;
        this.xms.AddFirst(new XmsBlock(0, 0, memoryAvailable, false));
    }
    /// <summary>
    /// Returns all of the free blocks in the map sorted by size in ascending order.
    /// </summary>
    /// <returns>Sorted list of free blocks in the map.</returns>
    private IEnumerable<XmsBlock> GetFreeBlocks() => xms.Where(static x => !x.IsUsed).OrderBy(static x => x.Length);
    /// <summary>
    /// Returns the next available handle for an allocation on success; returns 0 if no handles are available.
    /// </summary>
    /// <returns>New handle if available; otherwise returns null.</returns>
    private int GetNextHandle() {
        for (int i = 1; i <= MaxHandles; i++) {
            if (!this.handles.ContainsKey(i)) {
                return i;
            }
        }

        return 0;
    }
    /// <summary>
    /// Attempts to merge a free block with the following block if possible.
    /// </summary>
    /// <param name="firstBlock">Free block to merge.</param>
    private void MergeFreeBlocks(XmsBlock firstBlock) {
        LinkedListNode<XmsBlock>? firstNode = this.xms.Find(firstBlock);

        if (firstNode?.Next != null) {
            LinkedListNode<XmsBlock> nextNode = firstNode.Next;
            if (!nextNode.Value.IsUsed) {
                XmsBlock newBlock = firstBlock.Join(nextNode.Value);
                this.xms.Remove(nextNode);
                this.xms.Replace(firstBlock, newBlock);
            }
        }
    }
    /// <summary>
    /// Allocates a new block of memory.
    /// </summary>
    /// <param name="kbytes">Number of kilobytes requested.</param>
    private void AllocateBlock(uint kbytes) {
        byte res = this.TryAllocate(kbytes * 1024u, out short handle);
        if (res == 0) {
            _machine.Cpu.State.AX = 1; // Success.
            _machine.Cpu.State.DX = (ushort)handle;
        } else {
            _machine.Cpu.State.AX = 0; // Didn't work.
            _machine.Cpu.State.BL = res;
        }
    }
    /// <summary>
    /// Frees a block of memory.
    /// </summary>
    private void FreeBlock() {
        int handle = (ushort)_machine.Cpu.State.DX;

        if (!this.handles.TryGetValue(handle, out int lockCount)) {
            _machine.Cpu.State.AX = 0; // Didn't work.
            _machine.Cpu.State.BL = 0xA2; // Invalid handle.
            return;
        }

        if (lockCount > 0) {
            _machine.Cpu.State.AX = 0; // Didn't work.
            _machine.Cpu.State.BL = 0xAB; // Handle is locked.
            return;
        }

        if (this.TryGetBlock(handle, out XmsBlock block)) {
            XmsBlock freeBlock = block.Free();
            this.xms.Replace(block, freeBlock);
            this.MergeFreeBlocks(freeBlock);
        }

        this.handles.Remove(handle);
        _machine.Cpu.State.AX = 1; // Success.
    }
    /// <summary>
    /// Locks a block of memory.
    /// </summary>
    private void LockBlock() {
        int handle = (ushort)_machine.Cpu.State.DX;

        if (!this.handles.TryGetValue(handle, out int lockCount)) {
            _machine.Cpu.State.AX = 0; // Didn't work.
            _machine.Cpu.State.BL = 0xA2; // Invalid handle.
            return;
        }

        this.handles[handle] = lockCount + 1;

        _ = this.TryGetBlock(handle, out XmsBlock block);
        uint fullAddress = XmsBaseAddress + block.Offset;

        _machine.Cpu.State.AX = 1; // Success.
        _machine.Cpu.State.DX = (ushort)(fullAddress >> 16);
        _machine.Cpu.State.BX = (ushort)(fullAddress & 0xFFFFu);
    }
    /// <summary>
    /// Unlocks a block of memory.
    /// </summary>
    private void UnlockBlock() {
        int handle = (ushort)_machine.Cpu.State.DX;

        if (!this.handles.TryGetValue(handle, out int lockCount)) {
            _machine.Cpu.State.AX = 0; // Didn't work.
            _machine.Cpu.State.BL = 0xA2; // Invalid handle.
            return;
        }

        if (lockCount < 1) {
            _machine.Cpu.State.AX = 0;
            _machine.Cpu.State.BL = 0xAA; // Handle is not locked.
            return;
        }

        this.handles[handle] = lockCount - 1;

        _machine.Cpu.State.AX = 1; // Success.
    }
    /// <summary>
    /// Returns information about an XMS handle.
    /// </summary>
    private void GetHandleInformation() {
        int handle = (ushort)_machine.Cpu.State.DX;

        if (!this.handles.TryGetValue(handle, out int lockCount)) {
            _machine.Cpu.State.AX = 0; // Didn't work.
            _machine.Cpu.State.BL = 0xA2; // Invalid handle.
            return;
        }

        _machine.Cpu.State.BH = (byte)lockCount;
        _machine.Cpu.State.BL = (byte)(MaxHandles - this.handles.Count);

        if (!this.TryGetBlock(handle, out XmsBlock block)) {
            _machine.Cpu.State.DX = 0;
        } else {
            _machine.Cpu.State.DX = (ushort)((block).Length / 1024u);
        }

        _machine.Cpu.State.AX = 1; // Success.
    }
    /// <summary>
    /// Copies a block of memory.
    /// </summary>
    private void MoveMemoryBlock() {
        bool a20State = this._machine.Memory.EnableA20;
        this._machine.Memory.EnableA20 = true;

        XmsMoveData moveData;
        unsafe {
            moveData = *(XmsMoveData*)this._machine.Memory.GetPointer(this._machine.Cpu.State.DS, this._machine.Cpu.State.SI);
        }

        IntPtr srcPtr = IntPtr.Zero;
        IntPtr destPtr = IntPtr.Zero;

        if (moveData.SourceHandle == 0) {
            SegmentedAddress srcAddress = moveData.SourceAddress;
            srcPtr = this._machine.Memory.GetPointer(srcAddress.Segment, srcAddress.Offset);
        } else {
            if (this.TryGetBlock(moveData.SourceHandle, out XmsBlock srcBlock)) {
                srcPtr = this._machine.Memory.GetPointer((int)(XmsBaseAddress + (srcBlock).Offset + moveData.SourceOffset));
            }
        }

        if (moveData.DestHandle == 0) {
            SegmentedAddress destAddress = moveData.DestAddress;
            destPtr = this._machine.Memory.GetPointer(destAddress.Segment, destAddress.Offset);
        } else {
            if (this.TryGetBlock(moveData.DestHandle, out XmsBlock destBlock)) {
                destPtr = this._machine.Memory.GetPointer((int)(XmsBaseAddress + destBlock.Offset + moveData.DestOffset));
            }
        }

        if (srcPtr == IntPtr.Zero) {
            _machine.Cpu.State.BL = 0xA3; // Invalid source handle.
            _machine.Cpu.State.AX = 0; // Didn't work.
            return;
        }
        if (destPtr == IntPtr.Zero) {
            _machine.Cpu.State.BL = 0xA5; // Invalid destination handle.
            _machine.Cpu.State.AX = 0; // Didn't work.
            return;
        }

        unsafe {
            byte* src = (byte*)srcPtr.ToPointer();
            byte* dest = (byte*)destPtr.ToPointer();

            for (uint i = 0; i < moveData.Length; i++) {
                dest[i] = src[i];
            }
        }

        _machine.Cpu.State.AX = 1; // Success.
        this._machine.Memory.EnableA20 = a20State;
    }
    /// <summary>
    /// Queries free memory using 32-bit registers.
    /// </summary>
    private void QueryAnyFreeExtendedMemory() {
        this._machine.Cpu.State.EAX = (uint)(this.LargestFreeBlock / 1024u);
        this._machine.Cpu.State.ECX = (uint)(this._machine.Memory.MemorySize - 1);
        this._machine.Cpu.State.EDX = (uint)(this.TotalFreeMemory / 1024);

        if (this._machine.Cpu.State.EAX == 0) {
            this._machine.Cpu.State.BL = 0xA0;
        } else {
            this._machine.Cpu.State.BL = 0;
        }
    }
}