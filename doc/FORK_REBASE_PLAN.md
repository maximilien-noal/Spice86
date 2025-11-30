# Plan to Rebase Fork onto Upstream Master

## Overview

This document outlines the strategy for rebasing the `maximilien-noal/Spice86` fork onto `OpenRakis/Spice86` upstream master, and a plan for contributing all fork changes back to upstream through small, cumulative PRs.

## Current State Analysis

### Fork (maximilien-noal/Spice86)

The fork has significantly diverged from upstream with many new features:

| Category | Features |
|----------|----------|
| **Sound** | Gravis UltraSound (GUS) emulation, Sound Blaster DSP commands (0xE2, 0xE4, 0xE8) |
| **DOS** | FCB support, TSR support (INT 21h/31h), batch file processing, COMMAND.COM simulation, PSP lifecycle management, memory allocation strategies |
| **BIOS/Hardware** | VESA VBE 1.0 functions, RateGenerator mode for PC Speaker, OS HOOK handlers |
| **Developer Tools** | MCP server with stdio transport, themed debugger UI docks |
| **Infrastructure** | .NET 10 migration, comprehensive XML documentation |
| **Input** | Improved keyboard infrastructure with InputEventQueue |

### Upstream (OpenRakis/Spice86)

The upstream has evolved with:
- Full PS/2 controller and keyboard implementation
- Proper FIFO for keyboard
- INT 21h 50H/51H PSP handling
- Various refactoring and documentation improvements

## Rebase Strategy

### Option A: Interactive Rebase (Recommended for Clean History)

This approach creates a clean linear history but requires careful conflict resolution.

```bash
# 1. Add upstream remote
git remote add upstream https://github.com/OpenRakis/Spice86.git
git fetch upstream

# 2. Create backup branch
git checkout master
git checkout -b master-backup-$(date +%Y%m%d)

# 3. Interactive rebase onto upstream
git checkout master
git rebase -i upstream/master

# During rebase:
# - Resolve conflicts for each commit
# - Drop commits that duplicate upstream work
# - Squash related commits for cleaner history

# 4. Force push (after backup verification)
git push origin master --force
```

**Pros:**
- Clean, linear history
- Each commit is atomic and reviewable

**Cons:**
- Complex conflict resolution
- Requires force push (history rewrite)
- Risk of losing work if not backed up

### Option B: Merge Upstream (Recommended for Safety)

This approach preserves all history and is safer.

```bash
# 1. Add upstream remote
git remote add upstream https://github.com/OpenRakis/Spice86.git
git fetch upstream

# 2. Create integration branch
git checkout master
git checkout -b integrate-upstream

# 3. Merge upstream master
git merge upstream/master

# 4. Resolve conflicts
# Conflicts are likely in:
# - Keyboard/PS2 related files
# - Spice86DependencyInjection.cs
# - DOS INT 21h handlers

# 5. Test thoroughly
dotnet build
dotnet test

# 6. Merge back to master
git checkout master
git merge integrate-upstream
git push origin master
```

**Pros:**
- Preserves full history
- Lower risk of data loss
- Easier to review what was merged

**Cons:**
- More complex history graph
- Merge commits can obscure individual changes

## Small PR Strategy for Upstream Contribution

The goal is to close the fork by contributing all changes back to upstream through small, reviewable PRs.

### Phase 0: RTC/CMOS and Time Services (Week 1)

| PR # | Title | Files | Priority |
|------|-------|-------|----------|
| 1 | Implement RealTimeClock (CMOS) MC146818 emulation | `RealTimeClock.cs`, `CmosRegisters.cs`, `CmosRegisterAddresses.cs` | High |
| 2 | Add RTC INT 70h handler | `RtcInt70Handler.cs` | High |
| 3 | Implement INT 1Ah BIOS time services | `BiosInt1AHandler.cs` | High |
| 4 | Add INT 15h AH=86h BIOS Wait function | `SystemBiosInt15Handler.cs` | Medium |
| 5 | Implement INT 15h AH=83h Event Wait | `SystemBiosInt15Handler.cs` | Medium |
| 6 | Add DOS INT 21h date/time CMOS access | `DosInt21Handler.cs` | Medium |

