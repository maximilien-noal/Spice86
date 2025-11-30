# Fork Feature Inventory and PR Tracking

This document provides a detailed inventory of all unique features in the fork (`maximilien-noal/Spice86`) that need to be contributed to upstream (`OpenRakis/Spice86`).

## Feature Inventory

### ✅ Legend
- 🔴 Not started
- 🟡 PR in progress
- 🟢 Merged to upstream
- ⏸️ On hold / Blocked

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

### Themed Debugger UI Docks 🔴

**Source Files:**
- `src/Spice86/Views/*.axaml`
- `src/Spice86/ViewModels/*.cs`

**Description:**
Adds debugger docks for DOS, BIOS, EMS, XMS, Sound Blaster, PIT, PIC, DMA, OPL3.

**Dependencies:** None

**Estimated Size:** Very Large PR (~1500 lines)

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
| Sound | 3 | 3 | ~580 |
| DOS Subsystem | 12 | 10 | ~2000 |
| BIOS/Hardware | 4 | 4 | ~320 |
| Batch Processing | 3 | 3 | ~1150 |
| Developer Tools | 3 | 3 | ~2100 |
| Infrastructure | 3 | 3 | ~325 |
| **Total** | **28** | **26** | **~6475** |
