namespace Spice86.ViewModels.Messages;

using System.ComponentModel;

/// <summary>
/// The i notify property changed.
/// </summary>
public record RemoveViewModelMessage<T>(T ViewModel) where T : INotifyPropertyChanged;