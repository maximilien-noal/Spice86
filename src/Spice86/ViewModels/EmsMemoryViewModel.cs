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
/// Note: EMS memory is stored in separate EmmPage objects (each with its own Ram)
/// that are not part of main memory. This view shows the EMS Page Frame in main memory
/// at segment 0xE000, which is a 64KB window that maps to the separate EMS pages.
/// To view the actual EMS pages directly, a wrapper implementation would be needed.
/// </summary>
public partial class EmsMemoryViewModel : MemoryViewModel {
    private readonly ExpandedMemoryManager? _emsManager;

    public EmsMemoryViewModel(IMemory memory, MemoryDataExporter memoryDataExporter, State state,
        BreakpointsViewModel breakpointsViewModel, IPauseHandler pauseHandler,
        IMessenger messenger, IUIDispatcher uiDispatcher, ITextClipboard textClipboard,
        IHostStorageProvider storageProvider, IStructureViewModelFactory structureViewModelFactory,
        ExpandedMemoryManager? emsManager,
        bool canCloseTab = false, string? startAddress = null,
        string? endAddress = null) :
            base(memory, memoryDataExporter, state, breakpointsViewModel, pauseHandler, messenger,
                uiDispatcher, textClipboard, storageProvider, structureViewModelFactory,
                canCloseTab, startAddress, endAddress) {
        _emsManager = emsManager;
        Title = "EMS Memory (Page Frame in Main Memory)";
        if (emsManager != null) {
            pauseHandler.Paused += () => uiDispatcher.Post(() => UpdateEmsMemoryViewModel(this),
                DispatcherPriority.Background);
        }
    }

    private static void UpdateEmsMemoryViewModel(MemoryViewModel emsMemoryViewModel) {
        // Show the EMS Page Frame at segment 0xE000 in main memory
        // This is a 64KB window that provides access to mapped EMS pages
        // Note: The actual EMS pages (EmmHandle.LogicalPages) are separate and not shown directly
        uint emsPageFrameAddress = MemoryUtils.ToPhysicalAddress(ExpandedMemoryManager.EmmPageFrameSegment, 0);
        emsMemoryViewModel.StartAddress = ConvertUtils.ToHex32(emsPageFrameAddress);
        emsMemoryViewModel.EndAddress = ConvertUtils.ToHex32(emsPageFrameAddress + ExpandedMemoryManager.EmmPageFrameSize - 1);
    }

    /// <summary>
    /// Override to allow addresses in the EMS page frame range
    /// </summary>
    protected override void ValidateMemoryAddressIsWithinLimit(State state, string? value,
        uint limit = A20Gate.EndOfHighMemoryArea,
        [System.Runtime.CompilerServices.CallerMemberName] string? bindedPropertyName = null) {
        // EMS page frame is in main memory, so use normal limit
        base.ValidateMemoryAddressIsWithinLimit(state, value, A20Gate.EndOfHighMemoryArea, bindedPropertyName);
    }
}
