namespace Spice86.ViewModels;

using Avalonia.Collections;

using CommunityToolkit.Mvvm.ComponentModel;

using Spice86.Core.Emulator.Mcp;
using Spice86.Core.Emulator.VM;
using Spice86.ViewModels.Services;

/// <summary>
/// ViewModel for observing MCP (Model Context Protocol) Server state in the debugger.
/// Displays information about available tools, server status, and configuration.
/// </summary>
public partial class McpServerViewModel : DebuggerTabViewModel {
    private readonly IMcpServer? _mcpServer;

    /// <summary>
    /// MCP protocol version string.
    /// </summary>
    private const string McpProtocolVersionString = "2025-06-18";

    /// <inheritdoc />
    public override string Header => "MCP Server";

    /// <inheritdoc />
    public override string? IconKey => "Plug";

    // Server Status
    [ObservableProperty]
    private bool _isServerRunning;

    [ObservableProperty]
    private string _serverStatus = string.Empty;

    [ObservableProperty]
    private string _protocolVersion = McpProtocolVersionString;

    [ObservableProperty]
    private string _serverName = "Spice86 MCP Server";

    [ObservableProperty]
    private string _serverVersion = "1.0.0";

    // Available Tools
    [ObservableProperty]
    private AvaloniaList<McpToolInfo> _availableTools = new();

    [ObservableProperty]
    private int _toolCount;

    // CFG CPU Status (affects available tools)
    [ObservableProperty]
    private bool _isCfgCpuEnabled;

    [ObservableProperty]
    private string _cfgCpuStatus = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="McpServerViewModel"/> class.
    /// </summary>
    /// <param name="mcpServer">The MCP server instance to observe. Can be null if MCP is disabled.</param>
    /// <param name="isMcpEnabled">Whether MCP server is enabled in configuration.</param>
    /// <param name="isCfgCpuEnabled">Whether CFG CPU is enabled (affects available tools).</param>
    /// <param name="pauseHandler">The pause handler for tracking emulator pause state.</param>
    /// <param name="uiDispatcher">The UI dispatcher for thread-safe UI updates.</param>
    public McpServerViewModel(
        IMcpServer? mcpServer,
        bool isMcpEnabled,
        bool isCfgCpuEnabled,
        IPauseHandler pauseHandler,
        IUIDispatcher uiDispatcher)
        : base(pauseHandler, uiDispatcher) {
        _mcpServer = mcpServer;
        IsEnabled = mcpServer != null;
        IsServerRunning = isMcpEnabled;
        IsCfgCpuEnabled = isCfgCpuEnabled;

        ServerStatus = isMcpEnabled ? "Running (stdio transport)" : "Disabled";
        CfgCpuStatus = isCfgCpuEnabled ? "Enabled (read_cfg_cpu_graph available)" : "Disabled";

        // Populate tools immediately since they don't change at runtime
        PopulateAvailableTools();
    }

    /// <inheritdoc />
    public override void UpdateValues(object? sender, EventArgs e) {
        if (!IsVisible) {
            return;
        }

        // MCP server state is mostly static after initialization
        // Only update status indicators
        UpdateServerStatus();
    }

    private void UpdateServerStatus() {
        // Server status is determined at startup and doesn't change
        // This could be extended to show request statistics if needed
    }

    private void PopulateAvailableTools() {
        if (_mcpServer == null) {
            return;
        }

        ModelContextProtocol.Protocol.Tool[] tools = _mcpServer.GetAvailableTools();
        ToolCount = tools.Length;

        AvailableTools.Clear();
        foreach (ModelContextProtocol.Protocol.Tool tool in tools) {
            AvailableTools.Add(new McpToolInfo {
                Name = tool.Name ?? "Unknown",
                Description = tool.Description ?? "No description",
                HasInputSchema = tool.InputSchema.ValueKind != System.Text.Json.JsonValueKind.Undefined
            });
        }
    }

    /// <summary>
    /// Information about an MCP tool.
    /// </summary>
    public class McpToolInfo {
        /// <summary>
        /// Gets or sets the tool name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tool description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this tool has an input schema.
        /// </summary>
        public bool HasInputSchema { get; set; }
    }
}
