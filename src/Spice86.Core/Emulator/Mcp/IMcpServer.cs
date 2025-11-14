namespace Spice86.Core.Emulator.Mcp;

/// <summary>
/// Interface for in-process Model Context Protocol (MCP) server.
/// MCP is a standardized protocol that enables AI models and applications to interact with emulator state and tools.
/// </summary>
public interface IMcpServer {
    /// <summary>
    /// Handles an MCP JSON-RPC request and returns a JSON-RPC response.
    /// </summary>
    /// <param name="requestJson">The JSON-RPC request as a string.</param>
    /// <returns>The JSON-RPC response as a string.</returns>
    string HandleRequest(string requestJson);

    /// <summary>
    /// Gets the list of available tools that this MCP server exposes.
    /// </summary>
    /// <returns>Array of tool descriptions.</returns>
    McpTool[] GetAvailableTools();
}
