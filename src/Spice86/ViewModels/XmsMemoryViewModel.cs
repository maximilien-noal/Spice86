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
/// Note: XMS memory is stored in ExtendedMemoryManager.XmsRam which is a separate RAM
/// not part of main memory. This view currently shows the HMA (High Memory Area) which
/// is the only part of XMS accessible in real mode. To view the full XMS RAM, a wrapper
/// implementation would be needed to present IMemoryDevice as IMemory.
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
            base(memory, memoryDataExporter, state, breakpointsViewModel, pauseHandler, messenger,
                uiDispatcher, textClipboard, storageProvider, structureViewModelFactory,
                canCloseTab, startAddress, endAddress) {
        _xmsManager = xmsManager;
        Title = "XMS Memory (HMA in Main Memory)";
        if (xmsManager != null) {
            pauseHandler.Paused += () => uiDispatcher.Post(() => UpdateXmsMemoryViewModel(this, memory, xmsManager),
                DispatcherPriority.Background);
        }
    }

    private static void UpdateXmsMemoryViewModel(MemoryViewModel xmsMemoryViewModel, IMemory memory, ExtendedMemoryManager xmsManager) {
        // Show the HMA (High Memory Area) which is in main memory
        // Note: The bulk of XMS memory (ExtendedMemoryManager.XmsRam) is separate and not shown here
        xmsMemoryViewModel.StartAddress = ConvertUtils.ToHex32(A20Gate.StartOfHighMemoryArea);
        xmsMemoryViewModel.EndAddress = ConvertUtils.ToHex32(Math.Min((uint)memory.Length - 1, A20Gate.EndOfHighMemoryArea));
    }

    /// <summary>
    /// Override to allow addresses in the HMA range
    /// </summary>
    protected override void ValidateMemoryAddressIsWithinLimit(State state, string? value,
        uint limit = A20Gate.EndOfHighMemoryArea,
        [System.Runtime.CompilerServices.CallerMemberName] string? bindedPropertyName = null) {
        // Allow addresses up to the end of HMA
        base.ValidateMemoryAddressIsWithinLimit(state, value, A20Gate.EndOfHighMemoryArea, bindedPropertyName);
    }
}
