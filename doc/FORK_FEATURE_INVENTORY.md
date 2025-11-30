# Fork Feature Inventory and PR Tracking

This document provides a detailed inventory of all unique features in the fork (`maximilien-noal/Spice86`) that need to be contributed to upstream (`OpenRakis/Spice86`).

## Feature Inventory

### ✅ Legend
- 🔴 Not started
- 🟡 PR in progress
- 🟢 Merged to upstream
- ⏸️ On hold / Blocked

---

## Category 0: RTC/CMOS and Time Services

### RealTimeClock (CMOS) Implementation 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/Devices/Cmos/RealTimeClock.cs`
- `src/Spice86.Core/Emulator/Devices/Cmos/CmosRegisters.cs`
- `src/Spice86.Core/Emulator/Devices/Cmos/CmosRegisterAddresses.cs`

**Description:**
Complete MC146818 RTC/CMOS chip emulation with:
- Time-based event scheduling with pause awareness
- BCD mode support
- NMI control
- Periodic interrupt control

**Dependencies:** None

**Estimated Size:** Large PR (~400 lines)

---

### RTC INT 70h Handler 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/InterruptHandlers/Bios/RtcInt70Handler.cs`
- `tests/Spice86.Tests/Bios/RtcIntegrationTests.cs`
- `tests/Spice86.Tests/Bios/RtcIntegrationTests_New.cs`
- `tests/Spice86.Tests/Resources/RtcTests/`

**Description:**
Implements INT 70h (IRQ 8) RTC periodic interrupt handler based on DOSBox pattern:
- Periodic interrupt handling
- Wait timeout countdown
- User callback support at INT 4Ah

**Dependencies:** RealTimeClock

**Estimated Size:** Medium PR (~200 lines)

---

### INT 1Ah BIOS Time Services 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/InterruptHandlers/Bios/BiosInt1AHandler.cs` (modifications)

**Description:**
Implements BIOS INT 1Ah time functions:
- Read/Write system clock
- Read/Write RTC date/time
- Set/Clear alarm
- BCD to binary conversions

**Dependencies:** RealTimeClock

**Estimated Size:** Medium PR (~150 lines)

---

### INT 15h AH=86h BIOS Wait 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/InterruptHandlers/Bios/SystemBiosInt15Handler.cs` (modifications)

**Description:**
Implements BIOS Wait function with:
- PIC event scheduling instead of blocking loops
- ASM handler placed in guest memory
- Non-blocking wait using callbacks

**Dependencies:** DualPic, BiosDataArea

**Estimated Size:** Medium PR (~200 lines)

---

### INT 15h AH=83h Event Wait 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/InterruptHandlers/Bios/SystemBiosInt15Handler.cs` (modifications)

**Description:**
Implements INT 15h AH=83h event wait function based on DOSBox Staging:
- RTC periodic interrupt control
- Wait counter in BDA
- User flag notification

**Dependencies:** RealTimeClock, BiosDataArea

**Estimated Size:** Medium PR (~150 lines)

---

### DOS INT 21h Date/Time Functions 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/OperatingSystem/DosInt21Handler.cs` (modifications)

**Description:**
DOS date/time functions that access CMOS via I/O ports (not DateTime.Now):
- Get/Set Date (AH=2Ah/2Bh)
- Get/Set Time (AH=2Ch/2Dh)

**Dependencies:** RealTimeClock

**Estimated Size:** Small PR (~100 lines)

---

## Category 1: Sound Emulation

### Gravis UltraSound (GUS) Emulation 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/Devices/Sound/Gus/GravisUltraSound.cs`
- `src/Spice86.Core/Emulator/Devices/Sound/Gus/GusVoice.cs`

**Description:**
Complete Gravis UltraSound emulation with 32-voice wavetable synthesis, DMA transfers, and timer events.

**Dependencies:** PIC-based timer events

**Estimated Size:** Large PR (~500 lines)

---

