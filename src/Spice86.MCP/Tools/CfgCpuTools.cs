namespace Spice86.MCP.Tools;

using ModelContextProtocol;
using ModelContextProtocol.Server;
using Spice86.Core.Emulator.CPU.CfgCpu;
using Spice86.Core.Emulator.CPU.CfgCpu.ControlFlowGraph;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Shared.Emulator.Memory;
using Spice86.Shared.Utils;
using System.ComponentModel;
using System.Text;

/// <summary>
/// MCP tools for exploring and analyzing the Control Flow Graph (CFG) CPU execution state.
/// </summary>
[McpServerToolType]
public sealed class CfgCpuTools {
    private readonly CfgCpu _cfgCpu;

    /// <summary>
    /// Initializes a new instance of the <see cref="CfgCpuTools"/> class.
    /// </summary>
    /// <param name="cfgCpu">The CFG CPU instance for graph exploration.</param>
    public CfgCpuTools(CfgCpu cfgCpu) {
        _cfgCpu = cfgCpu;
    }

    [McpServerTool]
    [Description("Get information about the current execution context in CFG CPU")]
    public Task<string> GetExecutionContext() {
        var result = new StringBuilder();
        var context = _cfgCpu.ExecutionContextManager.CurrentExecutionContext;
        
        result.AppendLine("Current Execution Context:");
        result.AppendLine($"Entry Point: {context.EntryPoint}");
        result.AppendLine($"Depth: {context.Depth}");
        result.AppendLine($"Is Initial Context: {_cfgCpu.IsInitialExecutionContext}");
        
        if (context.LastExecuted != null) {
            result.AppendLine($"Last Executed Node: {context.LastExecuted.Address}");
        }
        
        if (context.NodeToExecuteNextAccordingToGraph != null) {
            result.AppendLine($"Next Node (According to Graph): {context.NodeToExecuteNextAccordingToGraph.Address}");
        }
        
        return Task.FromResult(result.ToString());
    }

    [McpServerTool]
    [Description("List all entry points registered in the execution context manager")]
    public Task<string> ListEntryPoints() {
        var result = new StringBuilder();
        var entryPoints = _cfgCpu.ExecutionContextManager.ExecutionContextEntryPoints;
        
        result.AppendLine($"Total Entry Points: {entryPoints.Count}");
        result.AppendLine();
        
        foreach (var kvp in entryPoints.OrderBy(kv => kv.Key.Linear)) {
            result.AppendLine($"Address: {kvp.Key} ({kvp.Key.Linear:X})");
            result.AppendLine($"  Node Count: {kvp.Value.Count}");
            
            foreach (var node in kvp.Value) {
                if (node is CfgInstruction instruction) {
                    result.AppendLine($"    Instruction: {instruction.Address}");
                }
            }
            result.AppendLine();
        }
        
        return Task.FromResult(result.ToString());
    }

    [McpServerTool]
    [Description("Get information about a specific CFG node at an address")]
    public Task<string> GetCfgNodeInfo(
        [Description("The address to inspect (hex format, e.g., 0xF000:0x1000 or segmented)")] string address) {
        
        if (!TryParseSegmentedAddress(address, out SegmentedAddress segAddr)) {
            return Task.FromResult($"Error: Invalid address format '{address}'. Use format like 0xF000:0x1000 or physical address like 0xF1000");
        }

        var result = new StringBuilder();
        result.AppendLine($"CFG Node Information for {segAddr}:");
        result.AppendLine();
        
        // Check if this address is an entry point
        if (_cfgCpu.ExecutionContextManager.ExecutionContextEntryPoints.TryGetValue(segAddr, out var nodes)) {
            result.AppendLine($"This address is an entry point with {nodes.Count} node(s)");
            result.AppendLine();
        }
        
        // Try to get information from the current execution context
        var context = _cfgCpu.ExecutionContextManager.CurrentExecutionContext;
        if (context.LastExecuted?.Address.Equals(segAddr) == true) {
            result.AppendLine("This is the last executed node in the current context");
            result.AppendLine($"Node Type: {context.LastExecuted.GetType().Name}");
        }
        
        if (context.NodeToExecuteNextAccordingToGraph?.Address.Equals(segAddr) == true) {
            result.AppendLine("This is the next node to execute according to the graph");
        }
        
        return Task.FromResult(result.ToString());
    }

    [McpServerTool]
    [Description("Get statistics about the CFG execution")]
    public Task<string> GetCfgStatistics() {
        var result = new StringBuilder();
        result.AppendLine("CFG CPU Statistics:");
        result.AppendLine();
        
        var entryPoints = _cfgCpu.ExecutionContextManager.ExecutionContextEntryPoints;
        result.AppendLine($"Total Entry Points: {entryPoints.Count}");
        
        int totalNodes = entryPoints.Values.Sum(set => set.Count);
        result.AppendLine($"Total Nodes: {totalNodes}");
        
        var context = _cfgCpu.ExecutionContextManager.CurrentExecutionContext;
        result.AppendLine($"Current Context Depth: {context.Depth}");
        result.AppendLine($"Current Entry Point: {context.EntryPoint}");
        
        return Task.FromResult(result.ToString());
    }

    [McpServerTool]
    [Description("Explore the control flow graph starting from a specific address")]
    public Task<string> ExploreControlFlow(
        [Description("The starting address (hex format, e.g., 0xF000:0x1000)")] string address,
        [Description("Maximum depth to explore (default: 5)")] int maxDepth = 5) {
        
        if (!TryParseSegmentedAddress(address, out SegmentedAddress segAddr)) {
            return Task.FromResult($"Error: Invalid address format '{address}'");
        }

        if (maxDepth <= 0 || maxDepth > 20) {
            return Task.FromResult("Error: Max depth must be between 1 and 20");
        }

        var result = new StringBuilder();
        result.AppendLine($"Control Flow Exploration from {segAddr} (max depth: {maxDepth}):");
        result.AppendLine();
        
        // Check if this address is in the entry points
        if (_cfgCpu.ExecutionContextManager.ExecutionContextEntryPoints.TryGetValue(segAddr, out var nodes)) {
            result.AppendLine($"Found {nodes.Count} node(s) at entry point:");
            foreach (var node in nodes) {
                result.AppendLine($"  Node Type: {node.GetType().Name}");
                result.AppendLine($"  Address: {node.Address}");
                if (node is CfgInstruction instruction) {
                    result.AppendLine($"  Can Cause Context Restore: {instruction.CanCauseContextRestore}");
                }
            }
        } else {
            result.AppendLine("This address is not registered as an entry point in the current execution state.");
            result.AppendLine("Note: CFG nodes are created dynamically during execution.");
        }
        
        return Task.FromResult(result.ToString());
    }

    private static bool TryParseSegmentedAddress(string addressStr, out SegmentedAddress address) {
        address = SegmentedAddress.ZERO;
        
        if (string.IsNullOrWhiteSpace(addressStr)) {
            return false;
        }

        // Try segmented address first
        SegmentedAddress? parsed = AddressParser.ParseSegmentedAddress(addressStr);
        if (parsed != null) {
            address = parsed.Value;
            return true;
        }

        // Try linear/physical address
        uint? physAddr = AddressParser.ParseHex(addressStr);
        if (physAddr != null) {
            // Convert physical to segmented (simplified - segment * 16 + offset)
            ushort segment = (ushort)(physAddr.Value >> 4);
            ushort offset = (ushort)(physAddr.Value & 0xF);
            address = new SegmentedAddress(segment, offset);
            return true;
        }

        return false;
    }
}