### Phase 1: Infrastructure and Documentation (Week 2)

| PR # | Title | Files | Priority |
|------|-------|-------|----------|
| 7 | Add comprehensive XML documentation to shared projects | `Bufdio.Spice86/`, `Spice86.Logging/`, `Spice86.Shared/` | Low |
| 8 | Block Avalonia telemetry via Directory.Build.props | `Directory.Build.props` | Medium |
| 9 | Update CONTRIBUTING.md with development guidelines | `CONTRIBUTING.md` | Low |

### Phase 2: Core DOS Improvements (Week 3-4)

| PR # | Title | Files | Priority |
|------|-------|-------|----------|
| 10 | Add DOS memory allocation strategy support (INT 21h/58h) | `DosMemoryManager.cs`, `DosMemoryAllocationStrategy.cs` | High |
| 11 | Fix DOS environment block to preserve full program path | `DosFileManager.cs`, `DosProcessManager.cs` | High |
| 12 | Add COMMAND.COM simulation as PSP chain root | `CommandCom.cs`, `Dos.cs` | Medium |
| 13 | Implement FCB (File Control Block) support | `DosFileControlBlock.cs`, `DosFcbManager.cs` | Medium |
| 14 | Add FCB Find First/Next (INT 21h 0x11/0x12) | `DosFcbManager.cs`, `DosInt21Handler.cs` | Medium |
| 15 | Implement TSR support (INT 21h/31h) | `DosProcessManager.cs`, `DosInt21Handler.cs` | Medium |
| 16 | Add DOS process lifecycle management (INT 21h/4Dh) | `DosProcessManager.cs`, `DosProgramSegmentPrefix.cs` | High |
| 17 | Implement INT 21h/52h Get DOS SYSVARS pointer | `DosInt21Handler.cs` | Low |
| 18 | Add Create Child PSP (INT 21h/55h) | `DosProcessManager.cs` | Low |
| 19 | Implement character input with echo (INT 21h/01h) | `DosInt21Handler.cs` | Low |
| 20 | Add CDS/DBCS structures and INT 21h/63h | `DosInt21Handler.cs` | Low |
| 21 | Refactor IOCTL with enums and DosDeviceHeader improvements | `DeviceInformationFlags.cs`, `DosDeviceHeader.cs` | Low |

### Phase 3: BIOS and Hardware (Week 5)

| PR # | Title | Files | Priority |
|------|-------|-------|----------|
| 22 | Implement VESA VBE 1.0 functions (INT 10h/4Fh) | `VgaBios.cs`, `VideoModeInfo.cs` | Medium |
| 23 | Add RateGenerator mode to PC Speaker | `PcSpeaker.cs` | Low |
| 24 | Add OS HOOK Device Busy/Post handlers (INT 15h) | `BiosInt15Handler.cs` | Low |
| 25 | Fix EMS implementation bugs (handle ID, save/restore) | `ExpandedMemoryManager.cs` | Medium |

### Phase 4: Sound (Week 6)

| PR # | Title | Files | Priority |
|------|-------|-------|----------|
| 26 | Implement Sound Blaster DSP command 0xE2 (DMA ID) | `SoundBlaster.cs` | Low |
| 27 | Implement Sound Blaster DSP command 0xE4/0xE8 (Test Register) | `SoundBlaster.cs` | Low |
| 28 | Implement complete Gravis UltraSound emulation | `GravisUltraSound.cs`, `GusVoice.cs` | Medium |

### Phase 5: Developer Tools (Week 7)

| PR # | Title | Files | Priority |
|------|-------|-------|----------|
| 29 | Add MCP server with stdio transport | `McpServer.cs`, `McpStdioTransport.cs` | High |
| 30 | Add CFG CPU graph inspection to MCP server | `McpServer.cs` | Medium |

### Phase 6: User Interface Views/ViewModels (Week 8-9)