### Sound Blaster DSP Command 0xE2 (DMA Identification) 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/Devices/Sound/Blaster/SoundBlaster.cs`

**Description:**
Implements DMA identification command and handles DspReadData port writes.

**Dependencies:** None

**Estimated Size:** Small PR (~50 lines)

---

### Sound Blaster DSP Commands 0xE4/0xE8 (Test Register) 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/Devices/Sound/Blaster/SoundBlaster.cs`

**Description:**
Implements Write Test Register (0xE4) and Read Test Register (0xE8) commands.

**Dependencies:** None

**Estimated Size:** Small PR (~30 lines)

---

## Category 2: DOS Subsystem

### DOS Memory Allocation Strategy (INT 21h/58h) 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/OperatingSystem/DosMemoryManager.cs`
- `src/Spice86.Core/Emulator/OperatingSystem/Enums/DosMemoryAllocationStrategy.cs`
- `tests/Spice86.Tests/DosMemoryAllocationStrategyIntegrationTests.cs`

**Description:**
- Adds First/Best/Last fit memory allocation modes
- Implements Get/Set Memory Allocation Strategy handler
- Includes MCB chain integrity verification

**Dependencies:** None

**Estimated Size:** Medium PR (~200 lines)

---

### DOS Environment Block Fix 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/OperatingSystem/DosFileManager.cs`
- `src/Spice86.Core/Emulator/OperatingSystem/DosProcessManager.cs`
- `src/Spice86.Core/Emulator/OperatingSystem/DosDriveManager.cs`

**Description:**
Fixes environment block to preserve full program path (e.g., `C:\GAMES\MYGAME.EXE` instead of `C:\MYGAME.EXE`).

**Dependencies:** None

**Estimated Size:** Medium PR (~150 lines)

---

### COMMAND.COM Simulation 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/OperatingSystem/CommandCom.cs`
- `src/Spice86.Core/Emulator/OperatingSystem/Dos.cs`
- `src/Spice86.Core/Emulator/OperatingSystem/DosProcessManager.cs`

**Description:**
Adds COMMAND.COM simulation as root of PSP chain, including EXEC API implementation.

**Dependencies:** DOS Process lifecycle

**Estimated Size:** Large PR (~400 lines)

---

### FCB (File Control Block) Support 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/OperatingSystem/DosFileControlBlock.cs`
- `src/Spice86.Core/Emulator/OperatingSystem/DosExtendedFileControlBlock.cs`
- `src/Spice86.Core/Emulator/OperatingSystem/DosFcbManager.cs`

**Description:**
Implements FreeDOS-compatible FCB operations for legacy DOS programs.

**Dependencies:** None

**Estimated Size:** Large PR (~500 lines)

---

### FCB Find First/Next (INT 21h 0x11/0x12) 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/OperatingSystem/DosFcbManager.cs`
- `src/Spice86.Core/Emulator/OperatingSystem/DosInt21Handler.cs`

**Description:**
Implements FCB-based file searching with proper search state management.

**Dependencies:** FCB Support

**Estimated Size:** Medium PR (~200 lines)

---

### TSR Support (INT 21h/31h) 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/OperatingSystem/DosProcessManager.cs`
- `src/Spice86.Core/Emulator/OperatingSystem/DosInt21Handler.cs`

**Description:**
Implements Terminate and Stay Resident with return to parent process.

**Dependencies:** DOS Process lifecycle

**Estimated Size:** Medium PR (~150 lines)

---

### DOS Process Lifecycle Management (INT 21h/4Dh) 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/OperatingSystem/DosProcessManager.cs`
- `src/Spice86.Core/Emulator/OperatingSystem/DosProgramSegmentPrefix.cs`

**Description:**
Implements Get Child Return Code with proper termination handling and file handle closure.

**Dependencies:** None

**Estimated Size:** Medium PR (~200 lines)

---

### INT 21h/52h Get DOS SYSVARS 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/OperatingSystem/DosInt21Handler.cs`

