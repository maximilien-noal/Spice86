namespace Spice86.ViewModels;

using Avalonia.Threading;

using CommunityToolkit.Mvvm.Messaging;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Function.Dump;
using Spice86.Core.Emulator.InterruptHandlers.Dos.Xms;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.VM;
using Spice86.Shared.Emulator.Memory;
using Spice86.Shared.Utils;
using Spice86.ViewModels.Services;

/// <summary>
/// ViewModel for viewing XMS (Extended Memory Specification) memory.
/// Uses the separate XMS RAM that is managed by the XMS driver, not main memory.
/// </summary>
public partial class XmsMemoryViewModel : MemoryViewModel {
    private readonly ExtendedMemoryManager? _xmsManager;

    public XmsMemoryViewModel(IMemory memory, MemoryDataExporter memoryDataExporter, State state,
        BreakpointsViewModel breakpointsViewModel, IPauseHandler pauseHandler,
        IMessenger messenger, IUIDispatcher uiDispatcher, ITextClipboard textClipboard,
        IHostStorageProvider storageProvider, IStructureViewModelFactory structureViewModelFactory,
        ExtendedMemoryManager? xmsManager,
        bool canCloseTab = false, string? startAddress = null,
        string? endAddress = null) :
            base(xmsManager != null ? new XmsMemory(xmsManager) : memory,
                memoryDataExporter, state, breakpointsViewModel, pauseHandler, messenger,
                uiDispatcher, textClipboard, storageProvider, structureViewModelFactory,
                canCloseTab, startAddress, endAddress) {
        _xmsManager = xmsManager;
        Title = "XMS Memory";
        if (xmsManager != null) {
            pauseHandler.Paused += () => uiDispatcher.Post(() => UpdateXmsMemoryViewModel(this, xmsManager),
                DispatcherPriority.Background);
        }
    }

    private static void UpdateXmsMemoryViewModel(MemoryViewModel xmsMemoryViewModel, ExtendedMemoryManager xmsManager) {
        // XMS memory view starts at 0 in the XmsRam (separate from main memory)
        xmsMemoryViewModel.StartAddress = ConvertUtils.ToHex32(0);
        xmsMemoryViewModel.EndAddress = ConvertUtils.ToHex32(xmsManager.XmsRam.Size - 1);
    }

    /// <summary>
    /// Override to allow addresses up to XMS memory size
    /// </summary>
    protected override void ValidateMemoryAddressIsWithinLimit(State state, string? value,
        uint limit = A20Gate.EndOfHighMemoryArea,
        [System.Runtime.CompilerServices.CallerMemberName] string? bindedPropertyName = null) {
        // For XMS memory, use the XMS RAM size as the limit
        uint xmsLimit = _xmsManager?.XmsRam.Size ?? A20Gate.EndOfHighMemoryArea;
        base.ValidateMemoryAddressIsWithinLimit(state, value, xmsLimit, bindedPropertyName);
    }
}
