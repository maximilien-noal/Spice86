namespace Spice86.ViewModels.ValueViewModels.Debugging;

/// <summary>
/// Represents the ExceptionInfo record.
/// </summary>
public record ExceptionInfo(string? TargetSite, string Message, string? StackTrace);