| PR # | Title | Files | Priority |
|------|-------|-------|----------|
| 31 | Add docking framework to DebugWindow | `DebugWindow.axaml`, Dock.* packages | High |
| 32 | Add BIOS View and ViewModel | `BiosView.axaml`, `BiosViewModel.cs` | Medium |
| 33 | Add DOS View and ViewModel | `DosView.axaml`, `DosViewModel.cs` | Medium |
| 34 | Add EMS/XMS Views and ViewModels | `EmsView.axaml`, `EmsViewModel.cs`, etc. | Medium |
| 35 | Add Timer/PIC/DMA Views and ViewModels | `TimerView.axaml`, `PicView.axaml`, etc. | Medium |
| 36 | Add Sound Blaster/OPL3 Views and ViewModels | `SoundBlasterView.axaml`, `Opl3View.axaml` | Medium |
| 37 | Add MCB/PSP Graph Views | `McbGraphView.axaml`, `PspGraphView.axaml` | Medium |
| 38 | Add MCP/GDB Server Views | `McpServerView.axaml`, `GdbServerView.axaml` | Low |

### Phase 7: Batch Processing (Week 10)

| PR # | Title | Files | Priority |
|------|-------|-------|----------|
| 39 | Add DOS batch file processing support | `BatchProcessor.cs`, `BatchExecutor.cs` | Medium |
| 40 | Implement FOR, IF, GOTO, CALL batch commands | `BatchCommands/` | Medium |
| 41 | Add AutoexecGenerator and Z: drive support | `AutoexecGenerator.cs`, `DosDriveManager.cs` | Low |

### Phase 8: Final Cleanup (Week 11)

| PR # | Title | Files | Priority |
|------|-------|-------|----------|
| 42 | Fix IOCTL improvements | `DosInt21Handler.cs` | Low |
| 43 | .NET 10 migration | `*.csproj`, `Directory.Build.props` | High |
| 44 | Final documentation updates | `doc/`, `README.md` | Low |

## PR Best Practices

### For Each PR:

1. **Single Responsibility**: Each PR should do one thing well
2. **Tests Included**: Add ASM-based tests when possible (preferred over unit tests)
3. **Documentation**: Update XML comments and relevant docs
4. **Build Verification**: Ensure CI passes
5. **Descriptive Title**: Use conventional commits format
6. **Reference Issues**: Link to any related issues

### Commit Message Format

```
type(scope): description

- Details of change 1
- Details of change 2

Refs: #issue-number (if applicable)
```

Types: `feat`, `fix`, `docs`, `refactor`, `test`, `chore`

### Managing Conflicts with Upstream

When upstream changes conflict with pending PRs:

1. Rebase your feature branch onto latest upstream master
2. Resolve conflicts, preferring upstream's approach when architecturally appropriate
3. Update tests if behavior changed
4. Re-request review

## Timeline Estimate

| Phase | Description | Duration |
|-------|-------------|----------|
| Week 1 | RTC/CMOS & Time Services | 6 PRs |
| Week 2 | Infrastructure & Documentation | 3 PRs |
| Week 3-4 | Core DOS Improvements | 12 PRs |
| Week 5 | BIOS and Hardware | 4 PRs |
| Week 6 | Sound | 3 PRs |
| Week 7 | Developer Tools | 2 PRs |
| Week 8-9 | UI Views/ViewModels | 8 PRs |
| Week 10 | Batch Processing | 3 PRs |
| Week 11 | Final Cleanup | 3 PRs |

**Total: ~11 weeks, ~44 PRs**

## Handling Keyboard/Input Conflicts

Both fork and upstream have made significant keyboard changes. Strategy:

1. **Upstream's approach wins** for core PS/2 controller and keyboard hardware emulation
2. **Fork's InputEventQueue** may need to be adapted to work with upstream's new keyboard architecture
3. Create a dedicated PR for keyboard integration after understanding both implementations

## Success Criteria

The fork can be closed when:

1. ✅ All features from fork are in upstream
2. ✅ All tests pass in upstream
3. ✅ Fork can be deleted or archived
4. ✅ No unique code remains in fork

## Notes

- Coordinate with upstream maintainers on PR order
- Some features may be declined - have fallback plans
- Keep fork in sync with upstream during contribution period
- Consider creating a project board to track PR progress