**Description:**
Returns pointer to DOS List of Lists (SYSVARS) structure.

**Dependencies:** None

**Estimated Size:** Small PR (~50 lines)

---

### Create Child PSP (INT 21h/55h) 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/OperatingSystem/DosProcessManager.cs`
- `src/Spice86.Core/Emulator/OperatingSystem/DosInt21Handler.cs`

**Description:**
Creates child PSP based on DOS 4 and DOSBox behavior with no-inherit flag support.

**Dependencies:** DOS Process lifecycle

**Estimated Size:** Medium PR (~150 lines)

---

### Character Input with Echo (INT 21h/01h) 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/OperatingSystem/DosInt21Handler.cs`

**Description:**
Reads character from STDIN and echoes to STDOUT.

**Dependencies:** None

**Estimated Size:** Small PR (~30 lines)

---

### DOS IOCTL Improvements 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/OperatingSystem/DosInt21Handler.cs`

**Description:**
- Fix file seek bug in GetInputStatus
- Implement IsHandleRemote (IOCTL function 0x0A)

**Dependencies:** None

**Estimated Size:** Small PR (~50 lines)

---

### DOS CDS and DBCS Structures 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/OperatingSystem/Structures/DosCds.cs` (if exists)
- `src/Spice86.Core/Emulator/OperatingSystem/Structures/DosDbcs.cs` (if exists)
- `src/Spice86.Core/Emulator/OperatingSystem/DosInt21Handler.cs`

**Description:**
Current Directory Structure (CDS) and Double Byte Character Set (DBCS) support:
- INT 21h AH=63h Get DBCS Lead Byte Table
- CDS table for drive current directories

**Dependencies:** None

**Estimated Size:** Medium PR (~150 lines)

---

### IOCTL Refactoring with Enums 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/OperatingSystem/Enums/DeviceInformationFlags.cs`
- `src/Spice86.Core/Emulator/OperatingSystem/Enums/GenericIoctlCategory.cs`
- `src/Spice86.Core/Emulator/OperatingSystem/Structures/DosDeviceHeader.cs`

**Description:**
Refactors IOCTL implementation:
- Replace magic numbers with descriptive enums
- Use MemoryBasedDataStructure for DosDeviceHeader
- Comprehensive ASM-based IOCTL integration tests

**Dependencies:** None

**Estimated Size:** Medium PR (~200 lines)

---

## Category 3: BIOS and Hardware

### VESA VBE 1.0 Functions (INT 10h/4Fh) 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/InterruptHandlers/Bios/VgaBios.cs`
- `src/Spice86.Core/Emulator/Devices/Video/VideoModeInfo.cs`

**Description:**
Implements VESA BIOS Extension 1.0 for video mode queries.

**Dependencies:** None

**Estimated Size:** Medium PR (~200 lines)

---

### RateGenerator Mode for PC Speaker 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/Devices/Sound/PcSpeaker.cs`

**Description:**
Adds RateGenerator and RateGeneratorAlias mode support to PIT control.

**Dependencies:** None

**Estimated Size:** Small PR (~30 lines)

---

### OS HOOK Device Handlers (INT 15h) 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/InterruptHandlers/Bios/BiosInt15Handler.cs`

**Description:**
Adds Device Busy and Device Post handlers to BIOS INT 15h.

**Dependencies:** None

**Estimated Size:** Small PR (~40 lines)

---

### EMS Bug Fixes 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/OperatingSystem/ExpandedMemoryManager.cs`

**Description:**
Fixes handle ID reuse, off-by-one errors, and save/restore issues.

**Dependencies:** None

**Estimated Size:** Small PR (~50 lines)

---

## Category 4: Batch Processing

