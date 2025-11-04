namespace Spice86.MCP.Tools;

using ModelContextProtocol;
using ModelContextProtocol.Server;
using Spice86.Core.Emulator.InterruptHandlers.Bios.Structures;
using Spice86.Core.Emulator.OperatingSystem.Structures;
using Spice86.Core.Emulator.IOPorts;
using Spice86.Shared.Emulator.Memory;
using System.ComponentModel;
using System.Text;
using System.Reflection;

/// <summary>
/// MCP server for exploring DOS and BIOS data structures and device port documentation.
/// Provides comprehensive device documentation (DMA, PIC, Timer, etc.) and structure information for reverse engineering.
/// </summary>
[McpServerToolType]
public sealed class DeviceDocumentationProvider {
    private readonly BiosDataArea? _biosDataArea;
    private readonly IOPortDispatcher _ioPortDispatcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceDocumentationProvider"/> class.
    /// </summary>
    /// <param name="biosDataArea">The BIOS data area (may be null in headless configurations).</param>
    /// <param name="ioPortDispatcher">The IO port dispatcher for port exploration.</param>
    public DeviceDocumentationProvider(BiosDataArea? biosDataArea, IOPortDispatcher ioPortDispatcher) {
        _biosDataArea = biosDataArea;
        _ioPortDispatcher = ioPortDispatcher;
    }

    [McpServerTool]
    [Description("Get information about the BIOS Data Area structure")]
    public Task<string> GetBiosDataAreaInfo() {
        if (_biosDataArea == null) {
            return Task.FromResult("BIOS Data Area is not available in this emulator configuration.");
        }

        var result = new StringBuilder();
        result.AppendLine("BIOS Data Area Information:");
        result.AppendLine($"Base Address: {_biosDataArea.BaseAddress}");
        result.AppendLine();
        
        result.AppendLine("COM Port Addresses:");
        for (int i = 0; i < 4; i++) {
            result.AppendLine($"  COM{i + 1}: 0x{_biosDataArea.PortCom[i]:X4}");
        }
        result.AppendLine();
        
        result.AppendLine("LPT Port Addresses:");
        for (int i = 0; i < 3; i++) {
            result.AppendLine($"  LPT{i + 1}: 0x{_biosDataArea.PortLpt[i]:X4}");
        }
        result.AppendLine();
        
        result.AppendLine($"Equipment List Flags: 0x{_biosDataArea.EquipmentListFlags:X4}");
        result.AppendLine($"Memory Size (KB): {_biosDataArea.ConventionalMemorySizeKb}");
        result.AppendLine($"Keyboard Status Flag: 0x{_biosDataArea.KeyboardStatusFlag:X2}");
        result.AppendLine($"Keyboard Buffer Start: 0x{_biosDataArea.KbdBufStartOffset:X4}");
        result.AppendLine($"Keyboard Buffer End: 0x{_biosDataArea.KbdBufEndOffset:X4}");
        result.AppendLine($"Video Mode: 0x{_biosDataArea.VideoMode:X2}");
        result.AppendLine($"Video Columns: {_biosDataArea.ScreenColumns}");
        result.AppendLine($"Active Display Page: {_biosDataArea.CurrentVideoPage}");
        result.AppendLine($"Video Base IO Port: 0x{_biosDataArea.CrtControllerBaseAddress:X4}");
        result.AppendLine($"Hard Disk Count: {_biosDataArea.Hdcount}");
        result.AppendLine($"Tick Count: {_biosDataArea.TimerCounter}");
        
        return Task.FromResult(result.ToString());
    }

    [McpServerTool]
    [Description("Get documentation about DOS structure types available in the emulator")]
    public Task<string> GetDosStructureTypes() {
        var result = new StringBuilder();
        result.AppendLine("DOS Structure Types Available:");
        result.AppendLine();
        
        result.AppendLine("1. DosTables - Container for DOS system tables");
        result.AppendLine("   - Holds various DOS internal data structures");
        result.AppendLine();
        
        result.AppendLine("2. DosMemoryControlBlock (MCB) - Memory allocation block header");
        result.AppendLine("   - Located at segment:0000");
        result.AppendLine("   - Contains: signature (1 byte), owner PSP (2 bytes), size in paragraphs (2 bytes)");
        result.AppendLine();
        
        result.AppendLine("3. DosProgramSegmentPrefix (PSP) - Program header");
        result.AppendLine("   - 256 bytes starting at program segment");
        result.AppendLine("   - Contains: INT 20h instruction, memory size, command tail, FCBs, etc.");
        result.AppendLine();
        
        result.AppendLine("4. DosDiskTransferArea (DTA) - File I/O buffer");
        result.AppendLine("   - Default 128 bytes at PSP:0080h");
        result.AppendLine("   - Used for file operations");
        result.AppendLine();
        
        result.AppendLine("5. DosFile - File handle structure");
        result.AppendLine("   - Manages open files");
        result.AppendLine();
        
        result.AppendLine("6. DosSwappableDataArea (SDA) - DOS kernel work area");
        result.AppendLine("   - Contains DOS internal state");
        result.AppendLine();
        
        result.AppendLine("7. DosSysVars - DOS system variables");
        result.AppendLine("   - Global DOS configuration");
        result.AppendLine();
        
        result.AppendLine("Note: These structures are internal to the emulator's DOS implementation.");
        result.AppendLine("Access them through Memory tools using the documented offsets.");
        
        return Task.FromResult(result.ToString());
    }

