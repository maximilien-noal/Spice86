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
/// ViewModel for visualizing the PSP (Program Segment Prefix) chain as a graph.
/// Shows parent-child relationships between processes and detailed PSP information.
/// </summary>
public partial class PspGraphViewModel : ViewModelBase {
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
    private AvaloniaList<PspNodeInfo> _pspNodes = new();

    [ObservableProperty]
    private PspNodeInfo? _selectedNode;

    /// <summary>
    /// Initializes a new instance of the <see cref="PspGraphViewModel"/> class.
    /// </summary>
    public PspGraphViewModel(Dos dos, IMemory memory, IPauseHandler pauseHandler, IUIDispatcher uiDispatcher) {
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
        Dictionary<ushort, PspNodeInfo> pspMap = new();
        List<PspNodeInfo> nodes = new();
        HashSet<ushort> visitedSegments = new();

        // Start from the current PSP and walk up to find all PSPs
        ushort currentPspSegment = _dos.DosSwappableDataArea.CurrentProgramSegmentPrefix;

        // First, collect all PSPs by walking up the parent chain
        Queue<ushort> toVisit = new();
        if (currentPspSegment != 0) {
            toVisit.Enqueue(currentPspSegment);
        }

        while (toVisit.Count > 0 && visitedSegments.Count < 64) {
            ushort pspSegment = toVisit.Dequeue();

            if (pspSegment == 0 || visitedSegments.Contains(pspSegment)) {
                continue;
            }

            visitedSegments.Add(pspSegment);

            uint linearAddress = MemoryUtils.ToPhysicalAddress(pspSegment, 0);
            DosProgramSegmentPrefix psp = new(_memory, linearAddress);

            PspNodeInfo nodeInfo = new() {
                Segment = pspSegment,
                SegmentHex = $"0x{pspSegment:X4}",
                LinearAddress = linearAddress,
                LinearAddressHex = $"0x{linearAddress:X6}",
                ParentSegment = psp.ParentProgramSegmentPrefix,
                ParentSegmentHex = $"0x{psp.ParentProgramSegmentPrefix:X4}",
                EnvironmentSegment = psp.EnvironmentTableSegment,
                EnvironmentSegmentHex = $"0x{psp.EnvironmentTableSegment:X4}",
                CommandLine = psp.DosCommandTail.Command,
                NextSegment = psp.NextSegment,
                NextSegmentHex = $"0x{psp.NextSegment:X4}",
                IsCurrent = pspSegment == currentPspSegment,
                IsRoot = psp.ParentProgramSegmentPrefix == pspSegment || psp.ParentProgramSegmentPrefix == 0
            };

            nodes.Add(nodeInfo);
            pspMap[pspSegment] = nodeInfo;

            // Queue parent for visiting
            if (psp.ParentProgramSegmentPrefix != 0 &&
                psp.ParentProgramSegmentPrefix != pspSegment &&
                !visitedSegments.Contains(psp.ParentProgramSegmentPrefix)) {
                toVisit.Enqueue(psp.ParentProgramSegmentPrefix);
            }
        }

        // Build the graph edges (parent -> child relationships)
        foreach (PspNodeInfo node in nodes) {
            if (!node.IsRoot && pspMap.TryGetValue(node.ParentSegment, out PspNodeInfo? parentNode)) {
                string parentLabel = FormatNodeLabel(parentNode);
                string childLabel = FormatNodeLabel(node);
                currentGraph.Edges.Add(new Edge(parentLabel, childLabel, "spawned"));
            }
        }

        // If only one node with no edges, add it as a standalone node
        if (nodes.Count == 1 && currentGraph.Edges.Count == 0) {
            string label = FormatNodeLabel(nodes[0]);
            // Add a self-referencing edge just to show the node
            currentGraph.Edges.Add(new Edge(label, label, "root"));
        }

        Graph = currentGraph;
        NodeCount = nodes.Count;
        StatusMessage = $"PSP chain: {nodes.Count} process(es)";

        PspNodes.Clear();
        foreach (PspNodeInfo node in nodes) {
            PspNodes.Add(node);
        }
    }

    private static string FormatNodeLabel(PspNodeInfo node) {
        string status = node.IsCurrent ? "▶ CURRENT" : (node.IsRoot ? "ROOT" : "");
        string cmd = string.IsNullOrWhiteSpace(node.CommandLine) ? "(no command)" : node.CommandLine;
        return $"{status}\nPSP: {node.SegmentHex}\n{cmd}";
    }

    /// <summary>
    /// Detailed information about a PSP node for display in the graph.
    /// </summary>
    public record PspNodeInfo {
        /// <summary>PSP segment address.</summary>
        public ushort Segment { get; init; }

        /// <summary>PSP segment address in hex.</summary>
        public string SegmentHex { get; init; } = string.Empty;

        /// <summary>Linear address of the PSP.</summary>
        public uint LinearAddress { get; init; }

        /// <summary>Linear address in hex.</summary>
        public string LinearAddressHex { get; init; } = string.Empty;

        /// <summary>Parent PSP segment.</summary>
        public ushort ParentSegment { get; init; }

        /// <summary>Parent PSP segment in hex.</summary>
        public string ParentSegmentHex { get; init; } = string.Empty;

        /// <summary>Environment table segment.</summary>
        public ushort EnvironmentSegment { get; init; }

        /// <summary>Environment segment in hex.</summary>
        public string EnvironmentSegmentHex { get; init; } = string.Empty;

        /// <summary>Command line from the PSP.</summary>
        public string CommandLine { get; init; } = string.Empty;

        /// <summary>Next segment (top of memory allocated to program).</summary>
        public ushort NextSegment { get; init; }

        /// <summary>Next segment in hex.</summary>
        public string NextSegmentHex { get; init; } = string.Empty;

        /// <summary>Whether this is the currently executing PSP.</summary>
        public bool IsCurrent { get; init; }

        /// <summary>Whether this is a root process (parent == self or parent == 0).</summary>
        public bool IsRoot { get; init; }
    }
}
