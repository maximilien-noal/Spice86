namespace Spice86.ViewModels.Messages;

/// <summary>
/// Represents the StatusMessage record.
/// </summary>
public record StatusMessage(DateTime Time, object Origin, string Message);