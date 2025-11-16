namespace Spice86.ViewModels.Messages;

using System.ComponentModel;

/// <summary>
/// Represents the RemoveViewModelMessage record.
/// </summary>
public record RemoveViewModelMessage<T>(T ViewModel) where T : INotifyPropertyChanged;