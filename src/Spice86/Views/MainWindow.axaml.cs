namespace Spice86.Views;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

using Spice86.ViewModels;

using System;

/// <summary>
/// Represents main window.
/// </summary>
internal partial class MainWindow : Window {
    /// <summary>
    /// Initializes a new instance
    /// </summary>
    public MainWindow() {
        InitializeComponent();
        this.Menu.KeyDown += OnMenuKeyDown;
        this.Menu.KeyDown += OnMenuKeyUp;
        this.Menu.GotFocus += OnMenuGotFocus;
        this.Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        Dispatcher.UIThread.Post(() => {
            if (DataContext is MainWindowViewModel viewModel) {
                viewModel.StartEmulator();
            }
        }, DispatcherPriority.Background);
    }

    /// <summary>
    /// The performance view model property.
    /// </summary>
    public static readonly StyledProperty<PerformanceViewModel?> PerformanceViewModelProperty =
        AvaloniaProperty.Register<MainWindow, PerformanceViewModel?>(nameof(PerformanceViewModel),
            defaultValue: null);

    public PerformanceViewModel? PerformanceViewModel {
        get => GetValue(PerformanceViewModelProperty);
        set => SetValue(PerformanceViewModelProperty, value);
    }


    private void OnMenuGotFocus(object? sender, GotFocusEventArgs e) {
        FocusOnVideoBuffer();
        e.Handled = true;
    }

    private void OnMenuKeyUp(object? sender, KeyEventArgs e) {
        (DataContext as MainWindowViewModel)?.OnKeyUp(e);
        e.Handled = true;
    }

    private void OnMenuKeyDown(object? sender, KeyEventArgs e) {
        (DataContext as MainWindowViewModel)?.OnKeyDown(e);
        e.Handled = true;
    }

    private void FocusOnVideoBuffer() {
        Image.Focus();
    }

    /// <summary>
    /// Called when opened.
    /// </summary>
    /// <param name="e">The e.</param>
    protected override void OnOpened(EventArgs e) {
        base.OnOpened(e);
        if (DataContext is not MainWindowViewModel mainVm) {
            return;
        }
        mainVm.CloseMainWindow += (_, _) => Close();
    }

    /// <summary>
    /// Called when key up.
    /// </summary>
    /// <param name="e">The e.</param>
    protected override void OnKeyUp(KeyEventArgs e) {
        FocusOnVideoBuffer();
        var mainWindowViewModel = (DataContext as MainWindowViewModel);
        mainWindowViewModel?.OnKeyUp(e);
        e.Handled = true;
    }

    /// <summary>
    /// Called when key down.
    /// </summary>
    /// <param name="e">The e.</param>
    protected override void OnKeyDown(KeyEventArgs e) {
        FocusOnVideoBuffer();
        (DataContext as MainWindowViewModel)?.OnKeyDown(e);
        e.Handled = true;
    }

    /// <summary>
    /// Called when closing.
    /// </summary>
    /// <param name="e">The e.</param>
    protected override void OnClosing(WindowClosingEventArgs e) {
        (DataContext as MainWindowViewModel)?.OnMainWindowClosing();
        base.OnClosing(e);
    }

    /// <summary>
    /// Called when closed.
    /// </summary>
    /// <param name="e">The e.</param>
    protected override void OnClosed(EventArgs e) {
        (DataContext as IDisposable)?.Dispose();
        base.OnClosed(e);
    }
}