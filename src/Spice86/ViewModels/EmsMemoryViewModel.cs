namespace Spice86.ViewModels;

using Avalonia.Threading;

using CommunityToolkit.Mvvm.Messaging;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Function.Dump;
using Spice86.Core.Emulator.InterruptHandlers.Dos.Ems;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.VM;
using Spice86.Shared.Emulator.Memory;
using Spice86.Shared.Utils;
using Spice86.ViewModels.Services;

/// <summary>
/// ViewModel for viewing EMS (Expanded Memory Specification) memory.
/// EMS uses a 64KB page frame at segment 0xE000 to access expanded memory.
/// </summary>
public partial class EmsMemoryViewModel : MemoryViewModel {
    public EmsMemoryViewModel(IMemory memory, MemoryDataExporter memoryDataExporter, State state,
        BreakpointsViewModel breakpointsViewModel, IPauseHandler pauseHandler,
        IMessenger messenger, IUIDispatcher uiDispatcher, ITextClipboard textClipboard,
        IHostStorageProvider storageProvider, IStructureViewModelFactory structureViewModelFactory,
        bool canCloseTab = false, string? startAddress = null,
        string? endAddress = null) :
            base(memory, memoryDataExporter, state, breakpointsViewModel, pauseHandler, messenger,
                uiDispatcher, textClipboard, storageProvider, structureViewModelFactory,
                canCloseTab, startAddress, endAddress) {
        Title = "EMS Memory";
        pauseHandler.Paused += () => uiDispatcher.Post(() => UpdateEmsMemoryViewModel(this),
            DispatcherPriority.Background);
    }

    private static void UpdateEmsMemoryViewModel(MemoryViewModel emsMemoryViewModel) {
        // EMS Page Frame is located at segment 0xE000, which is 0xE0000 physical address
        uint emsPageFrameAddress = MemoryUtils.ToPhysicalAddress(ExpandedMemoryManager.EmmPageFrameSegment, 0);
        emsMemoryViewModel.StartAddress = ConvertUtils.ToHex32(emsPageFrameAddress);
        emsMemoryViewModel.EndAddress = ConvertUtils.ToHex32(emsPageFrameAddress + ExpandedMemoryManager.EmmPageFrameSize - 1);
    }
}
