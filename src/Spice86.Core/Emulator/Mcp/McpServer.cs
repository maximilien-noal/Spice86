namespace Spice86.Core.Emulator.Mcp;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Function;
using Spice86.Core.Emulator.Memory;
using Spice86.Shared.Interfaces;

using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

/// <summary>
/// In-process Model Context Protocol (MCP) server for inspecting emulator state.
/// This server exposes tools to query CPU registers, memory contents, and function definitions.
/// </summary>
public sealed class McpServer : IMcpServer {
    private readonly IMemory _memory;
    private readonly State _state;
    private readonly FunctionCatalogue _functionCatalogue;
    private readonly ILoggerService _loggerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="McpServer"/> class.
    /// </summary>
    /// <param name="memory">The memory bus to inspect.</param>
    /// <param name="state">The CPU state to inspect.</param>
    /// <param name="functionCatalogue">The function catalogue to query.</param>
    /// <param name="loggerService">The logger service for diagnostics.</param>
    public McpServer(IMemory memory, State state, FunctionCatalogue functionCatalogue, ILoggerService loggerService) {
        _memory = memory;
        _state = state;
        _functionCatalogue = functionCatalogue;
        _loggerService = loggerService;
    }

    /// <inheritdoc />
    public string HandleRequest(string requestJson) {
        try {
            JsonNode? requestNode = JsonNode.Parse(requestJson);
            if (requestNode == null) {
                return CreateErrorResponse(null, -32700, "Parse error: Invalid JSON");
            }

            string? method = requestNode["method"]?.GetValue<string>();
            JsonNode? id = requestNode["id"];

            if (string.IsNullOrEmpty(method)) {
                return CreateErrorResponse(id, -32600, "Invalid Request: Missing method");
            }

            return method switch {
                "initialize" => HandleInitialize(id),
                "tools/list" => HandleToolsList(id),
                "tools/call" => HandleToolCall(requestNode, id),
                _ => CreateErrorResponse(id, -32601, $"Method not found: {method}")
            };
        } catch (JsonException ex) {
            _loggerService.Error(ex, "JSON parsing error in MCP request");
            return CreateErrorResponse(null, -32700, "Parse error: " + ex.Message);
        } catch (Exception ex) {
            _loggerService.Error(ex, "Unexpected error handling MCP request");
            return CreateErrorResponse(null, -32603, "Internal error: " + ex.Message);
        }
    }

    /// <inheritdoc />
    public McpTool[] GetAvailableTools() {
        return new[] {
            new McpTool {
                Name = "read_cpu_registers",
                Description = "Read the current values of CPU registers (general purpose, segment, instruction pointer, and flags)",
                InputSchema = new {
                    type = "object",
                    properties = new { },
                    required = Array.Empty<string>()
                }
            },
            new McpTool {
                Name = "read_memory",
                Description = "Read a range of bytes from emulator memory",
                InputSchema = new {
                    type = "object",
                    properties = new {
                        address = new { type = "integer", description = "The starting memory address (linear address)" },
                        length = new { type = "integer", description = "The number of bytes to read (max 4096)" }
                    },
                    required = new[] { "address", "length" }
                }
            },
            new McpTool {
                Name = "list_functions",
                Description = "List all known functions in the function catalogue",
                InputSchema = new {
                    type = "object",
                    properties = new {
                        limit = new { type = "integer", description = "Maximum number of functions to return (default 100)" }
                    },
                    required = Array.Empty<string>()
                }
            }
        };
    }

    private string HandleInitialize(JsonNode? id) {
        var result = new {
            protocolVersion = "2025-06-18",
            serverInfo = new {
                name = "Spice86 MCP Server",
                version = "1.0.0"
            },
            capabilities = new {
                tools = new { }
            }
        };

        return CreateSuccessResponse(id, result);
    }

    private string HandleToolsList(JsonNode? id) {
        McpTool[] tools = GetAvailableTools();
        var toolsJson = tools.Select(t => new {
            name = t.Name,
            description = t.Description,
            inputSchema = t.InputSchema
        }).ToArray();

        var result = new {
            tools = toolsJson
        };

        return CreateSuccessResponse(id, result);
    }

