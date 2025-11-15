namespace Spice86.Core.Emulator.Mcp;

/// <summary>
/// Response for CPU registers query.
/// </summary>
public sealed record CpuRegistersResponse {
    /// <summary>
    /// Gets the general purpose registers.
    /// </summary>
    public required GeneralPurposeRegisters GeneralPurpose { get; init; }
    
    /// <summary>
    /// Gets the segment registers.
    /// </summary>
    public required SegmentRegisters Segments { get; init; }
    
    /// <summary>
    /// Gets the instruction pointer.
    /// </summary>
    public required InstructionPointer InstructionPointer { get; init; }
    
    /// <summary>
    /// Gets the CPU flags.
    /// </summary>
    public required CpuFlags Flags { get; init; }
}

/// <summary>
/// General purpose registers state.
/// </summary>
public sealed record GeneralPurposeRegisters {
    public required uint EAX { get; init; }
    public required uint EBX { get; init; }
    public required uint ECX { get; init; }
    public required uint EDX { get; init; }
    public required uint ESI { get; init; }
    public required uint EDI { get; init; }
    public required uint ESP { get; init; }
    public required uint EBP { get; init; }
}

/// <summary>
/// Segment registers state.
/// </summary>
public sealed record SegmentRegisters {
    public required ushort CS { get; init; }
    public required ushort DS { get; init; }
    public required ushort ES { get; init; }
    public required ushort FS { get; init; }
    public required ushort GS { get; init; }
    public required ushort SS { get; init; }
}

/// <summary>
/// Instruction pointer state.
/// </summary>
public sealed record InstructionPointer {
    public required ushort IP { get; init; }
}

/// <summary>
/// CPU flags state.
/// </summary>
public sealed record CpuFlags {
    public required bool CarryFlag { get; init; }
    public required bool ParityFlag { get; init; }
    public required bool AuxiliaryFlag { get; init; }
    public required bool ZeroFlag { get; init; }
    public required bool SignFlag { get; init; }
    public required bool DirectionFlag { get; init; }
    public required bool OverflowFlag { get; init; }
    public required bool InterruptFlag { get; init; }
}

/// <summary>
/// Response for memory read operation.
/// </summary>
public sealed record MemoryReadResponse {
    /// <summary>
    /// Gets the starting address that was read.
    /// </summary>
    public required uint Address { get; init; }
    
    /// <summary>
    /// Gets the number of bytes that were read.
    /// </summary>
    public required int Length { get; init; }
    
    /// <summary>
    /// Gets the memory data as a hexadecimal string.
    /// </summary>
    public required string Data { get; init; }
}

/// <summary>
/// Response for function list query.
/// </summary>
public sealed record FunctionListResponse {
    /// <summary>
    /// Gets the array of functions.
    /// </summary>
    public required FunctionInfo[] Functions { get; init; }
    
    /// <summary>
    /// Gets the total count of functions in the catalogue.
    /// </summary>
    public required int TotalCount { get; init; }
}

/// <summary>
/// Information about a single function.
/// </summary>
public sealed record FunctionInfo {
    /// <summary>
    /// Gets the function address as a string.
    /// </summary>
    public required string Address { get; init; }
    
    /// <summary>
    /// Gets the function name.
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Gets the number of times this function was called.
    /// </summary>
    public required int CalledCount { get; init; }
    
    /// <summary>
    /// Gets whether this function has a C# override.
    /// </summary>
    public required bool HasOverride { get; init; }
}