    [McpServerTool]
    [Description("Get documentation about BIOS structure types available in the emulator")]
    public Task<string> GetBiosStructureTypes() {
        var result = new StringBuilder();
        result.AppendLine("BIOS Structure Types Available:");
        result.AppendLine();
        
        result.AppendLine("1. BiosDataArea (BDA) - Located at segment 0040:0000");
        result.AppendLine("   - Equipment list: 0040:0010");
        result.AppendLine("   - Memory size: 0040:0013 (KB)");
        result.AppendLine("   - Keyboard status: 0040:0017");
        result.AppendLine("   - Video mode: 0040:0049");
        result.AppendLine("   - Video columns: 0040:004A");
        result.AppendLine("   - COM ports: 0040:0000-0007");
        result.AppendLine("   - LPT ports: 0040:0008-000D");
        result.AppendLine("   - Tick count: 0040:006C (DWORD)");
        result.AppendLine();
        
        result.AppendLine("2. GlobalDescriptorTable (GDT) - Protected mode descriptor table");
        result.AppendLine("   - Used for protected mode transitions");
        result.AppendLine();
        
        result.AppendLine("Note: The BIOS Data Area is the primary structure for BIOS state.");
        result.AppendLine("Use GetBiosDataAreaInfo() tool for current values.");
        
        return Task.FromResult(result.ToString());
    }

    [McpServerTool]
    [Description("Get information about registered IO port handlers")]
    public Task<string> GetIoPortHandlers() {
        var result = new StringBuilder();
        result.AppendLine("IO Port Dispatcher Information:");
        result.AppendLine();
        
        result.AppendLine("The IO Port Dispatcher manages hardware I/O ports.");
        result.AppendLine("Common port ranges:");
        result.AppendLine();
        
        result.AppendLine("DMA Controller:");
        result.AppendLine("  0x000-0x00F - DMA channels 0-3");
        result.AppendLine("  0x0C0-0x0DF - DMA channels 4-7");
        result.AppendLine();
        
        result.AppendLine("Programmable Interrupt Controller (PIC):");
        result.AppendLine("  0x020-0x021 - Master PIC");
        result.AppendLine("  0x0A0-0x0A1 - Slave PIC");
        result.AppendLine();
        
        result.AppendLine("Timer:");
        result.AppendLine("  0x040-0x043 - Programmable Interval Timer (PIT)");
        result.AppendLine();
        
        result.AppendLine("Keyboard:");
        result.AppendLine("  0x060 - Keyboard data port");
        result.AppendLine("  0x064 - Keyboard command port");
        result.AppendLine();
        
        result.AppendLine("Serial Ports (COM):");
        result.AppendLine("  0x3F8-0x3FF - COM1");
        result.AppendLine("  0x2F8-0x2FF - COM2");
        result.AppendLine("  0x3E8-0x3EF - COM3");
        result.AppendLine("  0x2E8-0x2EF - COM4");
        result.AppendLine();
        
        result.AppendLine("Parallel Ports (LPT):");
        result.AppendLine("  0x378-0x37F - LPT1");
        result.AppendLine("  0x278-0x27F - LPT2");
        result.AppendLine();
        
        result.AppendLine("VGA/Video:");
        result.AppendLine("  0x3B4-0x3B5 - Monochrome CRT controller");
        result.AppendLine("  0x3C0-0x3CF - VGA registers");
        result.AppendLine("  0x3D4-0x3D5 - Color CRT controller");
        result.AppendLine();
        
        result.AppendLine("Sound Blaster:");
        result.AppendLine("  0x220-0x22F - Base address (configurable)");
        result.AppendLine();
        
        result.AppendLine("MIDI:");
        result.AppendLine("  0x330-0x331 - MPU-401 (configurable)");
        result.AppendLine();
        
        result.AppendLine("Joystick:");
        result.AppendLine("  0x200-0x207 - Game port");
        result.AppendLine();
        
        result.AppendLine($"Last Port Read: 0x{_ioPortDispatcher.LastPortRead:X4}");
        result.AppendLine($"Last Port Written: 0x{_ioPortDispatcher.LastPortWritten:X4}");
        
        return Task.FromResult(result.ToString());
    }

