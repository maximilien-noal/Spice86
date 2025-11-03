namespace Spice86.MCP.Tools;

using ModelContextProtocol;
using ModelContextProtocol.Server;
using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.InterruptHandlers.Bios.Structures;
using Spice86.Core.Emulator.Memory;
using Spice86.Shared.Emulator.Memory;
using Spice86.Shared.Utils;
using System.ComponentModel;
using System.Text;

/// <summary>
/// MCP tools for exploring memory structures in a text-based format.
/// Provides structure definition lookup and memory interpretation capabilities.
/// </summary>
[McpServerToolType]
public sealed class StructureViewerTools {
    private readonly IMemory _memory;
    private readonly State _state;
    private readonly BiosDataArea _biosDataArea;

    /// <summary>
    /// Initializes a new instance of the <see cref="StructureViewerTools"/> class.
    /// </summary>
    public StructureViewerTools(IMemory memory, State state, BiosDataArea biosDataArea) {
        _memory = memory;
        _state = state;
        _biosDataArea = biosDataArea;
    }

    [McpServerTool]
    [Description("Get information about common DOS/BIOS memory structures and their layouts")]
    public Task<string> GetStructureDefinitions() {
        var result = new StringBuilder();
        result.AppendLine("Common Memory Structure Definitions:");
        result.AppendLine();
        
        result.AppendLine("=== BIOS Data Area (0040:0000) ===");
        result.AppendLine("0040:0000 - COM1 port address (word)");
        result.AppendLine("0040:0002 - COM2 port address (word)");
        result.AppendLine("0040:0004 - COM3 port address (word)");
        result.AppendLine("0040:0006 - COM4 port address (word)");
        result.AppendLine("0040:0008 - LPT1 port address (word)");
        result.AppendLine("0040:000A - LPT2 port address (word)");
        result.AppendLine("0040:000C - LPT3 port address (word)");
        result.AppendLine("0040:000E - EBDA segment or LPT4 (word)");
        result.AppendLine("0040:0010 - Equipment list flags (word)");
        result.AppendLine("0040:0013 - Memory size in KB (word)");
        result.AppendLine("0040:0017 - Keyboard status flags (byte)");
        result.AppendLine("0040:001A - Keyboard buffer head (word)");
        result.AppendLine("0040:001C - Keyboard buffer tail (word)");
        result.AppendLine("0040:001E - Keyboard buffer (32 bytes)");
        result.AppendLine("0040:0049 - Video mode (byte)");
        result.AppendLine("0040:004A - Screen columns (word)");
        result.AppendLine("0040:0062 - Active video page (byte)");
        result.AppendLine("0040:0063 - CRT controller base address (word)");
        result.AppendLine("0040:006C - Timer counter (dword)");
        result.AppendLine("0040:0075 - Hard disk count (byte)");
        result.AppendLine();
        
        result.AppendLine("=== DOS Program Segment Prefix (PSP) ===");
        result.AppendLine("Segment:0000 - INT 20h instruction (2 bytes)");
        result.AppendLine("Segment:0002 - Memory size in paragraphs (word)");
        result.AppendLine("Segment:0005 - CP/M-style call to DOS (5 bytes)");
        result.AppendLine("Segment:000A - Terminate address (far pointer)");
        result.AppendLine("Segment:000E - Break address (far pointer)");
        result.AppendLine("Segment:0012 - Critical error address (far pointer)");
        result.AppendLine("Segment:002C - Environment segment (word)");
        result.AppendLine("Segment:0050 - INT 21h, RETF (3 bytes)");
        result.AppendLine("Segment:005C - FCB1 (16 bytes)");
        result.AppendLine("Segment:006C - FCB2 (16 bytes)");
        result.AppendLine("Segment:0080 - Command tail length (byte)");
        result.AppendLine("Segment:0081 - Command tail (127 bytes)");
        result.AppendLine();
        
        result.AppendLine("=== DOS Memory Control Block (MCB) ===");
        result.AppendLine("Segment:0000 - Signature 'M' or 'Z' (byte)");
        result.AppendLine("Segment:0001 - Owner PSP segment (word)");
        result.AppendLine("Segment:0003 - Size in paragraphs (word)");
        result.AppendLine("Segment:0008 - Program name (8 bytes, DOS 4+)");
        result.AppendLine();
        
        result.AppendLine("=== Interrupt Vector Table (0000:0000) ===");
        result.AppendLine("Each entry is 4 bytes: offset (word) + segment (word)");
        result.AppendLine("0000:0000 - INT 00h (Divide by Zero)");
        result.AppendLine("0000:0004 - INT 01h (Single Step)");
        result.AppendLine("0000:0008 - INT 02h (NMI)");
        result.AppendLine("0000:0020 - INT 08h (Timer)");
        result.AppendLine("0000:0024 - INT 09h (Keyboard)");
        result.AppendLine("0000:0048 - INT 12h (Memory Size)");
        result.AppendLine("0000:0050 - INT 14h (Serial)");
        result.AppendLine("0000:0054 - INT 15h (System Services)");
        result.AppendLine("0000:0058 - INT 16h (Keyboard Services)");
        result.AppendLine("0000:005C - INT 17h (Printer)");
        result.AppendLine("0000:0084 - INT 21h (DOS Services)");
        result.AppendLine("0000:0098 - INT 26h (Absolute Disk Write)");
        result.AppendLine("0000:00CC - INT 33h (Mouse)");
        result.AppendLine();
        
        result.AppendLine("Note: Use ReadStructure tool to read actual values from memory.");
        
        return Task.FromResult(result.ToString());
    }

