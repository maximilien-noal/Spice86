namespace Spice86.ViewModels.DataModels;

/// <summary>
/// Represents a memory map location from MemoryMap constants.
/// Used for quick navigation to important memory areas in the Memory View.
/// </summary>
public class MemoryMapItem {
    /// <summary>
    /// Gets or sets the display name of the memory location.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the segment address of the memory location.
    /// </summary>
    public required ushort Segment { get; init; }

    /// <summary>
    /// Gets or sets the offset within the segment.
    /// </summary>
    public ushort Offset { get; init; }

    /// <summary>
    /// Gets or sets the optional length of the memory region.
    /// </summary>
    public uint? Length { get; init; }

    /// <summary>
    /// Gets or sets a description of what this memory region contains.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Returns a string representation of the memory map item.
    /// </summary>
    public override string ToString() => Name;
}