### DOS Batch File Support 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/OperatingSystem/Command/BatchProcessing/BatchProcessor.cs`
- `src/Spice86.Core/Emulator/OperatingSystem/Command/BatchProcessing/BatchContext.cs`
- `src/Spice86.Core/Emulator/OperatingSystem/Command/BatchProcessing/BatchExecutor.cs`
- `src/Spice86.Core/Emulator/OperatingSystem/Command/BatchProcessing/IBatchOutput.cs`
- `src/Spice86.Core/Emulator/OperatingSystem/Command/BatchProcessing/ILineReader.cs`

**Description:**
DOSBox staging-inspired batch file processing architecture.

**Dependencies:** COMMAND.COM simulation

**Estimated Size:** Large PR (~600 lines)

---

### Batch Commands (FOR, IF, GOTO, CALL) 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/OperatingSystem/Command/BatchProcessing/BatchCommand.cs`
- Various command implementations

**Description:**
Implements SET, IF, SHIFT, PAUSE, EXIT, FOR, GOTO, CALL batch commands.

**Dependencies:** DOS Batch File Support

**Estimated Size:** Large PR (~400 lines)

---

### AutoexecGenerator and Z: Drive 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/OperatingSystem/Command/BatchProcessing/AutoexecGenerator.cs`
- `src/Spice86.Core/Emulator/OperatingSystem/DosDriveManager.cs`

**Description:**
Generates AUTOEXEC.BAT and adds Z: drive mounting support.

**Dependencies:** DOS Batch File Support

**Estimated Size:** Medium PR (~150 lines)

---

## Category 5: Developer Tools

### MCP Server with stdio Transport 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/Mcp/McpServer.cs`
- `src/Spice86.Core/Emulator/Mcp/McpStdioTransport.cs`
- `src/Spice86.Core/Emulator/Mcp/McpTypes.cs`
- `doc/mcpServerReadme.md`
- `doc/mcpServerExample.md`

**Description:**
Model Context Protocol server for AI-assisted debugging with thread-safe state inspection.

**Dependencies:** None

**Estimated Size:** Large PR (~500 lines)

---

### CFG CPU Graph Inspection 🔴

**Source Files:**
- `src/Spice86.Core/Emulator/Mcp/McpServer.cs`

**Description:**
Adds read_cfg_cpu_graph tool to MCP server for CFG CPU state inspection.

**Dependencies:** MCP Server

**Estimated Size:** Small PR (~100 lines)

---

## Category 5b: User Interface Views and ViewModels

### Debugger Window with Docking Framework 🔴

**Source Files:**
- `src/Spice86/Views/DebugWindow.axaml`
- `src/Spice86/Views/DebugWindow.axaml.cs`

**Description:**
Implements docking framework using wieslawsoltes/Dock library:
- Detachable, dockable debug panels
- Persistent dock layout
- Document-based tab structure

**Dependencies:** Dock.Avalonia NuGet packages

**Estimated Size:** Medium PR (~300 lines)

---

### BIOS View and ViewModel 🔴

**Source Files:**
- `src/Spice86/Views/BiosView.axaml`
- `src/Spice86/Views/BiosView.axaml.cs`
- `src/Spice86/ViewModels/BiosViewModel.cs`

**Description:**
Comprehensive BIOS Data Area (BDA) observability dock:
- Video mode, keyboard state, equipment info
- Serial/parallel port configuration
- Timer tick count, break key state
- Live updates via DispatcherTimer

**Dependencies:** None

**Estimated Size:** Large PR (~400 lines)

---

### DOS View and ViewModel 🔴

**Source Files:**
- `src/Spice86/Views/DosView.axaml`
- `src/Spice86/Views/DosView.axaml.cs`
- `src/Spice86/ViewModels/DosViewModel.cs`

**Description:**
DOS subsystem observability dock:
- MCB chain visualization
- PSP details and file handle table
- SysVars inspection
- Current drive/directory, DOS version

**Dependencies:** None

**Estimated Size:** Large PR (~450 lines)

---

### EMS View and ViewModel 🔴

**Source Files:**
- `src/Spice86/Views/EmsView.axaml`
- `src/Spice86/Views/EmsView.axaml.cs`
- `src/Spice86/ViewModels/EmsViewModel.cs`