    [McpServerTool]
    [Description("Read and interpret a memory structure at a specific address")]
    public Task<string> ReadStructure(
        [Description("Structure type: bda, psp, mcb, ivt_entry")] string structureType,
        [Description("Segment value (hex, e.g., 0x0040 for BDA)")] string segment,
        [Description("Offset value (hex, default: 0x0000)")] string offset = "0x0000") {
        
        if (!TryParseHex(segment, out ushort seg)) {
            return Task.FromResult($"Error: Invalid segment '{segment}'");
        }
        
        if (!TryParseHex(offset, out ushort off)) {
            return Task.FromResult($"Error: Invalid offset '{offset}'");
        }

        var address = new SegmentedAddress(seg, off);
        var result = new StringBuilder();
        
        switch (structureType.ToLowerInvariant()) {
            case "bda":
                return ReadBiosDataArea(address);
            case "psp":
                return ReadProgramSegmentPrefix(address);
            case "mcb":
                return ReadMemoryControlBlock(address);
            case "ivt_entry":
                return ReadInterruptVectorEntry(address);
            default:
                return Task.FromResult($"Error: Unknown structure type '{structureType}'. Use: bda, psp, mcb, ivt_entry");
        }
    }

    private Task<string> ReadBiosDataArea(SegmentedAddress address) {
        var result = new StringBuilder();
        result.AppendLine($"BIOS Data Area at {address}:");
        result.AppendLine();
        
        result.AppendLine("Serial Ports:");
        for (int i = 0; i < 4; i++) {
            ushort portAddr = _biosDataArea.PortCom[i];
            result.AppendLine($"  COM{i + 1}: 0x{portAddr:X4}");
        }
        result.AppendLine();
        
        result.AppendLine("Parallel Ports:");
        for (int i = 0; i < 3; i++) {
            ushort portAddr = _biosDataArea.PortLpt[i];
            result.AppendLine($"  LPT{i + 1}: 0x{portAddr:X4}");
        }
        result.AppendLine();
        
        result.AppendLine($"Equipment Flags: 0x{_biosDataArea.EquipmentListFlags:X4}");
        result.AppendLine($"Memory Size: {_biosDataArea.ConventionalMemorySizeKb} KB");
        result.AppendLine($"Keyboard Flags: 0x{_biosDataArea.KeyboardStatusFlag:X2}");
        result.AppendLine($"Video Mode: 0x{_biosDataArea.VideoMode:X2}");
        result.AppendLine($"Screen Columns: {_biosDataArea.ScreenColumns}");
        result.AppendLine($"Timer Counter: {_biosDataArea.TimerCounter}");
        
        return Task.FromResult(result.ToString());
    }