    private string HandleToolCall(JsonNode requestNode, JsonNode? id) {
        JsonNode? paramsNode = requestNode["params"];
        if (paramsNode == null) {
            return CreateErrorResponse(id, -32602, "Invalid params: Missing params");
        }

        string? toolName = paramsNode["name"]?.GetValue<string>();
        if (string.IsNullOrEmpty(toolName)) {
            return CreateErrorResponse(id, -32602, "Invalid params: Missing tool name");
        }

        JsonNode? arguments = paramsNode["arguments"];

        try {
            object result = toolName switch {
                "read_cpu_registers" => ReadCpuRegisters(),
                "read_memory" => ReadMemory(arguments),
                "list_functions" => ListFunctions(arguments),
                _ => throw new InvalidOperationException($"Unknown tool: {toolName}")
            };

            return CreateToolCallResponse(id, result);
        } catch (Exception ex) {
            _loggerService.Error(ex, "Error executing tool {ToolName}", toolName);
            return CreateErrorResponse(id, -32603, $"Tool execution error: {ex.Message}");
        }
    }

    private object ReadCpuRegisters() {
        return new {
            generalPurpose = new {
                EAX = _state.EAX,
                EBX = _state.EBX,
                ECX = _state.ECX,
                EDX = _state.EDX,
                ESI = _state.ESI,
                EDI = _state.EDI,
                ESP = _state.ESP,
                EBP = _state.EBP
            },
            segments = new {
                CS = _state.CS,
                DS = _state.DS,
                ES = _state.ES,
                FS = _state.FS,
                GS = _state.GS,
                SS = _state.SS
            },
            instructionPointer = new {
                IP = _state.IP
            },
            flags = new {
                CarryFlag = _state.CarryFlag,
                ParityFlag = _state.ParityFlag,
                AuxiliaryFlag = _state.AuxiliaryFlag,
                ZeroFlag = _state.ZeroFlag,
                SignFlag = _state.SignFlag,
                DirectionFlag = _state.DirectionFlag,
                OverflowFlag = _state.OverflowFlag,
                InterruptFlag = _state.InterruptFlag
            }
        };
    }

    private object ReadMemory(JsonNode? arguments) {
        if (arguments == null) {
            throw new ArgumentException("Missing arguments for read_memory");
        }

        uint address = arguments["address"]?.GetValue<uint>() ?? throw new ArgumentException("Missing address parameter");
        int length = arguments["length"]?.GetValue<int>() ?? throw new ArgumentException("Missing length parameter");

        if (length <= 0 || length > 4096) {
            throw new ArgumentException("Length must be between 1 and 4096");
        }

        byte[] data = _memory.ReadRam((uint)length, address);
        
        return new {
            address = address,
            length = length,
            data = Convert.ToHexString(data)
        };
    }

    private object ListFunctions(JsonNode? arguments) {
        int limit = arguments?["limit"]?.GetValue<int>() ?? 100;
        
        var functions = _functionCatalogue.FunctionInformations.Values
            .OrderByDescending(f => f.CalledCount)
            .Take(limit)
            .Select(f => new {
                address = f.Address.ToString(),
                name = f.Name,
                calledCount = f.CalledCount,
                hasOverride = f.HasOverride
            })
            .ToArray();

        return new {
            functions = functions,
            totalCount = _functionCatalogue.FunctionInformations.Count
        };
    }

    private static string CreateSuccessResponse(JsonNode? id, object result) {
        var response = new {
            jsonrpc = "2.0",
            id = id?.GetValue<object>(),
            result = result
        };

        return JsonSerializer.Serialize(response);
    }

    private static string CreateToolCallResponse(JsonNode? id, object toolResult) {
        var response = new {
            jsonrpc = "2.0",
            id = id?.GetValue<object>(),
            result = new {
                content = new[] {
                    new {
                        type = "text",
                        text = JsonSerializer.Serialize(toolResult, new JsonSerializerOptions { WriteIndented = true })
                    }
                }
            }
        };

        return JsonSerializer.Serialize(response);
    }

    private static string CreateErrorResponse(JsonNode? id, int code, string message) {
        var response = new {
            jsonrpc = "2.0",
            id = id?.GetValue<object>(),
            error = new {
                code = code,
                message = message
            }
        };

        return JsonSerializer.Serialize(response);
    }
}
