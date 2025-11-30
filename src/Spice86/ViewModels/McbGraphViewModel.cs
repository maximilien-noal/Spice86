namespace Spice86.ViewModels;

using Avalonia.Collections;

using AvaloniaGraphControl;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.OperatingSystem;
using Spice86.Core.Emulator.OperatingSystem.Structures;
using Spice86.Core.Emulator.VM;
using Spice86.Shared.Utils;
using Spice86.ViewModels.Services;

/// <summary>
/// ViewModel for visualizing the MCB (Memory Control Block) chain as a graph.
/// Each MCB is displayed as an information card with detailed memory allocation information.
/// </summary>
public partial class McbGraphViewModel : ViewModelBase {
    private readonly Dos _dos;
    private readonly IMemory _memory;
    private readonly IUIDispatcher _uiDispatcher;

    [ObservableProperty]
    private Graph? _graph;

    [ObservableProperty]
    private int _nodeCount;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _autoFollow = true;

    [ObservableProperty]
    private AvaloniaList<McbNodeInfo> _mcbNodes = new();

    [ObservableProperty]
    private McbNodeInfo? _selectedNode;

    /// <summary>
    /// Initializes a new instance of the <see cref="McbGraphViewModel"/> class.
    /// </summary>
    public McbGraphViewModel(Dos dos, IMemory memory, IPauseHandler pauseHandler, IUIDispatcher uiDispatcher) {
        _dos = dos;
        _memory = memory;
        _uiDispatcher = uiDispatcher;

        pauseHandler.Paused += () => {
            if (AutoFollow) {
                _uiDispatcher.Post(() => {
                    UpdateGraphCommand.Execute(null);
                });
            }
        };
    }

    [RelayCommand]
    private void UpdateGraph() {
        Graph = null;
        RegenerateGraph();
    }

    private void RegenerateGraph() {
        Graph currentGraph = new();
        List<McbNodeInfo> nodes = new();
        ushort mcbSegment = _dos.DosSysVars.FirstMCB;
        int count = 0;
        McbNodeInfo? previousNode = null;

        // If first MCB segment is 0 or invalid, show a message
        if (mcbSegment == 0) {
            StatusMessage = "MCB chain not initialized (FirstMCB = 0)";
            Graph = currentGraph;
            NodeCount = 0;
            McbNodes.Clear();
            return;
        }

        DosMemoryControlBlock firstMcb = new(_memory, MemoryUtils.ToPhysicalAddress(mcbSegment, 0));
        if (!firstMcb.IsValid) {
            StatusMessage = $"First MCB at {mcbSegment:X4} is invalid (TypeField = 0x{firstMcb.TypeField:X2}, expected 'M'=0x4D or 'Z'=0x5A)";
            Graph = currentGraph;
            NodeCount = 0;
            McbNodes.Clear();
            return;
        }

        while (mcbSegment != 0 && count < 256) {
            DosMemoryControlBlock mcb = new(_memory, MemoryUtils.ToPhysicalAddress(mcbSegment, 0));

            if (!mcb.IsValid) {
                break;
            }

            int sizeBytes = mcb.Size * 16;
            uint linearAddress = MemoryUtils.ToPhysicalAddress(mcbSegment, 0);

            McbNodeInfo nodeInfo = new() {
                Segment = mcbSegment,
                SegmentHex = $"0x{mcbSegment:X4}",
                DataSegment = mcb.DataBlockSegment,
                DataSegmentHex = $"0x{mcb.DataBlockSegment:X4}",
                LinearAddress = linearAddress,
                LinearAddressHex = $"0x{linearAddress:X6}",
                Size = sizeBytes,
                SizeFormatted = $"{sizeBytes:N0} bytes",
                SizeParagraphs = mcb.Size,
                PspSegment = mcb.PspSegment,
                PspSegmentHex = $"0x{mcb.PspSegment:X4}",
                Owner = mcb.Owner,
                IsFree = mcb.IsFree,
                IsLast = mcb.IsLast,
                TypeField = mcb.TypeField,
                TypeDescription = mcb.IsFree ? "FREE" : (mcb.IsLast ? "LAST (Z)" : "ALLOCATED (M)")
            };

            nodes.Add(nodeInfo);

            string nodeLabel = FormatNodeLabel(nodeInfo);

            if (previousNode != null) {
                string prevLabel = FormatNodeLabel(previousNode);
                currentGraph.Edges.Add(new Edge(prevLabel, nodeLabel, "next"));
            }

            previousNode = nodeInfo;
            count++;

            if (mcb.IsLast) {
                break;
            }

            mcbSegment = (ushort)(mcbSegment + mcb.Size + 1);
        }

        Graph = currentGraph;
        NodeCount = count;
        StatusMessage = $"MCB chain: {count} blocks";

        McbNodes.Clear();
        foreach (McbNodeInfo node in nodes) {
            McbNodes.Add(node);
        }
    }

    private static string FormatNodeLabel(McbNodeInfo node) {
        string status = node.IsFree ? "FREE" : "USED";
        return $"{status} @ {node.SegmentHex}\n{node.SizeFormatted}\nOwner: {node.Owner}";
    }

    /// <summary>
    /// Detailed information about an MCB node for display in the graph.
    /// </summary>
    public record McbNodeInfo {
        /// <summary>MCB segment address.</summary>
        public ushort Segment { get; init; }

        /// <summary>MCB segment address in hex.</summary>
        public string SegmentHex { get; init; } = string.Empty;

        /// <summary>Data block segment (MCB segment + 1).</summary>
        public ushort DataSegment { get; init; }

        /// <summary>Data block segment in hex.</summary>
        public string DataSegmentHex { get; init; } = string.Empty;

        /// <summary>Linear address of the MCB.</summary>
        public uint LinearAddress { get; init; }

        /// <summary>Linear address in hex.</summary>
        public string LinearAddressHex { get; init; } = string.Empty;

        /// <summary>Size in bytes.</summary>
        public int Size { get; init; }

        /// <summary>Formatted size string.</summary>
        public string SizeFormatted { get; init; } = string.Empty;

        /// <summary>Size in paragraphs (16-byte units).</summary>
        public ushort SizeParagraphs { get; init; }

        /// <summary>PSP segment of the owning process.</summary>
        public ushort PspSegment { get; init; }

        /// <summary>PSP segment in hex.</summary>
        public string PspSegmentHex { get; init; } = string.Empty;

        /// <summary>Owner name (8 characters from MCB).</summary>
        public string Owner { get; init; } = string.Empty;

        /// <summary>Whether this block is free.</summary>
        public bool IsFree { get; init; }

        /// <summary>Whether this is the last block in the chain.</summary>
        public bool IsLast { get; init; }

        /// <summary>Raw type field byte ('M' or 'Z').</summary>
        public byte TypeField { get; init; }

        /// <summary>Human-readable type description.</summary>
        public string TypeDescription { get; init; } = string.Empty;
    }
}
