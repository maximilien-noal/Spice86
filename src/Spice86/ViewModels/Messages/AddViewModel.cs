namespace Spice86.ViewModels.Messages;

using System.ComponentModel;

/// <summary>
/// The i notify property changed.
/// </summary>
public record AddViewModelMessage<T>(T ViewModel) where T : INotifyPropertyChanged;