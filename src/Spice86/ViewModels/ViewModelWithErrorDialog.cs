namespace Spice86.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Spice86.ViewModels.Services;
using Spice86.ViewModels.ValueViewModels.Debugging;
using Spice86.Views.Behaviors;

using System.Text.Json;

/// <summary>
/// Represents view model with error dialog.
/// </summary>
public abstract partial class ViewModelWithErrorDialog : ViewModelBase {
    /// <summary>
    /// The _text clipboard.
    /// </summary>
    protected readonly ITextClipboard _textClipboard;
    /// <summary>
    /// The _ui dispatcher.
    /// </summary>
    protected readonly IUIDispatcher _uiDispatcher;

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="uiDispatcher">The ui dispatcher.</param>
    /// <param name="textClipboard">The text clipboard.</param>
    protected ViewModelWithErrorDialog(IUIDispatcher uiDispatcher,
        ITextClipboard textClipboard) {
        _uiDispatcher = uiDispatcher;
        _textClipboard = textClipboard;
    }

    /// <summary>
    /// The is dialog visible.
    /// </summary>
    [RelayCommand]
    public void ClearDialog() => IsDialogVisible = false;

    /// <summary>
    /// Performs the show internal debugger operation.
    /// </summary>
    /// <param name="commandParameter">The command parameter.</param>
    [RelayCommand]
    public void ShowInternalDebugger(object? commandParameter) {
        if (commandParameter is ShowInternalDebuggerBehavior showInternalDebuggerBehavior) {
            showInternalDebuggerBehavior.ShowInternalDebugger();
        }
    }

    /// <summary>
    /// Performs the show error operation.
    /// </summary>
    /// <param name="e">The e.</param>
    protected void ShowError(Exception e) {
        Exception = e.GetBaseException();
        IsDialogVisible = true;
    }
    [ObservableProperty]
    private bool _isDialogVisible;

    [ObservableProperty]
    private Exception? _exception;

    /// <summary>
    /// Performs the copy exception to clipboard operation.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    [RelayCommand]
    public async Task CopyExceptionToClipboard() {
        if (Exception is not null) {
            await _textClipboard.SetTextAsync(
                JsonSerializer.Serialize(
                    new ExceptionInfo(Exception.TargetSite?.ToString(), Exception.Message, Exception.StackTrace)));
        }
    }
}