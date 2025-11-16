namespace Spice86.ViewModels.Messages;

using System.ComponentModel;

/// <summary>
/// Represents the AddViewModelMessage record.
/// </summary>
public record AddViewModelMessage<T>(T ViewModel) where T : INotifyPropertyChanged;