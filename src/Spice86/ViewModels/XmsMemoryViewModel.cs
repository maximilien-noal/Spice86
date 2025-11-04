namespace Spice86.ViewModels;

using Avalonia.Threading;

using CommunityToolkit.Mvvm.Messaging;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Function.Dump;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.VM;
using Spice86.Shared.Emulator.Memory;
using Spice86.Shared.Utils;
using Spice86.ViewModels.Services;

/// <summary>
/// ViewModel for viewing XMS (Extended Memory Specification) memory.
/// XMS memory starts at 1MB+64KB and extends beyond conventional memory.
/// </summary>
public partial class XmsMemoryViewModel : MemoryViewModel {
    public XmsMemoryViewModel(IMemory memory, MemoryDataExporter memoryDataExporter, State state,
        BreakpointsViewModel breakpointsViewModel, IPauseHandler pauseHandler,
        IMessenger messenger, IUIDispatcher uiDispatcher, ITextClipboard textClipboard,
        IHostStorageProvider storageProvider, IStructureViewModelFactory structureViewModelFactory,
        bool canCloseTab = false, string? startAddress = null,
        string? endAddress = null) :
            base(memory, memoryDataExporter, state, breakpointsViewModel, pauseHandler, messenger,
                uiDispatcher, textClipboard, storageProvider, structureViewModelFactory,
                canCloseTab, startAddress, endAddress) {
        Title = "XMS Memory";
        pauseHandler.Paused += () => uiDispatcher.Post(() => UpdateXmsMemoryViewModel(this, memory),
            DispatcherPriority.Background);
    }

    private static void UpdateXmsMemoryViewModel(MemoryViewModel xmsMemoryViewModel, IMemory memory) {
        // XMS memory starts at 1MB + 64KB (High Memory Area)
        xmsMemoryViewModel.StartAddress = ConvertUtils.ToHex32(A20Gate.StartOfHighMemoryArea);
        xmsMemoryViewModel.EndAddress = ConvertUtils.ToHex32((uint)memory.Length - 1);
    }
}
