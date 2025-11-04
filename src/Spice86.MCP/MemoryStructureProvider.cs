namespace Spice86.MCP;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.InterruptHandlers.Bios.Structures;
using Spice86.Core.Emulator.Memory;

/// <summary>
/// Provides access to memory-based emulator structures via composition.
/// Avoids having to pass individual structure instances as constructor parameters.
/// </summary>
public sealed class MemoryStructureProvider {
    /// <summary>
    /// Gets the BIOS Data Area structure.
    /// </summary>
    public BiosDataArea BiosDataArea { get; }
    
    /// <summary>
    /// Gets the memory interface.
    /// </summary>
    public IMemory Memory { get; }
    
    /// <summary>
    /// Gets the CPU state.
    /// </summary>
    public State State { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryStructureProvider"/> class.
    /// </summary>
    /// <param name="biosDataArea">The BIOS data area.</param>
    /// <param name="memory">The memory interface.</param>
    /// <param name="state">The CPU state.</param>
    public MemoryStructureProvider(BiosDataArea biosDataArea, IMemory memory, State state) {
        BiosDataArea = biosDataArea;
        Memory = memory;
        State = state;
    }
}
