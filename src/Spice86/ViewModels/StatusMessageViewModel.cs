namespace Spice86.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

using Spice86.ViewModels.Messages;
using Spice86.ViewModels.Services;

/// <summary>
/// Represents status message view model.
/// </summary>
public partial class StatusMessageViewModel : ViewModelBase, IRecipient<StatusMessage> {
    private readonly IUIDispatcher _uiDispatcher;

    [ObservableProperty]
    private StatusMessage? _message;

    [ObservableProperty]
    private bool _isVisible;

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="dispatcher">The dispatcher.</param>
    /// <param name="messenger">The messenger.</param>
    public StatusMessageViewModel(IUIDispatcher dispatcher, IMessenger messenger) {
        messenger.Register(this);
        _uiDispatcher = dispatcher;
    }

    /// <summary>
    /// Performs the receive operation.
    /// </summary>
    /// <param name="message">The message.</param>
    public void Receive(StatusMessage message) {
        Message = message;
        IsVisible = true;
        Task.Delay(millisecondsDelay: 5000).ContinueWith(_ => {
            _uiDispatcher.Post(() => IsVisible = false);
        });
    }
}