**Description:**
Expanded Memory Manager observability:
- EMS version, page frame address
- Handle allocation and page mapping
- Memory usage statistics

**Dependencies:** None

**Estimated Size:** Medium PR (~250 lines)

---

### XMS View and ViewModel 🔴

**Source Files:**
- `src/Spice86/Views/XmsView.axaml` (if exists)
- `src/Spice86/ViewModels/XmsViewModel.cs` (if exists)

**Description:**
Extended Memory Manager observability:
- XMS version and driver info
- Handle table with block details
- Free/used memory statistics

**Dependencies:** None

**Estimated Size:** Medium PR (~200 lines)

---

### Timer (PIT) View and ViewModel 🔴

**Source Files:**
- `src/Spice86/Views/TimerView.axaml`
- `src/Spice86/ViewModels/TimerViewModel.cs`

**Description:**
Intel 8254 PIT observability:
- Channel 0/1/2 state and count
- Mode, reload value, output state
- Timer IRQ statistics

**Dependencies:** None

**Estimated Size:** Medium PR (~150 lines)

---

### PIC View and ViewModel 🔴

**Source Files:**
- `src/Spice86/Views/PicView.axaml`
- `src/Spice86/Views/PicView.axaml.cs`
- `src/Spice86/ViewModels/PicViewModel.cs`

**Description:**
Dual 8259 PIC observability:
- IRR, ISR, IMR registers
- Master/slave configuration
- Pending interrupt visualization

**Dependencies:** None

**Estimated Size:** Medium PR (~200 lines)

---

### DMA View and ViewModel 🔴

**Source Files:**
- `src/Spice86/Views/DmaView.axaml`
- `src/Spice86/Views/DmaView.axaml.cs`
- `src/Spice86/ViewModels/DmaViewModel.cs`

**Description:**
DMA controller observability:
- Channel 0-7 state and addressing
- Transfer mode and direction
- Current address and count

**Dependencies:** None

**Estimated Size:** Medium PR (~150 lines)

---

### OPL3/FM View and ViewModel 🔴

**Source Files:**
- `src/Spice86/Views/Opl3View.axaml`
- `src/Spice86/Views/Opl3View.axaml.cs`
- `src/Spice86/ViewModels/Opl3ViewModel.cs`

**Description:**
OPL3 FM synthesizer observability:
- Register state
- Channel/operator status
- Audio output status

**Dependencies:** None

**Estimated Size:** Medium PR (~100 lines)

---

### Sound Blaster View and ViewModel 🔴

**Source Files:**
- `src/Spice86/Views/SoundBlasterView.axaml`
- `src/Spice86/ViewModels/SoundBlasterViewModel.cs`

**Description:**
Sound Blaster DSP observability:
- DSP version and ports
- DMA channel configuration
- IRQ settings, mixer state

**Dependencies:** None

**Estimated Size:** Medium PR (~200 lines)

---

### GDB Server View and ViewModel 🔴

**Source Files:**
- `src/Spice86/Views/GdbServerView.axaml`
- `src/Spice86/Views/GdbServerView.axaml.cs`
- `src/Spice86/ViewModels/GdbServerViewModel.cs`

**Description:**
GDB remote debugging server observability:
- Connection status
- Port configuration
- Debug session state

**Dependencies:** None

**Estimated Size:** Small PR (~80 lines)

---

### MCP Server View and ViewModel 🔴

**Source Files:**
- `src/Spice86/Views/McpServerView.axaml`
- `src/Spice86/Views/McpServerView.axaml.cs`
- `src/Spice86/ViewModels/McpServerViewModel.cs`

**Description:**
MCP server observability:
- Server status and configuration
- Available tools list
- Request/response logging

**Dependencies:** MCP Server

**Estimated Size:** Medium PR (~150 lines)

---

### MCB Graph View and ViewModel 🔴

