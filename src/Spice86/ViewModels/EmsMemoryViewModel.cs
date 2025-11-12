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
/// Uses the separate EMS pages that are not part of main memory.
/// All allocated EMS pages are shown as a contiguous memory space.
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
            base(emsManager != null ? new EmsMemory(emsManager) : memory,
                memoryDataExporter, state, breakpointsViewModel, pauseHandler, messenger,
                uiDispatcher, textClipboard, storageProvider, structureViewModelFactory,
                canCloseTab, startAddress, endAddress) {
        _emsManager = emsManager;
        Title = "EMS Memory";
        if (emsManager != null) {
            pauseHandler.Paused += () => uiDispatcher.Post(() => UpdateEmsMemoryViewModel(this, emsManager),
                DispatcherPriority.Background);
        }
    }

    private static void UpdateEmsMemoryViewModel(MemoryViewModel emsMemoryViewModel, ExpandedMemoryManager emsManager) {
        // Calculate total allocated EMS memory across all handles
        int totalPages = 0;
        foreach (var handle in emsManager.EmmHandles.Values) {
            totalPages += handle.LogicalPages.Count;
        }
        uint totalSize = (uint)(totalPages * ExpandedMemoryManager.EmmPageSize);
        
        // EMS memory view starts at 0 (separate from main memory)
        emsMemoryViewModel.StartAddress = ConvertUtils.ToHex32(0);
        emsMemoryViewModel.EndAddress = ConvertUtils.ToHex32(totalSize > 0 ? totalSize - 1 : 0);
    }

    /// <summary>
    /// Override to allow addresses up to EMS memory size
    /// </summary>
    protected override void ValidateMemoryAddressIsWithinLimit(State state, string? value,
        uint limit = A20Gate.EndOfHighMemoryArea,
        [System.Runtime.CompilerServices.CallerMemberName] string? bindedPropertyName = null) {
        // For EMS memory, calculate the limit based on allocated pages
        if (_emsManager != null) {
            int totalPages = 0;
            foreach (var handle in _emsManager.EmmHandles.Values) {
                totalPages += handle.LogicalPages.Count;
            }
            uint emsLimit = (uint)(totalPages * ExpandedMemoryManager.EmmPageSize);
            base.ValidateMemoryAddressIsWithinLimit(state, value, emsLimit, bindedPropertyName);
        } else {
            base.ValidateMemoryAddressIsWithinLimit(state, value, limit, bindedPropertyName);
        }
    }
}
