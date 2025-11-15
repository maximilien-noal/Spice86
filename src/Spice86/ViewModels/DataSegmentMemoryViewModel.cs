using Avalonia.Threading;

using CommunityToolkit.Mvvm.Messaging;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Function.Dump;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.VM;
using Spice86.Shared.Utils;
using Spice86.ViewModels.Services;

namespace Spice86.ViewModels;

/// <summary>
/// Represents data segment memory view model.
/// </summary>
public partial class DataSegmentMemoryViewModel : MemoryViewModel {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="memory">The memory.</param>
    /// <param name="memoryDataExporter">The memory data exporter.</param>
    /// <param name="state">The state.</param>
    /// <param name="breakpointsViewModel">The breakpoints view model.</param>
    /// <param name="pauseHandler">The pause handler.</param>
    /// <param name="messenger">The messenger.</param>
    /// <param name="uiDispatcher">The ui dispatcher.</param>
    /// <param name="textClipboard">The text clipboard.</param>
    /// <param name="storageProvider">The storage provider.</param>
    /// <param name="structureViewModelFactory">The structure view model factory.</param>
    /// <param name="canCloseTab">The can close tab.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="endAddress">The end address.</param>
    public DataSegmentMemoryViewModel(IMemory memory, MemoryDataExporter memoryDataExporter, State state,
        BreakpointsViewModel breakpointsViewModel, IPauseHandler pauseHandler,
        IMessenger messenger, IUIDispatcher uiDispatcher, ITextClipboard textClipboard,
        IHostStorageProvider storageProvider, IStructureViewModelFactory structureViewModelFactory,
        bool canCloseTab = false, string? startAddress = null,
        string? endAddress = null) :
            base(memory, memoryDataExporter, state, breakpointsViewModel, pauseHandler, messenger,
                uiDispatcher, textClipboard, storageProvider, structureViewModelFactory,
                canCloseTab, startAddress, endAddress) {
        Title = "Data Segment";
        pauseHandler.Paused += () => uiDispatcher.Post(() => UpdateDataSegmentMemoryViewModel(this, state),
            DispatcherPriority.Background);
    }
    private static void UpdateDataSegmentMemoryViewModel(MemoryViewModel instance, State state) {
        instance.StartAddress = ConvertUtils.ToHex32(MemoryUtils.ToPhysicalAddress(state.DS, 0));
        instance.EndAddress = ConvertUtils.ToHex32(MemoryUtils.ToPhysicalAddress(state.DS, ushort.MaxValue));
    }
}