**Source Files:**
- `src/Spice86/Views/McbGraphView.axaml`
- `src/Spice86/Views/McbGraphView.axaml.cs`
- `src/Spice86/ViewModels/McbGraphViewModel.cs`

**Description:**
Memory Control Block chain visualization:
- Graph-based MCB visualization using AvaloniaGraphControl
- Block ownership and size display
- Chain integrity validation

**Dependencies:** AvaloniaGraphControl NuGet package

**Estimated Size:** Medium PR (~200 lines)

---

### PSP Graph View and ViewModel 🔴

**Source Files:**
- `src/Spice86/Views/PspGraphView.axaml`
- `src/Spice86/Views/PspGraphView.axaml.cs`
- `src/Spice86/ViewModels/PspGraphViewModel.cs`

**Description:**
Program Segment Prefix chain visualization:
- PSP parent/child relationships
- Process tree visualization
- File handle table display

**Dependencies:** AvaloniaGraphControl NuGet package

**Estimated Size:** Medium PR (~200 lines)

---

### Software Mixer ViewModel 🔴

**Source Files:**
- `src/Spice86/ViewModels/SoftwareMixerViewModel.cs`

**Description:**
Audio software mixer observability for combined audio output.

**Dependencies:** None

**Estimated Size:** Small PR (~80 lines)

---

### MIDI ViewModel 🔴

**Source Files:**
- `src/Spice86/Views/MidiView.axaml`
- `src/Spice86/Views/MidiView.axaml.cs`
- `src/Spice86/ViewModels/MidiViewModel.cs`

**Description:**
MIDI output status observability.

**Dependencies:** None

**Estimated Size:** Small PR (~60 lines)

---

## Category 6: Infrastructure

### .NET 10 Migration 🔴

**Source Files:**
- All `*.csproj` files
- `Directory.Build.props`
- `global.json`

**Description:**
Migrates project from .NET 8 to .NET 10.

**Dependencies:** None (do last)

**Estimated Size:** Small PR (~20 lines)

---

### Block Avalonia Telemetry 🔴

**Source Files:**
- `Directory.Build.props`

**Description:**
Adds AVALONIA_TELEMETRY_OPTOUT environment variable.

**Dependencies:** None

**Estimated Size:** Very Small PR (~5 lines)

---

### XML Documentation 🔴

**Source Files:**
- Multiple files across `Bufdio.Spice86/`, `Spice86.Logging/`, `Spice86.Shared/`

**Description:**
Adds comprehensive XML documentation to public APIs.

**Dependencies:** None

**Estimated Size:** Medium PR (~300 lines)

---

## Keyboard/Input (Special Case) ⏸️

**Note:** Both fork and upstream have made significant changes to keyboard handling. This requires careful coordination:

**Fork changes:**
- `InputEventQueue` for thread-safe input processing
- `KeyCodes.cs`, `KeyboardMap.cs` for key mapping
- Modified `PS2Keyboard.cs` integration

**Upstream changes:**
- Full PS/2 controller implementation
- Proper FIFO for keyboard
- INT 9H refactoring

**Strategy:**
1. Analyze both implementations
2. Identify which upstream's architecture should be preserved
3. Adapt fork's useful features to work with upstream's architecture

---

## Summary Statistics

| Category | Features | Est. PRs | Est. Lines |
|----------|----------|----------|------------|
| RTC/CMOS & Time | 6 | 5 | ~1200 |
| Sound | 3 | 3 | ~580 |
| DOS Subsystem | 14 | 12 | ~2350 |
| BIOS/Hardware | 4 | 4 | ~320 |
| Batch Processing | 3 | 3 | ~1150 |
| Developer Tools | 3 | 3 | ~600 |
| UI Views/ViewModels | 17 | 15 | ~2800 |
| Infrastructure | 3 | 3 | ~325 |
| **Total** | **53** | **48** | **~9325** |

*Note: Some features may be combined into fewer PRs. Actual line counts will vary.*