    [McpServerTool]
    [Description("Get detailed information about a specific device category")]
    public Task<string> GetDeviceInfo(
        [Description("Device category: dma, pic, timer, keyboard, serial, parallel, video, sound, midi, joystick")] string deviceCategory) {
        
        var result = new StringBuilder();
        
        switch (deviceCategory.ToLowerInvariant()) {
            case "dma":
                result.AppendLine("DMA Controller (Direct Memory Access):");
                result.AppendLine("Manages high-speed data transfers between memory and peripherals.");
                result.AppendLine();
                result.AppendLine("Ports 0x000-0x00F (Channels 0-3):");
                result.AppendLine("  0x000 - Channel 0 address");
                result.AppendLine("  0x001 - Channel 0 count");
                result.AppendLine("  0x002 - Channel 1 address");
                result.AppendLine("  0x003 - Channel 1 count");
                result.AppendLine("  0x004 - Channel 2 address");
                result.AppendLine("  0x005 - Channel 2 count");
                result.AppendLine("  0x006 - Channel 3 address");
                result.AppendLine("  0x007 - Channel 3 count");
                result.AppendLine("  0x008 - Status/Command register");
                result.AppendLine("  0x009 - Request register");
                result.AppendLine("  0x00A - Mask register");
                result.AppendLine("  0x00B - Mode register");
                result.AppendLine("  0x00C - Clear byte pointer flip-flop");
                result.AppendLine("  0x00D - Master clear");
                result.AppendLine("  0x00E - Clear mask register");
                result.AppendLine("  0x00F - Write all mask bits");
                break;

            case "pic":
                result.AppendLine("Programmable Interrupt Controller (8259):");
                result.AppendLine("Manages hardware interrupts (IRQs).");
                result.AppendLine();
                result.AppendLine("Master PIC (0x020-0x021):");
                result.AppendLine("  IRQ0 - Timer");
                result.AppendLine("  IRQ1 - Keyboard");
                result.AppendLine("  IRQ2 - Cascade to slave PIC");
                result.AppendLine("  IRQ3 - COM2/COM4");
                result.AppendLine("  IRQ4 - COM1/COM3");
                result.AppendLine("  IRQ5 - LPT2/Sound card");
                result.AppendLine("  IRQ6 - Floppy controller");
                result.AppendLine("  IRQ7 - LPT1");
                result.AppendLine();
                result.AppendLine("Slave PIC (0x0A0-0x0A1):");
                result.AppendLine("  IRQ8 - Real-time clock");
                result.AppendLine("  IRQ9 - Redirect to IRQ2");
                result.AppendLine("  IRQ10 - Available");
                result.AppendLine("  IRQ11 - Available");
                result.AppendLine("  IRQ12 - PS/2 Mouse");
                result.AppendLine("  IRQ13 - Math coprocessor");
                result.AppendLine("  IRQ14 - Primary ATA");
                result.AppendLine("  IRQ15 - Secondary ATA");
                break;

            case "timer":
                result.AppendLine("Programmable Interval Timer (8254):");
                result.AppendLine("Provides timing signals for system clock and speaker.");
                result.AppendLine();
                result.AppendLine("Ports:");
                result.AppendLine("  0x040 - Counter 0 (System timer, IRQ0 @ 18.2Hz)");
                result.AppendLine("  0x041 - Counter 1 (DRAM refresh)");
                result.AppendLine("  0x042 - Counter 2 (PC Speaker)");
                result.AppendLine("  0x043 - Control register");
                break;

            case "keyboard":
                result.AppendLine("Keyboard Controller (8042):");
                result.AppendLine("Handles keyboard input and PS/2 mouse.");
                result.AppendLine();
                result.AppendLine("Ports:");
                result.AppendLine("  0x060 - Data port (read/write)");
                result.AppendLine("  0x064 - Status register (read) / Command register (write)");
                result.AppendLine();
                result.AppendLine("Common commands:");
                result.AppendLine("  0x20 - Read command byte");
                result.AppendLine("  0x60 - Write command byte");
                result.AppendLine("  0xAA - Self test");
                result.AppendLine("  0xAD - Disable keyboard");
                result.AppendLine("  0xAE - Enable keyboard");
                result.AppendLine("  0xD0 - Read output port");
                result.AppendLine("  0xD1 - Write output port (controls A20 gate)");
                break;

            case "serial":
                result.AppendLine("Serial Ports (16550 UART):");
                result.AppendLine("Provide RS-232 serial communication.");
                result.AppendLine();
                result.AppendLine("Port offsets (add to base, e.g., COM1 = 0x3F8 + offset):");
                result.AppendLine("  +0 - Data register / Divisor latch low (DLAB=1)");
                result.AppendLine("  +1 - Interrupt enable / Divisor latch high (DLAB=1)");
                result.AppendLine("  +2 - Interrupt identification / FIFO control");
                result.AppendLine("  +3 - Line control register");
                result.AppendLine("  +4 - Modem control register");
                result.AppendLine("  +5 - Line status register");
                result.AppendLine("  +6 - Modem status register");
                result.AppendLine("  +7 - Scratch register");
                break;

            case "parallel":
                result.AppendLine("Parallel Ports (LPT):");
                result.AppendLine("Provide parallel printer communication.");
                result.AppendLine();
                result.AppendLine("Port offsets (add to base, e.g., LPT1 = 0x378 + offset):");
                result.AppendLine("  +0 - Data port");
                result.AppendLine("  +1 - Status port");
                result.AppendLine("  +2 - Control port");
                break;

            case "video":
                result.AppendLine("VGA Video Controller:");
                result.AppendLine("Controls video display modes and rendering.");
                result.AppendLine();
                result.AppendLine("Port groups:");
                result.AppendLine("  0x3C0-0x3C1 - Attribute controller");
                result.AppendLine("  0x3C2 - Miscellaneous output");
                result.AppendLine("  0x3C4-0x3C5 - Sequencer");
                result.AppendLine("  0x3C6-0x3C9 - DAC (palette)");
                result.AppendLine("  0x3CE-0x3CF - Graphics controller");
                result.AppendLine("  0x3D4-0x3D5 - CRT controller (color)");
                result.AppendLine("  0x3DA - Input status register");
                break;

            case "sound":
                result.AppendLine("Sound Blaster:");
                result.AppendLine("Provides digital audio and FM synthesis.");
                result.AppendLine();
                result.AppendLine("Typical base port: 0x220");
                result.AppendLine("Port offsets:");
                result.AppendLine("  +0 - FM left address");
                result.AppendLine("  +1 - FM left data");
                result.AppendLine("  +2 - FM right address");
                result.AppendLine("  +3 - FM right data");
                result.AppendLine("  +4 - Mixer address");
                result.AppendLine("  +5 - Mixer data");
                result.AppendLine("  +6 - DSP reset");
                result.AppendLine("  +8 - FM music status / data");
                result.AppendLine("  +A - DSP read data");
                result.AppendLine("  +C - DSP write buffer status");
                result.AppendLine("  +E - DSP read buffer status");
                break;

            case "midi":
                result.AppendLine("MPU-401 MIDI Interface:");
                result.AppendLine("Provides MIDI input/output for music.");
                result.AppendLine();
                result.AppendLine("Typical base port: 0x330");
                result.AppendLine("Ports:");
                result.AppendLine("  +0 - Data port");
                result.AppendLine("  +1 - Status (read) / Command (write)");
                break;

            case "joystick":
                result.AppendLine("Game Port / Joystick:");
                result.AppendLine("Provides analog joystick input.");
                result.AppendLine();
                result.AppendLine("Ports:");
                result.AppendLine("  0x200 - Read: joystick positions and button states");
                result.AppendLine("  0x201 - Status register");
                result.AppendLine();
                result.AppendLine("Button bits:");
                result.AppendLine("  Bit 4 - Joystick A button 1");
                result.AppendLine("  Bit 5 - Joystick A button 2");
                result.AppendLine("  Bit 6 - Joystick B button 1");
                result.AppendLine("  Bit 7 - Joystick B button 2");
                break;

            default:
                result.AppendLine($"Unknown device category: {deviceCategory}");
                result.AppendLine();
                result.AppendLine("Available categories:");
                result.AppendLine("  dma, pic, timer, keyboard, serial, parallel, video, sound, midi, joystick");
                break;
        }
        
        return Task.FromResult(result.ToString());
    }
}