    private Task<string> ReadProgramSegmentPrefix(SegmentedAddress address) {
        var result = new StringBuilder();
        result.AppendLine($"Program Segment Prefix at {address}:");
        result.AppendLine();
        
        uint baseAddr = MemoryUtils.ToPhysicalAddress(address.Segment, address.Offset);
        
        byte int20h = _memory.UInt8[baseAddr];
        result.AppendLine($"INT 20h Instruction: 0x{int20h:X2}");
        
        ushort memorySize = _memory.UInt16[baseAddr + 0x02];
        result.AppendLine($"Memory Size (paragraphs): 0x{memorySize:X4} ({memorySize} paragraphs = {memorySize * 16} bytes)");
        
        ushort terminateSeg = _memory.UInt16[baseAddr + 0x0A];
        ushort terminateOff = _memory.UInt16[baseAddr + 0x0C];
        result.AppendLine($"Terminate Address: {terminateSeg:X4}:{terminateOff:X4}");
        
        ushort envSeg = _memory.UInt16[baseAddr + 0x2C];
        result.AppendLine($"Environment Segment: 0x{envSeg:X4}");
        
        byte cmdLen = _memory.UInt8[baseAddr + 0x80];
        result.AppendLine($"Command Tail Length: {cmdLen}");
        
        if (cmdLen > 0) {
            var cmdBytes = new byte[Math.Min((int)cmdLen, 127)];
            for (int i = 0; i < cmdBytes.Length; i++) {
                cmdBytes[i] = _memory.UInt8[baseAddr + 0x81 + (uint)i];
            }
            string cmdTail = System.Text.Encoding.ASCII.GetString(cmdBytes);
            result.AppendLine($"Command Tail: \"{cmdTail}\"");
        }
        
        return Task.FromResult(result.ToString());
    }

    private Task<string> ReadMemoryControlBlock(SegmentedAddress address) {
        var result = new StringBuilder();
        result.AppendLine($"Memory Control Block at {address}:");
        result.AppendLine();
        
        uint baseAddr = MemoryUtils.ToPhysicalAddress(address.Segment, address.Offset);
        
        byte signature = _memory.UInt8[baseAddr];
        char sigChar = (char)signature;
        result.AppendLine($"Signature: '{sigChar}' (0x{signature:X2}) - {(signature == 'M' ? "More blocks follow" : signature == 'Z' ? "Last block" : "Invalid")}");
        
        ushort ownerPSP = _memory.UInt16[baseAddr + 0x01];
        result.AppendLine($"Owner PSP: 0x{ownerPSP:X4} {(ownerPSP == 0 ? "(Free)" : ownerPSP == 8 ? "(DOS)" : "(Program)")}");
        
        ushort sizeParagraphs = _memory.UInt16[baseAddr + 0x03];
        result.AppendLine($"Size: {sizeParagraphs} paragraphs ({sizeParagraphs * 16} bytes)");
        
        // Try to read program name (DOS 4+)
        var nameBytes = new byte[8];
        for (int i = 0; i < 8; i++) {
            nameBytes[i] = _memory.UInt8[baseAddr + 0x08 + (uint)i];
        }
        string name = System.Text.Encoding.ASCII.GetString(nameBytes).TrimEnd('\0', ' ');
        if (!string.IsNullOrWhiteSpace(name)) {
            result.AppendLine($"Program Name: \"{name}\"");
        }
        
        return Task.FromResult(result.ToString());
    }

    private Task<string> ReadInterruptVectorEntry(SegmentedAddress address) {
        var result = new StringBuilder();
        result.AppendLine($"Interrupt Vector Entry at {address}:");
        result.AppendLine();
        
        uint baseAddr = MemoryUtils.ToPhysicalAddress(address.Segment, address.Offset);
        
        ushort offset = _memory.UInt16[baseAddr];
        ushort segment = _memory.UInt16[baseAddr + 0x02];
        
        result.AppendLine($"Handler Address: {segment:X4}:{offset:X4}");
        result.AppendLine($"Linear Address: 0x{((segment << 4) + offset):X}");
        
        return Task.FromResult(result.ToString());
    }

    private static bool TryParseHex(string hexStr, out ushort value) {
        value = 0;
        if (string.IsNullOrWhiteSpace(hexStr)) {
            return false;
        }

        hexStr = hexStr.Trim();
        if (hexStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) {
            hexStr = hexStr.Substring(2);
        }

        return ushort.TryParse(hexStr, System.Globalization.NumberStyles.HexNumber, null, out value);
    }
}
