namespace Spice86.Core.Emulator.Mcp;

/// <summary>
/// Represents a tool exposed by the MCP server.
/// </summary>
public sealed record McpTool {
    /// <summary>
    /// Gets the unique name of the tool.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets a human-readable description of what the tool does.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the JSON schema for the tool's input parameters.
    /// </summary>
    public required object InputSchema { get; init; }
}
